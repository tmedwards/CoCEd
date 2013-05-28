using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public enum PiercingLocation
    {
        Eyebrow,
        Tongue,
        Nose,
        Ears,
        Lip,
        Cock,
        Nipples,
        Clitoris,
        Labia,
    }

    public sealed class PiercingVM : ObjectVM
    {
        readonly string _prefix;
        readonly PiercingLocation _location;

        public PiercingVM(AmfObject obj, string prefix, PiercingLocation location)
            : base(obj)
        {
            _prefix = prefix;
            _location = location;
        }

        public IEnumerable<XmlEnum> AllTypes
        {
            get 
            {
                foreach (var type in XmlData.Instance.Body.PiercingTypes)
                {
                    var type2 = new XmlEnum { ID = type.ID, Name = type.Name };
                    switch (type.ID)
                    {
                        // Ladder
                        case 3:
                            type2.IsGrayedOut = (_location != PiercingLocation.Cock);
                            break;

                        // Hoop
                        case 4:
                            type2.IsGrayedOut = (_location != PiercingLocation.Ears);
                            break;

                        // Chain
                        case 5:
                            type2.IsGrayedOut = (_location != PiercingLocation.Nipples);
                            break;
                    }
                    yield return type2;
                }
            }
        }

        public IEnumerable<String> SuggestedNames
        {
            get 
            {
                foreach (var material in XmlData.Instance.Body.PiercingMaterials)
                {
                    yield return GeneratePiercingName(Type, material.ID);
                }
            }
        }

        public int Type
        {
            get { return GetInt(_prefix == "" ? "pierced" : _prefix + "Pierced"); }
            set
            {
                SetValue(_prefix == "" ? "pierced" : _prefix + "Pierced", value);
                OnPropertyChanged("Label");
                OnPropertyChanged("CanEditName");
                OnPropertyChanged("SuggestedNames");
            }
        }

        string GeneratePiercingName(int type, int material)
        {
            if (type == 0 || material == 0) return "";

            //location
            string rest = " ";
            if (_location == PiercingLocation.Cock)
            {
                rest += type == 3 ? "" : "cock-";
            }
            else if (_location == PiercingLocation.Nipples)
            {
                rest += "nipple-";
            }
            else if (_location == PiercingLocation.Ears)
            {
                rest += "ear";
            }
            else
            {
                rest += _prefix + "-";
            }

            //type
            if (type == 3)
            {
                rest += "jacob's ladder";
            }
            else if (type == 5)
            {
                rest += "chain";
            }
            else
            {
                var xmlType = XmlData.Instance.Body.PiercingTypes.FirstOrDefault(x => x.ID == Type);
                if (xmlType != null) rest += xmlType.Name.ToLower();
                if (_prefix == "ears" || _prefix == "nipples" || _prefix == "labia")
                {
                    rest += "s";
                }
            }

            var xmlMaterial = XmlData.Instance.Body.PiercingMaterials.FirstOrDefault(x => x.ID == material);
            return xmlMaterial.Name + rest;
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
