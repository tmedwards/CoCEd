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
