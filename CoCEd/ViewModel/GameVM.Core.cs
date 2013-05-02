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
        public void OnFlagChanged(int index)
        {
            foreach(var prop in _allFlags[index].GameProperties) OnPropertyChanged(prop);
        }

        public void OnStatusChanged(string name)
        {
            foreach (var prop in _allStatuses.First(x => x.Name == name).GameProperties) OnPropertyChanged(prop);
        }

        void RegisterFlagDependency(int index, [CallerMemberName] string propertyName = null)
        {
            _allFlags[index].GameProperties.Add(propertyName);
        }

        void RegisterStatusDependency(string name, [CallerMemberName] string propertyName = null)
        {
            _allStatuses.First(x => x.Name == name).GameProperties.Add(propertyName);
        }



        int GetFlagInt(int index, [CallerMemberName] string propertyName = null)
        {
            RegisterFlagDependency(index, propertyName);
            return _allFlags[index].GetInt();
        }

        void SetFlag(int index, object value)
        {
            _allFlags[index].SetValue(value);
        }



        bool HasStatus(string name, [CallerMemberName] string propertyName = null)
        {
            RegisterStatusDependency(name, propertyName);
            return _allStatuses.First(x => x.Name == name).HasStatus;
        }

        int GetStatusInt(string name, string index, int defaultValue = 0, [CallerMemberName] string propertyName = null)
        {
            RegisterStatusDependency(name, propertyName);
            var status = _allStatuses.First(x => x.Name == name);
            if (!status.HasStatus) return defaultValue;
            return status.GetInt("value" + index);
        }

        double GetStatusDouble(string name, string index, double defaultValue = 0, [CallerMemberName] string propertyName = null)
        {
            RegisterStatusDependency(name, propertyName);
            var status = _allStatuses.First(x => x.Name == name);
            if (!status.HasStatus) return defaultValue;
            return status.GetDouble("value" + index);
        }

        void SetStatusValue(string statusName, string valueIndex, object value)
        {
            var status = _allStatuses.First(x => x.Name == statusName);
            status.SetValue("value" + valueIndex, value);
        }

        void RemoveStatus(string name)
        {
            var status = _allStatuses.First(x => x.Name == name);
            status.HasStatus = false;
        }

        void EnsureStatusExists(string name, double defaultValue1, double defaultValue2, double defaultValue3, double defaultValue4)
        {
            var status = _allStatuses.First(x => x.Name == name);
            if (status.HasStatus) return;

            status.HasStatus = true;
            status.Value1 = defaultValue1;
            status.Value2 = defaultValue2;
            status.Value3 = defaultValue3;
            status.Value4 = defaultValue4;
        }



        void OnGenitalCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Cocks.Count != 0 && Vaginas.Count != 0) SetValue("gender", 3);
            else if (Vaginas.Count != 0) SetValue("gender", 2);
            else if (Cocks.Count != 0) SetValue("gender", 1);
            else SetValue("gender", 0);

            OnPropertyChanged("NippleVisibility");
            OnPropertyChanged("ClitVisibility");
        }
    }
}
