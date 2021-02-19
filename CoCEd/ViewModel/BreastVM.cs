using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class BreastArrayVM : ArrayVM<BreastsVM>
    {
        public BreastArrayVM(GameVM game, AmfObject obj)
            : base(obj, x => new BreastsVM(game, x))
        {
        }

        protected override AmfObject CreateNewObject()
        {
            var obj = new AmfObject(AmfTypes.Array)
            {
                ["breasts"] = 2,
                ["nipplesPerBreast"] = 1,
                ["breastRating"] = 3.0,
                ["lactationMultiplier"] = 0.0,
                ["milkFullness"] = 0.0,
                ["fullness"] = 0.0,
                ["fuckable"] = false,
            };
            return obj;
        }
    }

    public class BreastsVM : ObjectVM
    {
        public BreastsVM(GameVM game, AmfObject obj)
            : base(obj)
        {
            _game = game;
        }

        private GameVM _game { get; set; }

        public int Rating
        {
            get { return GetInt("breastRating"); }
            set
            {
                SetDouble("breastRating", value);
                OnPropertyChanged("RatingDescription");
                OnPropertyChanged("Description");
                OnPropertyChanged("MilkVolume");
            }
        }

        public int MaxRating
        {
            get { return _game.IsRevamp || _game.IsXianxia ? 199 : 99; }
        }

        public int BreastCount
        {
            get { return GetInt("breasts"); }
            set
            {
                SetValue("breasts", value);
                OnPropertyChanged("Description");
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
                SetValue("lactationMultiplier", value);
                OnPropertyChanged("MilkVolume");
            }
        }

        public string MilkVolume
        {
            get
            {
                double endurance = _game.GetStatus("Lactation Endurance").IsOwned ? _game.GetStatus("Lactation Endurance").Value1 : 1.0;
                double reduction = _game.GetStatus("Lactation Reduction").IsOwned ? _game.GetStatus("Lactation Reduction").Value1 : 0.0;
                double qty = Rating * 10 * LactationMultiplier * endurance * BreastCount;
                if (_game.IsRevamp || _game.IsXianxia)
                {
                    var milkMaid = _game.GetPerk("Milk Maid");
                    if (milkMaid.IsOwned) qty += 200 + milkMaid.Value1 * 100;
                }
                if (reduction >= 48) qty *= 1.5;
                if ((_game.IsRevamp || _game.IsXianxia) && qty > Int32.MaxValue) qty = Int32.MaxValue;
                //return GameVM.FormatVolume(qty, "/h (base)");
                return GameVM.FormatVolume(qty);
            }
        }

        public void UpdateMilkVolume()
        {
            OnPropertyChanged("MilkVolume");
        }

        public string Description
        {
            get
            {
                var rating = RatingDescriptionLong;
                if (BreastCount == 1) return "A " + rating + " breast";
                if (BreastCount == 2) return "A pair of " + rating + " breasts";
                if (BreastCount == 3) return "A triad of " + rating + " breasts";
                if (BreastCount == 4) return "A quartet of " + rating + " breasts";
                if (BreastCount == 5) return "A quintet of " + rating + " breasts";
                if (BreastCount == 6) return "A sextet of " + rating + " breasts";
                return "A bunch of " + rating + " breasts";
            }
        }

        public string RatingDescription
        {
            get
            {
                if ((_game.IsRevamp || _game.IsXianxia) && Rating > 99) return RatingDescriptionLong.Replace("hyper", "hyp").Replace("large", "lg");
                else return RatingDescriptionLong;
            }
        }

        public string RatingDescriptionLong
        {
            get
            {
                int rating = Rating;
                var prefix = "";

                // Handle CoC-Revamp-Mod "hyper" ratings
                if ((_game.IsRevamp || _game.IsXianxia) && rating > 99)
                {
                    rating -= 99;
                    prefix = "hyper ";
                }

                switch (rating)
                {
                    case  0: return "flat, manly";
                    case  1: return prefix + "A-cup";
                    case  2: return prefix + "B-cup";
                    case  3: return prefix + "C-cup";
                    case  4: return prefix + "D-cup";
                    case  5: return prefix + "DD-cup";
                    case  6: return prefix + "big DD-cup";
                    case  7: return prefix + "E-cup";
                    case  8: return prefix + "big E-cup";
                    case  9: return prefix + "EE-cup";
                    case 10: return prefix + "big EE-cup";
                    case 11: return prefix + "F-cup";
                    case 12: return prefix + "big F-cup";
                    case 13: return prefix + "FF-cup";
                    case 14: return prefix + "big FF-cup";
                    case 15: return prefix + "G-cup";
                    case 16: return prefix + "big G-cup";
                    case 17: return prefix + "GG-cup";
                    case 18: return prefix + "big GG-cup";
                    case 19: return prefix + "H-cup";
                    case 20: return prefix + "big H-cup";
                    case 21: return prefix + "HH-cup";
                    case 22: return prefix + "big HH-cup";
                    case 23: return prefix + "HHH-cup";
                    case 24: return prefix + "I-cup";
                    case 25: return prefix + "big I-cup";
                    case 26: return prefix + "II-cup";
                    case 27: return prefix + "big II-cup";
                    case 28: return prefix + "J-cup";
                    case 29: return prefix + "big J-cup";
                    case 30: return prefix + "JJ-cup";
                    case 31: return prefix + "big JJ-cup";
                    case 32: return prefix + "K-cup";
                    case 33: return prefix + "big K-cup";
                    case 34: return prefix + "KK-cup";
                    case 35: return prefix + "big KK-cup";
                    case 36: return prefix + "L-cup";
                    case 37: return prefix + "big L-cup";
                    case 38: return prefix + "LL-cup";
                    case 39: return prefix + "big LL-cup";
                    case 40: return prefix + "M-cup";
                    case 41: return prefix + "big M-cup";
                    case 42: return prefix + "MM-cup";
                    case 43: return prefix + "big MM-cup";
                    case 44: return prefix + "MMM-cup";
                    case 45: return prefix + "large MMM-cup";
                    case 46: return prefix + "N-cup";
                    case 47: return prefix + "large N-cup";
                    case 48: return prefix + "NN-cup";
                    case 49: return prefix + "large NN-cup";
                    case 50: return prefix + "O-cup";
                    case 51: return prefix + "large O-cup";
                    case 52: return prefix + "OO-cup";
                    case 53: return prefix + "large OO-cup";
                    case 54: return prefix + "P-cup";
                    case 55: return prefix + "large P-cup";
                    case 56: return prefix + "PP-cup";
                    case 57: return prefix + "large PP-cup";
                    case 58: return prefix + "Q-cup";
                    case 59: return prefix + "large Q-cup";
                    case 60: return prefix + "QQ-cup";
                    case 61: return prefix + "large QQ-cup";
                    case 62: return prefix + "R-cup";
                    case 63: return prefix + "large R-cup";
                    case 64: return prefix + "RR-cup";
                    case 65: return prefix + "large RR-cup";
                    case 66: return prefix + "S-cup";
                    case 67: return prefix + "large S-cup";
                    case 68: return prefix + "SS-cup";
                    case 69: return prefix + "large SS-cup";
                    case 70: return prefix + "T-cup";
                    case 71: return prefix + "large T-cup";
                    case 72: return prefix + "TT-cup";
                    case 73: return prefix + "large TT-cup";
                    case 74: return prefix + "U-cup";
                    case 75: return prefix + "large U-cup";
                    case 76: return prefix + "UU-cup";
                    case 77: return prefix + "large UU-cup";
                    case 78: return prefix + "V-cup";
                    case 79: return prefix + "large V-cup";
                    case 80: return prefix + "VV-cup";
                    case 81: return prefix + "large VV-cup";
                    case 82: return prefix + "W-cup";
                    case 83: return prefix + "large W-cup";
                    case 84: return prefix + "WW-cup";
                    case 85: return prefix + "large WW-cup";
                    case 86: return prefix + "X-cup";
                    case 87: return prefix + "large X-cup";
                    case 88: return prefix + "XX-cup";
                    case 89: return prefix + "large XX-cup";
                    case 90: return prefix + "Y-cup";
                    case 91: return prefix + "large Y-cup";
                    case 92: return prefix + "YY-cup";
                    case 93: return prefix + "large YY-cup";
                    case 94: return prefix + "Z-cup";
                    case 95: return prefix + "large Z-cup";
                    case 96: return prefix + "ZZ-cup";
                    case 97: return prefix + "large ZZ-cup";
                    case 98: return prefix + "ZZZ-cup";
                    case 99: return prefix + "large ZZZ-cup";
                    default: return ((_game.IsRevamp || _game.IsXianxia) && rating == 100) ? "jacques00-cup" : "game-breaking";
                }
            }
        }
    }
}
