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
        readonly string _suffix;
        readonly PiercingLocation _location;

        public PiercingVM(AmfObject obj, string prefix, PiercingLocation location)
            : base(obj)
        {
            _prefix = prefix;
            _suffix = "";
            _location = location;
        }

        // new constructor for cock piercings, which require a suffix
        public PiercingVM(AmfObject obj, string prefix, string suffix, PiercingLocation location)
            : base(obj)
        {
            _prefix = prefix;
            _suffix = suffix;
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
                        // Ring
                        case 2:
                            type2.IsGrayedOut = (_location == PiercingLocation.Tongue);
                            break;

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

        public int Type
        {
            get { return GetInt(_prefix == "" ? "pierced" : _prefix + "Pierced", 0); }
            set
            {
                // Upper name is unfortunately changed by the combobox when we do change type, so we store it beforehand.
                var oldUpperName = UpperName;
                var oldLowerName = LowerName;
                bool wasStandardUpperName = String.IsNullOrEmpty(oldUpperName) || GetUnorderedNames().Contains(oldUpperName);

                // Change type
                SetValue(_prefix == "" ? "pierced" : _prefix + "Pierced", value);
                OnPropertyChanged("Label");
                OnPropertyChanged("CanEditName");
                OnPropertyChanged("SuggestedNames");

                // Change names if they were standard names
                if (wasStandardUpperName)
                {
                    if (Type == 0) UpperName = "";
                    else UpperName = SuggestedNames.First();
                }
                else
                {
                    UpperName = oldUpperName;
                    LowerName = oldLowerName;
                }

            }
        }

        public string UpperName
        {
            get { return GetString((_prefix == "" ? "pLong" : _prefix + "PLong") + _suffix); }
            set
            {
                if (!SetValue((_prefix == "" ? "pLong" : _prefix + "PLong") + _suffix, value)) return;
                OnPropertyChanged("Label");

                // Update lower name
                if (String.IsNullOrEmpty(value))
                {
                    LowerName = "";
                }
                else
                {
                    // Replace first letter by its lower counterpart
                    var chars = value.ToCharArray();
                    chars[0] = Char.ToLowerInvariant(chars[0]);
                    LowerName = new string(chars);
                }

            }
        }

        public string LowerName
        {
            get { return GetString((_prefix == "" ? "pShort" : _prefix + "PShort") + _suffix); }
            set
            {
                SetValue((_prefix == "" ? "pShort" : _prefix + "PShort") + _suffix, value);
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

        public IEnumerable<String> SuggestedNames
        {
            get { return GetUnorderedNames().OrderBy(x => x); }
        }

        IEnumerable<String> GetUnorderedNames()
        {
            if (Type == 0) yield break;

            foreach (var material in XmlData.Instance.Body.PiercingMaterials)
            {
                yield return GeneratePiercingName(Type, material);
            }

            if (Type == 1)
            {
                if (_location == PiercingLocation.Ears) yield return "Green gem-stone ear-studs";
                else if (_location == PiercingLocation.Nipples) yield return "Seamless black nipple-studs";
                else if (_location == PiercingLocation.Cock) yield return "Seamless, diamond cock-stud";
                else if (_location == PiercingLocation.Clitoris) yield return "Seamless, diamond clit-stud";
                else if (_location == PiercingLocation.Eyebrow) yield return "Seamless, diamond eyebrow-stud";
            }
        }

        string GeneratePiercingName(int type, string material)
        {
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
            return material + rest;
        }
    }
}
