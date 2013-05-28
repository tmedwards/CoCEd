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
            get { return XmlData.Instance.Body.PiercingTypes; }
        }

        public IEnumerable<XmlEnum> AllMaterials
        {
            get { return XmlData.Instance.Body.PiercingMaterials; }
        }

        public string AllowedTypes
        {
            get
            {
                switch(_location)
                {
                    case PiercingLocation.Cock:
                        return "Game-allowed cock piercing types are stud, ring, and ladder.";
                    case PiercingLocation.Clitoris:
                        return "Game-allowed clit piercing types are stud and ring.";
                    case PiercingLocation.Ears:
                        return "Game-allowed ear piercing types are stud, ring, and hoop.";
                    case PiercingLocation.Eyebrow:
                        return "Game-allowed eyebrow piercing types are stud and ring.";
                    case PiercingLocation.Lip:
                        return "Game-allowed lip piercing types are stud and ring.";
                    case PiercingLocation.Nose:
                        return "Game-allowed nose piercing types are stud and ring.";
                    case PiercingLocation.Tongue:
                        return "Game-allowed tongue piercing type is stud.";
                    case PiercingLocation.Nipples:
                        return "Game-allowed nipple piercing types are stud, ring, and chain.";
                    case PiercingLocation.Labia:
                        return "Game-allowed labia piercing types are stud and ring.";
                    default:
                        return "This PiercingVM did not have a prefix passed to its constructor. Please post about this on the forum.";
                }
            }
        }

        public int Type
        {
            get { return GetInt(_prefix == "" ? "pierced" : _prefix + "Pierced"); }
            set
            {
                SetValue(_prefix == "" ? "pierced" : _prefix + "Pierced", value);
                if (value != 0 && Material != 0) GeneratePiercingName(value, Material);
                OnPropertyChanged("CanEditName");
                OnPropertyChanged("Label");
            }
        }

        public int Material
        {
            get 
            {
                string name = GetString(_prefix == "" ? "pLong" : _prefix + "PLong");
                if (String.IsNullOrEmpty(name)) return 0;

                string material = name.Substring(0, name.IndexOf(' ')); //does not work well for Ceraph's piercings
                var xmlMaterial = XmlData.Instance.Body.PiercingMaterials.FirstOrDefault(x => x.Name == material);
                if (xmlMaterial != null) return xmlMaterial.ID;

                return 0;
            }
            set
            {
                if (Type != 0 && value != 0) GeneratePiercingName(Type, value);
                OnPropertyChanged("Label");
            }
        }

        private void GeneratePiercingName(int type, int material)
        {
            if (type == 0 || material == 0)
            {
                UpperName = "";
                LowerName = "";
                return;
            }

            var xmlMaterial = XmlData.Instance.Body.PiercingMaterials.FirstOrDefault(x => x.ID == material);
            string upper = xmlMaterial.Name;
            string lower = xmlMaterial.Name.ToLower();
            string rest = " ";

            //location
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
            UpperName = upper + rest;   // Using those properties is easier to read and trigger OnPropertyChanged trough SetValue's hidden argument
            LowerName = lower + rest;
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
