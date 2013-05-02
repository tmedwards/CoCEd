using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class PiercingVM : ObjectVM
    {
        readonly string _prefix;

        public PiercingVM(AmfObject obj, string prefix)
            : base(obj)
        {
            _prefix = prefix;
        }

        public IEnumerable<XmlEnum> AllTypes
        {
            get { return XmlData.Instance.Body.PiercingTypes; }
        }

        public int Type
        {
            get { return GetInt(_prefix == "" ? "pierced" : _prefix + "Pierced"); }
            set
            {
                SetValue(_prefix == "" ? "pierced" : _prefix + "Pierced", value);
                OnPropertyChanged("CanEditName");
                OnPropertyChanged("Label");
            }
        }

        public string UpperName
        {
            get { return GetString(_prefix == "" ? "pLong" : _prefix + "PLong"); }
            set
            {
                SetValue(_prefix == "" ? "pLong" : _prefix + "PLong", value);
                OnPropertyChanged("Label");
            }
        }

        public string LowerName
        {
            get { return GetString(_prefix == "" ? "pShort" : _prefix + "PShort"); }
            set
            {
                SetValue(_prefix == "" ? "pShort" : _prefix + "PShort", value);
                OnPropertyChanged("Label");
            }
        }

        public string Label
        {
            get
            {
                if (Type == 0) return "None";

                if (!String.IsNullOrEmpty(UpperName)) return UpperName;
                if (!String.IsNullOrEmpty(LowerName)) return LowerName;

                var xmlType = XmlData.Instance.Body.PiercingTypes.FirstOrDefault(x => x.ID == Type);
                if (xmlType != null) return xmlType.Name;

                return "<unknown>";
            }
        }

        public bool CanEditName
        {
            get { return Type != 0; }
        }
    }
}
