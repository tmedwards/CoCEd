using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public class CockVM : ObjectVM
    {
        public CockVM(AmfObject obj)
            : base(obj)
        {
            Piercing = new PiercingVM(obj, "");
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

        public double Length
        {
            get { return GetDouble("cockLength"); }
            set { SetValue("cockLength", value); }
        }

        public double Thickness
        {
            get { return GetDouble("cockThickness"); }
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

        public string LabelPart1
        {
            get { return Length.ToString("0") + "\""; }
        }

        public string LabelPart2
        {
            get
            {
                var type = Type;
                var cockType = XmlData.Instance.Body.CockTypes.FirstOrDefault(x => x.ID == type);
                var cockTypeName = cockType != null ? cockType.Name : "unknown";
                return String.Format(" long {0} cock", cockTypeName);
            }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            base.OnPropertyChanged("LabelPart1");
            base.OnPropertyChanged("LabelPart2");
        }
    }

    public sealed class CockArrayVM : ArrayVM<CockVM>
    {
        public CockArrayVM(AmfObject obj)
            : base(obj, x => new CockVM(x))
        {
        }

        protected override AmfObject CreateNewObject()
        {
            var obj = new AmfObject(AmfTypes.Array);
            obj["cockLength"] = 8;
            obj["cockThickness"] = 2;
            obj["cockType"] = 0;
            obj["knotMultiplier"] = 0.0;
            obj["pierced"] = 0;
            obj["pLong"] = "";
            obj["pShort"] = "";
            return obj;
        }
    }
}
