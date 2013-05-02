using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class StatusesVM : ArrayVM<StatusVM>
    {
        public StatusesVM(AmfObject array)
            : base(array, x => new StatusVM(x))
        {
        }

        public StatusVM this[string name]
        {
            get { return this.FirstOrDefault(x => x.Name == name); }
        }

        public StatusVM Create(string name, object defaultValue1, object defaultValue2, object defaultValue3, object defaultValue4)
        {
            var obj = new AmfObject(AmfTypes.Array);
            obj["statusAffectName"] = name;
            obj["value1"] = defaultValue1;
            obj["value2"] = defaultValue2;
            obj["value3"] = defaultValue3;
            obj["value4"] = defaultValue4;
            return Add(obj);
        }

        protected override AmfObject CreateNewObject()
        {
            var obj = new AmfObject(AmfTypes.Array);
            obj["statusAffectName"] = "";
            obj["value1"] = 0;
            obj["value2"] = 0;
            obj["value3"] = 0;
            obj["value4"] = 0;
            return obj;
        }
    }


    public sealed class StatusVM : ObjectVM
    {
        readonly XmlEnumWithStringID _data;

        public StatusVM(AmfObject obj)
            : base(obj)
        {
            var name = Name;
            _data = XmlData.Instance.Statuses.FirstOrDefault(x => x.Name == name);
        }

        public string Name
        {
            get { return GetString("statusAffectName"); }
            set 
            { 
                if (Name == value) return;
                VM.Instance.Game.OnStatusChanged(Name);
                base.SetValue("statusAffectName", value);
                VM.Instance.Game.OnStatusChanged(value);
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

        void SetDoubleOrIntValue(string key, double value, [CallerMemberName] string propertyName = null)
        {
            if (value == (int)value) SetValue(key, (int)value, propertyName);
            else SetValue(key, (double)value, propertyName);
        }

        public override bool SetValue(object key, object value, string propertyName = null)
        {
            if (!base.SetValue(key, value, propertyName)) return false;
            VM.Instance.Game.OnStatusChanged(Name);
            return true;
        }
    }
}
