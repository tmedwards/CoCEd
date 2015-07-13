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
            var obj = new AmfObject(AmfTypes.Array);
            obj["breasts"] = 2;
            obj["nipplesPerBreast"] = 1;
            obj["breastRating"] = 3.0;
            obj["lactationMultiplier"] = 0.0;
            obj["milkFullness"] = 0.0;
            obj["fullness"] = 0.0;
            obj["fuckable"] = false;
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
                if (_game.IsRevampMod)
                {
                    var milkMaid = _game.GetPerk("Milk Maid");
                    if (milkMaid.IsOwned) qty += 200 + milkMaid.Value1 * 100;
                }
                if (reduction >= 48) qty *= 1.5;
                if (_game.IsRevampMod && qty > Int32.MaxValue) qty = Int32.MaxValue;
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
                var rating = RatingDescription;
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
                switch(Rating)
                {
                    case  0: return "flat, manly";
                    case  1: return "A-cup";
                    case  2: return "B-cup";
                    case  3: return "C-cup";
                    case  4: return "D-cup";
                    case  5: return "DD-cup";
                    case  6: return "big DD-cup";
                    case  7: return "E-cup";
                    case  8: return "big E-cup";
                    case  9: return "EE-cup";
                    case 10: return "big EE-cup";
                    case 11: return "F-cup";
                    case 12: return "big F-cup";
                    case 13: return "FF-cup";
                    case 14: return "big FF-cup";
                    case 15: return "G-cup";
                    case 16: return "big G-cup";
                    case 17: return "GG-cup";
                    case 18: return "big GG-cup";
                    case 19: return "H-cup";
                    case 20: return "big H-cup";
                    case 21: return "HH-cup";
                    case 22: return "big HH-cup";
                    case 23: return "HHH-cup";
                    case 24: return "I-cup";
                    case 25: return "big I-cup";
                    case 26: return "II-cup";
                    case 27: return "big II-cup";
                    case 28: return "J-cup";
                    case 29: return "big J-cup";
                    case 30: return "JJ-cup";
                    case 31: return "big JJ-cup";
                    case 32: return "K-cup";
                    case 33: return "big K-cup";
                    case 34: return "KK-cup";
                    case 35: return "big KK-cup";
                    case 36: return "L-cup";
                    case 37: return "big L-cup";
                    case 38: return "LL-cup";
                    case 39: return "big LL-cup";
                    case 40: return "M-cup";
                    case 41: return "big M-cup";
                    case 42: return "MM-cup";
                    case 43: return "big MM-cup";
                    case 44: return "MMM-cup";
                    case 45: return "large MMM-cup";
                    case 46: return "N-cup";
                    case 47: return "large N-cup";
                    case 48: return "NN-cup";
                    case 49: return "large NN-cup";
                    case 50: return "O-cup";
                    case 51: return "large O-cup";
                    case 52: return "OO-cup";
                    case 53: return "large OO-cup";
                    case 54: return "P-cup";
                    case 55: return "large P-cup";
                    case 56: return "PP-cup";
                    case 57: return "large PP-cup";
                    case 58: return "Q-cup";
                    case 59: return "large Q-cup";
                    case 60: return "QQ-cup";
                    case 61: return "large QQ-cup";
                    case 62: return "R-cup";
                    case 63: return "large R-cup";
                    case 64: return "RR-cup";
                    case 65: return "large RR-cup";
                    case 66: return "S-cup";
                    case 67: return "large S-cup";
                    case 68: return "SS-cup";
                    case 69: return "large SS-cup";
                    case 70: return "T-cup";
                    case 71: return "large T-cup";
                    case 72: return "TT-cup";
                    case 73: return "large TT-cup";
                    case 74: return "U-cup";
                    case 75: return "large U-cup";
                    case 76: return "UU-cup";
                    case 77: return "large UU-cup";
                    case 78: return "V-cup";
                    case 79: return "large V-cup";
                    case 80: return "VV-cup";
                    case 81: return "large VV-cup";
                    case 82: return "W-cup";
                    case 83: return "large W-cup";
                    case 84: return "WW-cup";
                    case 85: return "large WW-cup";
                    case 86: return "X-cup";
                    case 87: return "large X-cup";
                    case 88: return "XX-cup";
                    case 89: return "large XX-cup";
                    case 90: return "Y-cup";
                    case 91: return "large Y-cup";
                    case 92: return "YY-cup";
                    case 93: return "large YY-cup";
                    case 94: return "Z-cup";
                    case 95: return "large Z-cup";
                    case 96: return "ZZ-cup";
                    case 97: return "large ZZ-cup";
                    case 98: return "ZZZ-cup";
                    case 99: return "large ZZZ-cup";
                    default: return "game-breaking";
                }
            }
        }
    }
}
