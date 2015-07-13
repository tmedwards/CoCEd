﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed partial class GameVM : ObjectVM
    {
        /// <summary>
        /// Returns the status with the specified name (even if not owned by the player) AND registers a dependency between the caller property and this status.
        /// That way, anytime the status is modified, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        public StatusVM GetStatus(string name, [CallerMemberName] string propertyName = null)
        {
            var status = _allStatuses.First(x => x.Name == name);
            status.GameVMProperties.Add(propertyName);
            return status;
        }

        /// <summary>
        /// Returns the key item with the specified name (even if not owned by the player) AND registers a dependency between the caller property and this key item.
        /// That way, anytime the key item is modified, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        public KeyItemVM GetKeyItem(string name, [CallerMemberName] string propertyName = null)
        {
            var keyItem = _allKeyitems.First(x => x.Name == name);
            keyItem.GameVMProperties.Add(propertyName);
            return keyItem;
        }

        /// <summary>
        /// Returns the perk with the specified name (even if not owned by the player) AND registers a dependency between the caller property and this perk.
        /// That way, anytime the perk is modified, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        public PerkVM GetPerk(string name, [CallerMemberName] string propertyName = null)
        {
            var perk = _allPerks.First(x => x.Name == name);
            perk.GameVMProperties.Add(propertyName);
            return perk;
        }

        /// <summary>
        /// Returns the flag with the specified index AND registers a dependency between the caller property and this flag. 
        /// That way, anytime the flag value is changed, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        public FlagVM GetFlag(int index, [CallerMemberName] string propertyName = null)
        {
            var flag = _allFlags[index];
            flag.GameVMProperties.Add(propertyName);
            return flag;
        }

        bool IsMale
        {
            get { return GetInt("gender", 0) <= 1; }
        }

        // Public helper for the various subordinate body part view models (e.g. CockVM)
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(propertyName);
        }

        // Whenever a PerkVM, FlagVM, or StatusVM is modified, it notifies GameVM with those functions so that it updates its dependent properties. 
        // See also GetPerk, GetFlag, and GetStatus.
        public void OnPerkChanged(string name)
        {
            // Must be here, rather than in OnPerkAddedOrRemoved(), to catch property value changes.
            if (name == "Milk Maid")
            {
                foreach (var breast in Breasts) breast.UpdateMilkVolume();
            }

            foreach (var prop in _allPerks.First(x => x.Name == name).GameVMProperties) OnPropertyChanged(prop);
        }

        public void OnFlagChanged(int index)
        {
            if (index == 2008) // CAMP_CABIN_FURNITURE_DRESSER
            {
                UpdateDresser();
                ItemContainers.Update();
            }

            foreach(var prop in _allFlags[index].GameVMProperties) OnPropertyChanged(prop);
        }

        public void OnStatusChanged(string name)
        {
            foreach (var prop in _allStatuses.First(x => x.Name == name).GameVMProperties) OnPropertyChanged(prop);
        }

        public void OnKeyItemChanged(string name)
        {
            // Must be here, rather than in OnKeyItemAddedOrRemoved(), to catch property value changes.
            if (name == "Backpack") // itemSlot# [6, 10]
            {
                var backpack = GetKeyItem("Backpack");
                for (int i = 0; i < 5; i++) GetObj("itemSlot" + (i + 6))["unlocked"] = false;
                if (backpack.IsOwned)
                {
                    int count = backpack.GetInt("value1");
                    if (count < 1 || count > 5)
                    {
                        count = Math.Max(1, Math.Min(5, count)); // clamp value to [1, 5], so CoC-Revamp-Mod doesn't assplode
                        backpack.Value1 = count;
                    }
                    for (int i = 0; i < count; i++) GetObj("itemSlot" + (i + 6))["unlocked"] = true;
                }
                UpdateInventory();
                ItemContainers.Update();
            }

            foreach (var prop in _allKeyitems.First(x => x.Name == name).GameVMProperties) OnPropertyChanged(prop);
        }

        public void OnPerkAddedOrRemoved(string name, bool isOwned)
        {
            // Grants/removes the player the appropriate bonuses when a perk is added or removed.
            // We do not add stats however since the user can already change them easily.
            switch (name)
            {
                case "Feeder":
                    GetStatus(name).IsOwned = isOwned;
                    break;

                case "Strong Back":
                    GetObj("itemSlot4")["unlocked"] = isOwned;
                    UpdateInventory();
                    ItemContainers.Update();
                    break;

                case "Strong Back 2: Strong Harder":
                    GetObj("itemSlot5")["unlocked"] = isOwned;
                    UpdateInventory();
                    ItemContainers.Update();
                    break;
            }
        }

        public void OnKeyItemAddedOrRemoved(string name, bool isOwned)
        {
            // Creates/destroys the corresponding item slots when a container is added/removed.
            switch (name)
            {
                case "Camp - Chest":
                case "Camp - Murky Chest":
                case "Camp - Ornate Chest":
                    if (IsRevampMod || name == "Camp - Chest")
                    {
                        var array = GetObj("itemStorage"); // max chest slots are 6 in CoC and 14 in CoC-Revamp-Mod
                        int count = name == "Camp - Chest" ? 6 : 4; // the CoC-Revamp-Mod chests add 4 slots a piece
                        if (isOwned)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var slot = new AmfObject(AmfTypes.Object);
                                slot["id"] = "NOTHING!";  // having to set this to "NOTHING!" is daft
                                slot["quantity"] = 0;
                                slot["unlocked"] = false; // must now be false or the camp chest will break in-game
                                array.Push(slot);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < count; i++) array.Pop(array.DenseCount - 1);
                        }
                        UpdateChest();
                        ItemContainers.Update();
                    }
                    break;

                case "Equipment Rack - Weapons":
                    GetFlag(254).SetValue(isOwned ? 1 : 0);
                    UpdateWeaponRack();
                    ItemContainers.Update();
                    break;

                case "Equipment Rack - Armor":
                    GetFlag(255).SetValue(isOwned ? 1 : 0);
                    UpdateArmorRack();
                    ItemContainers.Update();
                    break;

                case "Equipment Storage - Jewelry Box":
                    UpdateJewelryBox();
                    ItemContainers.Update();
                    break;

                case "Equipment Rack - Shields":
                    UpdateShieldRack();
                    ItemContainers.Update();
                    break;
            }
        }

        void UpdateInventory()
        {
            _inventory.Clear();
            int count = IsRevampMod ? 10 : 5; // max inventory slots are 5 in CoC and 10 in CoC-Revamp-Mod
            for (int i = 0; i < count; i++)
            {
                var slot = GetObj("itemSlot" + (i + 1));
                if (slot != null && slot.GetBool("unlocked")) _inventory.Add(slot);
            }
        }

        void UpdateChest()
        {
            _chest.Clear();
            foreach (var pair in GetObj("itemStorage")) _chest.Add(pair.ValueAsObject);
        }

        void UpdateWeaponRack() // gearStorage [0, 8]
        {
            _weaponRack.Clear();
            bool hasWeaponRack = IsRevampMod ? GetKeyItem("Equipment Rack - Weapons").IsOwned : GetFlag(254).AsInt() == 1;
            if (hasWeaponRack)
            {
                var gearStorage = GetObj("gearStorage");
                for (int i = 0; i < 9; i++) _weaponRack.Add(gearStorage.GetObj(i));
            }
        }

        void UpdateArmorRack() // gearStorage [9, 17]
        {
            _armorRack.Clear();
            bool hasArmorRack = IsRevampMod ? GetKeyItem("Equipment Rack - Armor").IsOwned : GetFlag(255).AsInt() == 1;
            if (hasArmorRack)
            {
                var gearStorage = GetObj("gearStorage");
                for (int i = 0; i < 9; i++) _armorRack.Add(gearStorage.GetObj(i + 9));
            }
        }

        void UpdateJewelryBox() // gearStorage [18, 26]
        {
            _jewelryBox.Clear();
            if (GetKeyItem("Equipment Storage - Jewelry Box").IsOwned)
            {
                var gearStorage = GetObj("gearStorage");
                for (int i = 0; i < 9; i++) _jewelryBox.Add(gearStorage.GetObj(i + 18));
            }
        }

        void UpdateDresser() // gearStorage [27, 35]
        {
            _dresser.Clear();
            if (GetFlag(2008).AsInt() == 1)
            {
                var gearStorage = GetObj("gearStorage");
                for (int i = 0; i < 9; i++) _dresser.Add(gearStorage.GetObj(i + 27));
            }
        }

        void UpdateShieldRack() // gearStorage [36, 44]
        {
            _shieldRack.Clear();
            if (GetKeyItem("Equipment Rack - Shields").IsOwned)
            {
                var gearStorage = GetObj("gearStorage");
                for (int i = 0; i < 9; i++) _shieldRack.Add(gearStorage.GetObj(i + 36));
            }
        }

        void OnGenitalCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Update gender when cocks or vaginas are added/removed.
            if (Cocks.Count != 0 && Vaginas.Count != 0) SetValue("gender", 3);
            else if (Vaginas.Count != 0) SetValue("gender", 2);
            else if (Cocks.Count != 0) SetValue("gender", 1);
            else SetValue("gender", 0);

            OnPropertyChanged("NippleVisibility");
            OnPropertyChanged("ClitVisibility");
        }

        public void BeforeSerialization()
        {
            _obj.GetObj("perks").SortDensePart((x, y) =>
                {
                    var obj1 = x as AmfObject;
                    var obj2 = y as AmfObject;
                    return String.Compare(obj1.GetString("id"), obj2.GetString("id"));
                });

            _obj.GetObj("keyItems").SortDensePart((x, y) =>
            {
                var obj1 = x as AmfObject;
                var obj2 = y as AmfObject;
                return String.Compare(obj1.GetString("keyName"), obj2.GetString("keyName"));
            });
        }
    }
}
