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
        // Whenever a FlagVM or StatusVM is modified, it notifies GameVM so that it updates its dependent properties.
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


        FlagVM GetFlag(int index, [CallerMemberName] string propertyName = null)
        {
            var flag = _allFlags[index];
            flag.GameVMProperties.Add(propertyName);
            return flag;
        }

        StatusVM GetStatus(string name, [CallerMemberName] string propertyName = null)
        {
            var status = _allStatuses.First(x => x.Name == name);
            status.GameVMProperties.Add(propertyName);
            return status;
        }

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
            // We do not care about perks that add stats since the user can already change them easily.
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
            else if (name == "Weapon Mastery")
            {
                var modifier = isOwned ? 2.0 : 0.5;
                if (GetString("weaponPerk") == "Large") SetDouble("weaponAttack", GetDouble("weaponAttack") * modifier);
            }
            else if (name == "Agility")
            {
                UpdateArmorDef();
            }
        }

        public void OnKeyItemAddedOrRemoved(string name, bool isOwned)
        {
            if (name == "Camp - Chest")
            {
                var array = GetObj("itemStorage");
                if (isOwned)
                {
                    while (array.DenseCount < 6)
                    {
                        var slot = new AmfObject(AmfTypes.Object);
                        slot["unlocked"] = true;
                        slot["shortName"] = "";
                        slot["quantity"] = 0;
                        array.Push(slot);
                    }
                }
                else
                {
                    while (array.DenseCount > 0) array.Pop(array.DenseCount - 1);
                }
                UpdateChest();
                ItemContainers.Update();
                if (isOwned) foreach (var slot in _chest.Slots) slot.CreateGroups();
            }
            else if (name == "Equipment Rack - Armor")
            {
                GetFlag(255).SetValue(isOwned ? 1 : 0);
                UpdateArmorRack();
                ItemContainers.Update();
                if (isOwned) foreach (var slot in _armorRack.Slots) slot.CreateGroups();
            }
            else if (name == "Equipment Rack - Weapons")
            {
                GetFlag(254).SetValue(isOwned ? 1 : 0);
                UpdateWeaponRack();
                ItemContainers.Update();
                if (isOwned) foreach (var slot in _weaponRack.Slots) slot.CreateGroups();
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
            // Update gender
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
                    return String.Compare(obj1.GetString("perkName"), obj2.GetString("perkName"));
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
