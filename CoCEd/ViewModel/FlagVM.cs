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
        readonly AmfObject _flagArray;
        readonly GameVM _game;

        readonly AmfTrait _valueTrait;
        string _valueLabel;

        public FlagVM(GameVM game, AmfObject flags, XmlEnum data, int index)
        {
            _flagArray = flags;
            _game = game;
            _index = index;
            _label = data != null ? data.Name : "";
            _comment = data != null ? data.Description : "";
            if (!String.IsNullOrEmpty(_comment)) _label = _label + "\u202F*";

            var value = flags[_index];
            if (value is AmfObject) _valueTrait = ((AmfObject)value).Trait;
            _valueLabel = flags.GetString(_index);

            GameVMProperties = new HashSet<string>();
        }

        public HashSet<string> GameVMProperties
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
            return _flagArray.GetInt(_index, 0);
        }

        public string Label
        {
            get { return _label; }
        }

        public string Comment
        {
            get { return _comment; }
        }

        public bool IsEnum
        {
            get { return _valueTrait != null && _valueTrait.IsEnum; }
        }

        public string ValueLabel
        {
            get { return _valueLabel; }
            set
            {
                if (_valueLabel == value) return;
                _valueLabel = value;
                OnPropertyChanged();

                object transcriptedValue = GetValueFromLabel(value);
                SetValue(transcriptedValue, false);
            }
        }

        object GetValueFromLabel(string value)
        {
            int iValue;
            if (IsEnum)
            {
                if (Int32.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out iValue)) return ConvertIntegerToEnum(iValue);
                if (Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out iValue)) return ConvertIntegerToEnum(iValue);
                return ConvertIntegerToEnum(0);
            }

            if (Int32.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out iValue)) return iValue;
            if (Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out iValue)) return iValue;

            if (Double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out double fValue)) return iValue;
            if (Double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out fValue)) return iValue;

            if (String.Equals(value, "true", StringComparison.InvariantCulture)) return true;
            if (String.Equals(value, "false", StringComparison.InvariantCulture)) return false;

            if (value == "<null>") return AmfNull.Instance;
            return value;
        }

        public bool SetValue(object value, bool updateText = true)
        {
            // Update value
            if (!SetValue(_flagArray, _index, value)) return false;

            // Update label
            if (updateText)
            {
                SetProperty(ref _valueLabel, value.ToString(), "ValueLabel");
            }
            return true;
        }

        AmfObject ConvertIntegerToEnum(int value)
        {
            var result = new AmfObject(AmfTypes.Object)
            {
                Trait = _valueTrait,
                ["value"] = value,
            };
            return result;
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

        protected override void OnSavePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnSavePropertyChanged(propertyName);
            _game.OnFlagChanged(Index);
        }
    }
}
