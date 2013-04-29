using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public class CockVM : NodeVM
    {
        public CockVM(AmfNode node)
            : base(node)
        {
            Piercing = new PiercingVM(node, "");
        }

        public PiercingVM Piercing { get; private set; }

        public XmlEnum[] AllTypes
        {
            get { return XmlData.Instance.Body.CockTypes; }
        }

        public int Type
        {
            get { return GetInt("cockType"); }
            set
            {
                if (!SetValue("cockType", value)) return;
                OnPropertyChanged("KnotVisibility");
            }
        }

        public int Length
        {
            get { return GetInt("cockLength"); }
            set { SetValue("cockLength", value); }
        }

        public int Thickness
        {
            get { return GetInt("cockThickness"); }
            set { SetValue("cockThickness", value); }
        }

        public double KnotMultiplier
        {
            get { return GetDouble("knotMultiplier"); }
            set { SetValue("knotMultiplier", value); }
        }

        public Visibility KnotVisibility
        {
            get { return Type == 2 ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string Description
        {
            get
            {
                var type = Type;
                var cockType = XmlData.Instance.Body.CockTypes.FirstOrDefault(x => x.ID == type);
                var cockTypeName = cockType != null ? cockType.Name : "unknown";
                return String.Format("\" long {0} cock", cockTypeName);
            }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            base.OnPropertyChanged("Description");
        }
    }

    public sealed class CockArrayVM : ArrayVM<CockVM>
    {
        public CockArrayVM(AmfNode node)
            : base(node, x => new CockVM(x))
        {
        }

        protected override AmfNode CreateNewNode()
        {
            var node = new AmfArray();
            node["cockLength"] = 8;
            node["cockThickness"] = 2;
            node["cockType"] = 0;
            node["knotMultiplier"] = 0.0;
            node["pierced"] = 0;
            node["pLong"] = "";
            node["pShort"] = "";
            return node;
        }
    }
}
