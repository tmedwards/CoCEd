using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class FlagVM : BindableBase
    {
        readonly int _index;
        readonly string _label;
        readonly string _comment;
        readonly AmfObject _obj;

        public FlagVM(AmfObject flags, XmlEnum data, int index)
        {
            _obj = flags;
            _index = index;
            _label = data != null ? data.Name : "";
            _comment = data != null ? data.Description : "";
            _description = flags.GetString(_index);
        }

        public int Index
        {
            get { return _index; }
        }

        public int GetInt()
        {
            return _obj.GetInt(_index);
        }

        string _description;
        public string ValueLabel
        {
            get { return _description; }
            set
            {
                if (_description == value) return;
                _description = value;
                OnPropertyChanged();

                object transcriptedValue = GetValueFromLabel(value);
                SetValue(transcriptedValue, false);
            }
        }

        object GetValueFromLabel(string value)
        {
            int iValue;
            if (Int32.TryParse(value, out iValue)) return iValue;

            double fValue;
            if (Double.TryParse(value, out fValue)) return fValue;

            if (String.Equals(value, "true", StringComparison.InvariantCulture)) return true;
            if (String.Equals(value, "false", StringComparison.InvariantCulture)) return false;

            if (value == "<null>") return AmfNull.Instance;
            return value;
        }

        public bool SetValue(object value, bool updateText = true)
        {
            // Update value
            object currentValue = _obj[_index];
            if (AmfObject.AreSame(currentValue, value)) return false;
            _obj[_index] = value;

            // Notify subscribers
            VM.Instance.Game.OnFlagChanged(Index);
            if (!updateText) return true;

            // Update label
            var newDescription = value.ToString();
            if (_description == newDescription) return true;
            _description = newDescription;

            // Notify subscribers
            OnPropertyChanged("ValueLabel");
            return true;
        }
    }
}
