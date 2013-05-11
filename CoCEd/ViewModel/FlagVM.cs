using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
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
            if (!String.IsNullOrEmpty(_comment)) _label = _label + "*";
            _description = flags.GetString(_index);

            GameProperties = new HashSet<string>();
        }

        public HashSet<string> GameProperties
        {
            get;
            private set;
        }

        public int Index
        {
            get { return _index; }
        }

        public int AsInt()
        {
            return _obj.GetInt(_index);
        }

        public string Label
        {
            get { return _label; }
        }

        public string Comment
        {
            get { return _comment; }
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
            if (Int32.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out iValue)) return iValue;
            if (Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out iValue)) return iValue;

            double fValue;
            if (Double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out fValue)) return iValue;
            if (Double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out fValue)) return iValue;

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

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            VM.Instance.NotifySaveRequiredChanged(true);
            VM.Instance.Game.OnFlagChanged(Index);
        }

        public bool Match(string str)
        {
            if (str == null || str.Length < 3) return true;

            int index = (Label ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            index = (Comment ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            return false;
        }
    }
}
