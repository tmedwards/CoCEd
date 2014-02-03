using System;
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
        // Whenever a FlagVM or StatusVM is modified, it notifies GameVM with those functions so that it updates its dependent properties. 
        // See also GetPerk,  GetFlag and GetStatus.
        public void OnPerkChanged(string name)
        {
            foreach (var prop in _allPerks.First(x => x.Name == name).GameVMProperties) OnPropertyChanged(prop);
        }

        public void OnFlagChanged(int index)
        {
            foreach(var prop in _allFlags[index].GameVMProperties) OnPropertyChanged(prop);
        }

        public void OnStatusChanged(string name)
        {
            foreach (var prop in _allStatuses.First(x => x.Name == name).GameVMProperties) OnPropertyChanged(prop);
        }


        /// <summary>
        /// Returns the flag with the specified index AND registers a dependency between the caller property and this flag. 
        /// That way, anytime the flag value is changed, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        FlagVM GetFlag(int index, [CallerMemberName] string propertyName = null)
        {
            var flag = _allFlags[index];
            flag.GameVMProperties.Add(propertyName);
            return flag;
        }

        /// <summary>
        /// Returns the status with the specified name (even if not owned by the player) AND registers a dependency between the caller property and this status.
        /// That way, anytime the status is modified, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        StatusVM GetStatus(string name, [CallerMemberName] string propertyName = null)
        {
            var status = _allStatuses.First(x => x.Name == name);
            status.GameVMProperties.Add(propertyName);
            return status;
        }

        /// <summary>
        /// Returns the perk with the specified name (even if not owned by the player) AND registers a dependency between the caller property and this perk.
        /// That way, anytime the perk is modified, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        PerkVM GetPerk(string name, [CallerMemberName] string propertyName = null)
        {
            var perk = _allPerks.First(x => x.Name == name);
            perk.GameVMProperties.Add(propertyName);
            return perk;
        }

        bool IsMale
        {
            get { return GetInt("gender", 0) <= 1; }
        }

        public void OnPerkAddedOrRemoved(string name, bool isOwned)
        {
            // Grants/removes the player the appropriate bonuses when a perk is added or removed.
            // We do not add stats however since the user can already change them easily.
            if (name == "Strong Back")
            {
                GetObj("itemSlot4")["unlocked"] = isOwned;
                UpdateInventory();
                ItemContainers.Update();
            }
            else if (name == "Strong Back 2: Strong Harder")
            {
                GetObj("itemSlot5")["unlocked"] = isOwned;
                UpdateInventory();
                ItemContainers.Update();
            }
#if PRE_SAVE_REFACTOR
            else if (name == "Weapon Mastery")
            {
                var modifier = isOwned ? 2.0 : 0.5;
                if (GetString("weaponPerk") == "Large") SetDouble("weaponAttack", GetDouble("weaponAttack") * modifier);
            }
            else if (name == "Agility")
            {
                UpdateArmorDef();
            }
#endif
        }

        public void OnKeyItemAddedOrRemoved(string name, bool isOwned)
        {
            // Creates/destroys the corresponding item slots when a container is added/removed.
            if (name == "Camp - Chest")
            {
                var array = GetObj("itemStorage");
                if (isOwned)
                {
                    while (array.DenseCount < 6)
                    {
                        var slot = new AmfObject(AmfTypes.Object);
#if !PRE_SAVE_REFACTOR
                        slot["id"] = "";
                        slot["quantity"] = 0;
                        slot["unlocked"] = true;
#else
                        slot["unlocked"] = true;
                        slot["shortName"] = "";
                        slot["quantity"] = 0;
#endif
                        array.Push(slot);
                    }
                }
                else
                {
                    while (array.DenseCount > 0) array.Pop(array.DenseCount - 1);
                }
                UpdateChest();
                ItemContainers.Update();
            }
            else if (name == "Equipment Rack - Armor")
            {
                GetFlag(255).SetValue(isOwned ? 1 : 0);
                UpdateArmorRack();
                ItemContainers.Update();
            }
            else if (name == "Equipment Rack - Weapons")
            {
                GetFlag(254).SetValue(isOwned ? 1 : 0);
                UpdateWeaponRack();
                ItemContainers.Update();
            }
        }

        void UpdateInventory()
        {
            _inventory.Clear();
            for (int i = 0; i < 5; i++)
            {
                var slot = GetObj("itemSlot" + (i + 1));
                if (slot.GetBool("unlocked")) _inventory.Add(slot);
            }
        }

        void UpdateChest()
        {
            _chest.Clear();
            foreach (var pair in GetObj("itemStorage")) _chest.Add(pair.ValueAsObject);
        }

        void UpdateArmorRack()
        {
            _armorRack.Clear();
            if (GetFlag(255).AsInt() == 1)
            {
                var gearStorage = GetObj("gearStorage");
                for (int i = 0; i < 9; i++) _armorRack.Add(gearStorage.GetObj(i + 9));
            }
        }

        void UpdateWeaponRack()
        {
            _weaponRack.Clear();
            if (GetFlag(254).AsInt() == 1)
            {
                var gearStorage = GetObj("gearStorage");
                for (int i = 0; i < 9; i++) _weaponRack.Add(gearStorage.GetObj(i));
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
#if !PRE_SAVE_REFACTOR
                    return String.Compare(obj1.GetString("id"), obj2.GetString("id"));
#else
                    return String.Compare(obj1.GetString("perkName"), obj2.GetString("perkName"));
#endif
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
