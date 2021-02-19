using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class CockArrayVM : ArrayVM<CockVM>
    {
        public CockArrayVM(GameVM game, AmfObject obj)
            : base(obj, x => new CockVM(game, x))
        {
        }

        protected override AmfObject CreateNewObject()
        {
            var obj = new AmfObject(AmfTypes.Array)
            {
                ["cockLength"] = 8,
                ["cockThickness"] = 2,
                ["cockType"] = 0,
                ["knotMultiplier"] = 1.0,
                ["pierced"] = 0,
                ["pLongDesc"] = "",
                ["pShortDesc"] = "",
                ["sock"] = "",
            };
            return obj;
        }
    }

    public class CockVM : ObjectVM
    {
        public CockVM(GameVM game, AmfObject obj)
            : base(obj)
        {
            Piercing = new PiercingVM(obj, "", "Desc", PiercingLocation.Cock);

            _game = game;
        }

        private GameVM _game { get; set; }

        public PiercingVM Piercing { get; private set; }

        public XmlEnum[] AllTypes
        {
            get { return XmlData.Current.Body.CockTypes; }
        }

        public XmlItem[] AllCockSocks
        {
            get { return XmlData.Current.Body.CockSockTypes; }
        }

        public int Type
        {
            get { return GetInt("cockType"); }
            set
            {
                SetValue("cockType", value);
                OnPropertyChanged("IsKnotEnabled");
                OnPropertyChanged("LabelPart2");
            }
        }

        public double Length
        {
            get { return GetDouble("cockLength"); }
            set 
            { 
                SetValue("cockLength", value);
                OnPropertyChanged("LabelPart1");
            }
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

        public bool IsKnotEnabled
        {
            // Dog, Coeurl, Fox
            get { return (Type == 2 || Type == 10 || Type == 11); }
        }

        public string CockSock
        {
            get { return GetString("sock"); }
            set
            {
                bool greenSockChanged = false;
                if (_game.IsRevamp || _game.IsXianxia)
                {
                    if (CockSock == "green")
                    {
                        if (value != "green") greenSockChanged = true;
                    }
                    else if (value == "green") greenSockChanged = true;
                }
                SetValue("sock", value);
                if (greenSockChanged) _game.NotifyPropertyChanged("MaxHP");
            }
        }

        public string LabelPart1
        {
            get { return Length.ToString("0") + "\u2033"; }
        }

        public string LabelPart2
        {
            get
            {
                var type = Type;
                var cockType = XmlData.Current.Body.CockTypes.FirstOrDefault(x => x.ID == type);
                var cockTypeName = cockType != null ? cockType.Name : "<unknown>";
                return String.Format(" long {0} cock", cockTypeName);
            }
        }
    }
}
