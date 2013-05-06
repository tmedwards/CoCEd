using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public class BreastsVM : ObjectVM
    {
        public BreastsVM(AmfObject obj)
            : base(obj)
        {
        }

        public int Rating
        {
            get { return GetInt("breastRating"); }
            set
            {
                if (!SetDouble("breastRating", value)) return;
                OnPropertyChanged("RatingDescription");
                OnPropertyChanged("MilkVolume");
            }
        }

        public int BreastCount
        {
            get { return GetInt("breasts"); }
            set
            {
                if (!SetValue("breasts", value)) return;
                OnPropertyChanged("MilkVolume");
            }
        }

        public int NipplesPerBreast
        {
            get { return GetInt("nipplesPerBreast"); }
            set { SetValue("nipplesPerBreast", value); }
        }

        public bool Fuckable
        {
            get { return GetBool("fuckable"); }
            set { SetValue("fuckable", value); }
        }

        public double LactationMultiplier
        {
            get { return GetDouble("lactationMultiplier"); }
            set
            {
                if (!SetValue("lactationMultiplier", value)) return;
                OnPropertyChanged("MilkVolume");
            }
        }

        public string MilkVolume
        {
            get
            {
                var qty = Rating * 10 * LactationMultiplier * BreastCount;
                if (qty == 0) return "";
                return qty < 1000 ? String.Format("{0:0} mL/h (base)", qty) : String.Format("{0:0.00} L/h (base)", qty * 0.001);
            }
        }

        public string RatingDescription
        {
            get
            {
                switch(Rating)
                {
                    case 1: return "flat, manly breast";
                    case 2: return "A-cup";
                    case 3: return "B-cup";
                    case 4: return "C-cup";
                    case 5: return "D-cup";
                    case 6: return "DD-cup";
                    case 7: return "big DD-cup";
                    case 8: return "E-cup";
                    case 9: return "big E-cup";
                    case 10: return "EE-cup";
                    case 11: return "big EE-cup";
                    case 12: return "F-cup";
                    case 13: return "big F-cup";
                    case 14: return "FF-cup";
                    case 15: return "big FF-cup";
                    case 16: return "G-cup";
                    case 17: return "big G-cup";
                    case 18: return "GG-cup";
                    case 19: return "big GG-cup";
                    case 20: return "H-cup";
                    case 21: return "big H-cup";
                    case 22: return "HH-cup";
                    case 23: return "big HH-cup";
                    case 24: return "HHH-cup";
                    case 25: return "I-cup";
                    case 26: return "big I-cup";
                    case 27: return "II-cup";
                    case 28: return "big II-cup";
                    case 29: return "J-cup";
                    case 30: return "big J-cup";
                    case 31: return "JJ-cup";
                    case 32: return "big JJ-cup";
                    case 33: return "K-cup";
                    case 34: return "big K-cup";
                    case 35: return "KK-cup";
                    case 36: return "big KK-cup";
                    case 37: return "L-cup";
                    case 38: return "big L-cup";
                    case 39: return "LL-cup";
                    case 40: return "big LL-cup";
                    case 41: return "M-cup";
                    case 42: return "big M-cup";
                    case 43: return "MM-cup";
                    case 44: return "big MM-cup";
                    case 45: return "MMM-cup";
                    case 46: return "large MMM-cup";
                    case 47: return "N-cup";
                    case 48: return "large N-cup";
                    case 49: return "NN-cup";
                    case 50: return "large NN-cup";
                    case 51: return "O-cup";
                    case 52: return "large O-cup";
                    case 53: return "OO-cup";
                    case 54: return "large OO-cup";
                    case 55: return "P-cup";
                    case 56: return "large P-cup";
                    case 57: return "PP-cup";
                    case 58: return "large PP-cup";
                    case 59: return "Q-cup";
                    case 60: return "large Q-cup";
                    case 61: return "QQ-cup";
                    case 62: return "large QQ-cup";
                    case 63: return "R-cup";
                    case 64: return "large R-cup";
                    case 65: return "RR-cup";
                    case 66: return "large RR-cup";
                    case 67: return "S-cup";
                    case 68: return "large S-cup";
                    case 69: return "SS-cup";
                    case 70: return "large SS-cup";
                    case 71: return "T-cup";
                    case 72: return "large T-cup";
                    case 73: return "TT-cup";
                    case 74: return "large TT-cup";
                    case 75: return "U-cup";
                    case 76: return "large U-cup";
                    case 77: return "UU-cup";
                    case 78: return "large UU-cup";
                    case 79: return "V-cup";
                    case 80: return "large V-cup";
                    case 81: return "VV-cup";
                    case 82: return "large VV-cup";
                    case 83: return "W-cup";
                    case 84: return "large W-cup";
                    case 85: return "WW-cup";
                    case 86: return "large WW-cup";
                    case 87: return "X-cup";
                    case 88: return "large X-cup";
                    case 89: return "XX-cup";
                    case 90: return "large XX-cup";
                    case 91: return "Y-cup";
                    case 92: return "large Y-cup";
                    case 93: return "YY-cup";
                    case 94: return "large YY-cup";
                    case 95: return "Z-cup";
                    case 96: return "large Z-cup";
                    case 97: return "ZZ-cup";
                    case 98: return "large ZZ-cup";
                    case 99: return "ZZZ-cup";
                    case 100: return "large ZZZ-cup";
                    default: return "game-breaking cup";
                }
            }
        }

        public string Description
        {
            get
            {
                var rating = RatingDescription;
                if (BreastCount == 1) return "A " + rating + " breast";
                if (BreastCount == 2) return "A pair of " + rating + " breasts";
                if (BreastCount == 3) return "A triad of " + rating + " breasts";
                if (BreastCount == 4) return "A quartet of " + rating + " breasts";
                if (BreastCount == 5) return "A quintet of " + rating + " breasts";
                if (BreastCount == 6) return "A sextet of " + rating + " breasts";
                return "A bunch of " + rating + "-cup breasts";
            }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            base.OnPropertyChanged("Description");
        }
    }

    public sealed class BreastArrayVM : ArrayVM<BreastsVM>
    {
        public BreastArrayVM(AmfObject obj)
            : base(obj, x => new BreastsVM(x))
        {
        }

        protected override AmfObject CreateNewObject()
        {
            var obj = new AmfObject(AmfTypes.Array);
            obj["breasts"] = 2;
            obj["fuckable"] = false;
            obj["breastRating"] = 3.0;
            obj["nipplesPerBreast"] = 1;
            obj["lactationMultiplier"] = 1.0;
            obj["milkFullness"] = 0;
            obj["fullness"] = 0;
            return obj;
        }
    }
}
