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
        readonly Dictionary<String, HashSet<String>> _statusToPropertyBindings = new Dictionary<String, HashSet<String>>();
        readonly Dictionary<Int32, HashSet<String>> _flagToPropertyBindings = new Dictionary<Int32, HashSet<String>>();

        public void OnFlagChanged(int index)
        {
            HashSet<String> properties;
            if (!_flagToPropertyBindings.TryGetValue(index, out properties)) return;
            foreach (var prop in properties) OnPropertyChanged(prop);
        }

        public void OnStatusChanged(string name)
        {
            HashSet<String> properties;
            if (!_statusToPropertyBindings.TryGetValue(name, out properties)) return;
            foreach (var prop in properties) OnPropertyChanged(prop);
        }

        void RegisterFlagDependency(int index, [CallerMemberName] string propertyName = null)
        {
            HashSet<String> properties;
            if (!_flagToPropertyBindings.TryGetValue(index, out properties))
            {
                properties = new HashSet<string>();
                _flagToPropertyBindings[index] = properties;
            }
            properties.Add(propertyName);
        }

        void RegisterStatusDependency(string name, [CallerMemberName] string propertyName = null)
        {
            HashSet<String> properties;
            if (!_statusToPropertyBindings.TryGetValue(name, out properties))
            {
                properties = new HashSet<string>();
                _statusToPropertyBindings[name] = properties;
            }
            properties.Add(propertyName);

        }

        void OnStatusCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (StatusVM status in e.OldItems) OnStatusChanged(status.Name);
            }
            if (e.NewItems != null)
            {
                foreach (StatusVM status in e.NewItems) OnStatusChanged(status.Name);
            }
        }



        int GetFlagInt(int index, [CallerMemberName] string propertyName = null)
        {
            RegisterFlagDependency(index, propertyName);
            return Flags[index].GetInt();
        }

        void SetFlag(int index, object value)
        {
            Flags[index].SetValue(value);
        }



        bool HasStatus(string name, [CallerMemberName] string propertyName = null)
        {
            RegisterStatusDependency(name, propertyName);
            return Statuses[name] != null;
        }

        int GetStatusInt(string name, string index, int defaultValue = 0, [CallerMemberName] string propertyName = null)
        {
            RegisterStatusDependency(name, propertyName);
            var obj = Statuses[name];
            if (obj == null) return defaultValue;
            return obj.GetInt("value" + index);
        }

        double GetStatusDouble(string name, string index, double defaultValue = 0, [CallerMemberName] string propertyName = null)
        {
            RegisterStatusDependency(name, propertyName);
            var obj = Statuses[name];
            if (obj == null) return defaultValue;
            return obj.GetDouble("value" + index);
        }

        void SetStatusValue(string statusName, string valueIndex, object value)
        {
            var obj = Statuses[statusName];
            obj.SetValue("value" + valueIndex, value);
        }

        void RemoveStatus(string name)
        {
            var status = Statuses[name];
            if (status == null) return;

            int index = Statuses.IndexOf(status);
            Statuses.Delete(index);
        }

        void EnsureStatusExists(string name, double defaultValue1, double defaultValue2, double defaultValue3, double defaultValue4)
        {
            var status = Statuses[name];
            if (status != null) return;

            Statuses.Create(name, defaultValue1, defaultValue2, defaultValue3, defaultValue4);
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
