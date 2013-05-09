using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public abstract class NamedVector4VM : BindableBase
    {
        readonly AmfObject _items;
        readonly HashSet<string> _gameProperties = new HashSet<string>();

        protected NamedVector4VM(AmfObject items)
        {
            _items = items;
        }

        public HashSet<string> GameProperties
        {
            get { return _gameProperties; }
        }

        public AmfObject GetObject()
        {
            return _items.Select(x => x.ValueAsObject).FirstOrDefault(x => IsObject(x));
        }

        public bool IsOwned
        {
            get { return GetObject() != null; }
            set
            {
                var pair = _items.FirstOrDefault(x => IsObject(x.ValueAsObject));
                if ((pair != null) == value) return;

                if (value)
                {
                    var obj = new AmfObject(AmfTypes.Array);
                    InitializeObject(obj);
                    _items.Push(obj);
                }
                else
                {
                    _items.Pop((int)pair.Key);
                }
                base.OnPropertyChanged("Value1");
                base.OnPropertyChanged("Value2");
                base.OnPropertyChanged("Value3");
                base.OnPropertyChanged("Value4");
                OnPropertyChanged();
            }
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

        public int GetInt(string name)
        {
            var obj = GetObject();
            if (obj == null) return 0;
            return obj.GetInt(name);
        }

        public double GetDouble(string name)
        {
            var obj = GetObject();
            if (obj == null) return 0;
            var value = obj.GetDouble(name);
            if (Double.IsNaN(value)) return 0.0;
            return value;
        }

        void SetDoubleOrIntValue(string key, double value, [CallerMemberName] string propertyName = null)
        {
            if (value == (int)value) SetValue(key, (int)value, propertyName);
            else SetValue(key, (double)value, propertyName);
        }

        public bool SetValue(object key, object value, [CallerMemberName] string propertyName = null)
        {
            var obj = GetObject();
            if (AmfObject.AreSame(obj[key], value)) return false;
            obj[key] = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            VM.Instance.NotifySaveRequiredChanged(true);
            NotifyGameVM();
        }

        protected abstract void InitializeObject(AmfObject obj);
        protected abstract bool IsObject(AmfObject obj);
        protected abstract void NotifyGameVM();
    }
}
