using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class StatusVM : BindableBase
    {
        readonly XmlStatus _data;
        readonly AmfObject _statuses;
        readonly string _name;

        public StatusVM(AmfObject allStatuses, string name)
        {
            _data = XmlData.Instance.Statuses.FirstOrDefault(x => x.Name == name);
            _statuses = allStatuses;
            _name = name;

            GameProperties = new HashSet<string>();
        }

        public HashSet<string> GameProperties
        {
            get;
            private set;
        }

        public AmfObject Object
        {
            get { return _statuses.Select(x => x.ValueAsObject).FirstOrDefault(x => x.GetString("statusAffectName") == _name); }
        }

        public bool HasStatus
        {
            get { return Object != null; }
            set
            {
                var pair = _statuses.FirstOrDefault(x => x.ValueAsObject.GetString("statusAffectName") == _name);
                if ((pair != null) == value) return;

                if (value)
                {
                    var obj = new AmfObject(AmfTypes.Array);
                    obj["statusAffectName"] = _name;
                    obj["value1"] = 0;
                    obj["value2"] = 0;
                    obj["value3"] = 0;
                    obj["value4"] = 0;
                    _statuses.Push(obj);
                }
                else
                {
                    _statuses.Pop((int)pair.Key);
                }
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Comment
        {
            get { return _data == null ? "" : _data.Description; }
        }

        public Visibility CommentVisibility
        {
            get { return _data != null ? Visibility.Visible : Visibility.Collapsed; }
        }

        public double Value1
        {
            get { return GetDouble("value1"); }
            set { SetDoubleOrIntValue("value1", value); }
        }

        public double Value2
        {
            get { return GetDouble("value2"); }
            set { SetDoubleOrIntValue("value2", value); }
        }

        public double Value3
        {
            get { return GetDouble("value3"); }
            set { SetDoubleOrIntValue("value3", value); }
        }

        public double Value4
        {
            get { return GetDouble("value4"); }
            set { SetDoubleOrIntValue("value4", value); }
        }

        public double GetDouble(string name)
        {
            var obj = Object;
            if (obj == null) return 0;
            return obj.GetDouble(name);
        }

        public int GetInt(string name)
        {
            var obj = Object;
            if (obj == null) return 0;
            return obj.GetInt(name);
        }

        void SetDoubleOrIntValue(string key, double value, [CallerMemberName] string propertyName = null)
        {
            if (value == (int)value) SetValue(key, (int)value, propertyName);
            else SetValue(key, (double)value, propertyName);
        }

        public bool SetValue(object key, object value, [CallerMemberName] string propertyName = null)
        {
            var obj = Object;
            if (AmfObject.AreSame(obj[key], value)) return false;
            obj[key] = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            VM.Instance.NotifySaveRequiredChanged(true);
            VM.Instance.Game.OnStatusChanged(Name);
        }

        public bool Match(string str)
        {
            if (str == null || str.Length < 3) return true;

            int index = (Name ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            index = (Comment ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            return false;
        }

        bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                base.OnPropertyChanged();
                base.OnPropertyChanged("DetailsVisibility");
            }
        }

        public Visibility DetailsVisibility
        {
            get { return IsExpanded ? Visibility.Visible : Visibility.Collapsed; }
        }
    }
}
