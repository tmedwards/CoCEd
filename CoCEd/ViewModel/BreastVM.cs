using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public class BreastsVM : NodeVM
    {
        public BreastsVM(AmfNode node)
            : base(node)
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
                if (Rating <= 1) return "A";
                if (Rating == 2) return "B";
                if (Rating == 3) return "C";
                if (Rating == 4) return "D";
                if (Rating <= 6) return "DD";

                // E=7,8  EE=9,10  F=11,12  FF=13,14
                int offset = Math.Min(21, (Rating - 7) / 4);
                char letter = (char)((int)'A' + (offset + 4));
                bool doubled = (Rating - 7) >= offset * 4 + 2;

                if (doubled) return letter.ToString() + letter;
                return letter.ToString();
            }
        }

        public string Description
        {
            get
            {
                var rating = RatingDescription;
                if (BreastCount == 1) return "A " + rating + "-cup breast";
                if (BreastCount == 2) return "A pair of " + rating + "-cup breasts";
                if (BreastCount == 3) return "A triad of " + rating + "-cup breasts";
                if (BreastCount == 4) return "A quartet of " + rating + "-cup breasts";
                if (BreastCount == 5) return "A quintet of " + rating + "-cup breasts";
                if (BreastCount == 6) return "A sextet of " + rating + "-cup breasts";
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
        public BreastArrayVM(AmfNode node)
            : base(node, x => new BreastsVM(x))
        {
        }

        protected override AmfNode CreateNewNode()
        {
            var node = new AmfArray();
            node["breasts"] = 2;
            node["fuckable"] = false;
            node["breastRating"] = 3.0;
            node["nipplesPerBreast"] = 1;
            node["lactationMultiplier"] = 1.0;
            node["milkFullness"] = 0;
            node["fullness"] = 0;
            return node;
        }
    }
}
