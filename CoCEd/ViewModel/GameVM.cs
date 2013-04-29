using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    // TeaseLevel / XP
    public sealed class GameVM : NodeVM
    {
        public GameVM(AmfFile file)
            : base(file)
        {
            // Unique children
            Ass = new AssVM(file["ass"]);
            LipPiercing = new PiercingVM(_node, "lip");
            NosePiercing = new PiercingVM(_node, "nose");
            EarsPiercing = new PiercingVM(_node, "ears");
            EyebrowPiercing = new PiercingVM(_node, "eyebrow");
            NipplesPiercing = new PiercingVM(_node, "nipples");
            TonguePiercing = new PiercingVM(_node, "tongue");


            // Collections
            Cocks = new CockArrayVM(file["cocks"]);
            Breasts = new BreastArrayVM(file["breastRows"]);
            Vaginas = new VaginaArrayVM(file["vaginas"]);
            Vaginas.CollectionChanged += OnGenitalsCollectionChanged;
            Breasts.CollectionChanged += OnGenitalsCollectionChanged;
            Cocks.CollectionChanged += OnGenitalsCollectionChanged;


            // Items
            var groups = new List<ItemSlotGroupVM>();
            var group = new ItemSlotGroupVM("Inventory", ItemCategories.All);
            for (int i = 0; i < 5; i++) group.Add(file["itemSlot" + (i + 1)]);
            groups.Add(group);

            group = new ItemSlotGroupVM("Chest", ItemCategories.All);
            foreach(var pair in file["itemStorage"]) group.Add(pair.Value);
            groups.Add(group);

            group = new ItemSlotGroupVM("Armor rack", ItemCategories.Armor);
            for (int i = 0; i < 9; i++) group.Add(file["gearStorage"][i + 9]);
            groups.Add(group);

            group = new ItemSlotGroupVM("Weapon rack", ItemCategories.Weapon);
            for (int i = 0; i < 9; i++) group.Add(file["gearStorage"][i]);
            groups.Add(group);

            ItemGroups = groups.ToArray();


            // Perks
            PerkGroups = new PerkGroupVM[]
            {
                new PerkGroupVM("Starter", _node, XmlData.Instance.Perks.StarterPerks),
                new PerkGroupVM("History", _node, XmlData.Instance.Perks.HistoryPerks),
                new PerkGroupVM("Tier0", _node, XmlData.Instance.Perks.Tier0Perks),
                new PerkGroupVM("Tier1", _node, XmlData.Instance.Perks.Tier1Perks),
                new PerkGroupVM("Tier2", _node, XmlData.Instance.Perks.Tier2Perks),
                new PerkGroupVM("Events", _node, XmlData.Instance.Perks.EventPerks)
            };
        }

        public CockArrayVM Cocks { get; private set; }
        public BreastArrayVM Breasts { get; private set; }
        public VaginaArrayVM Vaginas { get; private set; }

        public ItemSlotGroupVM[] ItemGroups { get; private set; }
        public PerkGroupVM[] PerkGroups { get; private set; }

        public AssVM Ass { get; private set; }
        public PiercingVM NosePiercing { get; private set; }
        public PiercingVM EarsPiercing { get; private set; }
        public PiercingVM EyebrowPiercing { get; private set; }
        public PiercingVM NipplesPiercing { get; private set; }
        public PiercingVM TonguePiercing { get; private set; }
        public PiercingVM LipPiercing { get; private set; }

        public string Name
        {
            get { return GetString("short"); }
            set 
            { 
                SetValue("short", value);
                SetValue("notes", value);
            }
        }

        public int Strength
        {
            get { return GetInt("str"); }
            set { SetDouble("str", value); }
        }

        public int Toughness
        {
            get { return GetInt("tou"); }
            set { SetDouble("tou", value); }
        }

        public int Speed
        {
            get { return GetInt("spe"); }
            set { SetDouble("spe", value); }
        }

        public int Intelligence
        {
            get { return GetInt("inte"); }
            set { SetDouble("inte", value); }
        }

        public int Libido
        {
            get { return GetInt("lib"); }
            set { SetDouble("lib", value); }
        }

        public int Sensitivity
        {
            get { return GetInt("sens"); }
            set { SetDouble("sens", value); }
        }

        public int Corruption
        {
            get { return GetInt("cor"); }
            set { SetDouble("cor", value); }
        }

        public int HP
        {
            get { return GetInt("HP"); }
            set { SetValue("HP", value); }
        }

        public int XP
        {
            get { return GetInt("XP"); }
            set { SetValue("XP", value); }
        }

        public int TeaseXP
        {
            get { return GetInt("teaseXP"); }
            set { SetValue("teaseXP", value); }
        }

        public int Lust
        {
            get { return GetInt("lust"); }
            set { SetDouble("lust", value); }
        }

        public int Fatigue
        {
            get { return GetInt("fatigue"); }
            set { SetValue("fatigue", value); }
        }

        public int Gems
        {
            get { return GetInt("gems"); }
            set { SetValue("gems", value); }
        }



        public double HairLength
        {
            get { return GetDouble("hairLength"); }
            set { SetValue("hairLength", value); }
        }

        public string HairColor
        {
            get { return GetString("hairColor"); }
            set { SetValue("hairColor", value); }
        }

        public int FaceType
        {
            get { return GetInt("faceType"); }
            set { SetValue("faceType", value); }
        }

        public int AntennaeType
        {
            get { return GetInt("antennae"); }
            set { SetValue("antennae", value); }
        }

        public int EyeType
        {
            get { return GetInt("eyeType"); }
            set { SetValue("eyeType", value); }
        }

        public int EarType
        {
            get { return GetInt("earType"); }
            set { SetValue("earType", value); }
        }

        public int TongueType
        {
            get { return GetInt("tongueType"); }
            set { SetValue("tongueType", value); }
        }

        public int HornType
        {
            get { return GetInt("hornType"); }
            set 
            {
                if (!SetValue("hornType", value)) return;
                OnPropertyChanged("HornsValueVisibility");
                OnPropertyChanged("HornsValueLabel");
                OnPropertyChanged("HornsValueUnit");
            }
        }

        public double HornsValue
        {
            get { return GetDouble("horns"); }
            set { SetValue("horns", value); }
        }

        public string HornsValueLabel
        {
            get
            {
                if (HornType == 1) return "Horn count";
                if (HornType == 2) return "Horns length";
                if (HornType == 5) return "Antlers' branches";
                return "?";
            }
        }

        public string HornsValueUnit
        {
            get { return HornType == 2 ? "inches" : ""; }
        }

        public Visibility HornsValueVisibility
        {
            get { return (HornType == 1 || HornType == 2 || HornType == 5) ? Visibility.Visible : Visibility.Collapsed; }
        }



        public double Height
        {
            get { return GetDouble("tallness"); }
            set { SetValue("tallness", value); }
        }

        public int HipRating
        {
            get { return GetInt("hipRating"); }
            set { SetValue("hipRating", value); }
        }

        public int ButtRating
        {
            get { return GetInt("buttRating"); }
            set { SetValue("buttRating", value); }
        }

        public int BodyThickness
        {
            get { return GetInt("thickness"); }
            set { SetValue("thickness", value); }
        }

        public int Muscles
        {
            get { return GetInt("tone"); }
            set { SetValue("tone", value); }
        }

        public int Feminity
        {
            get { return GetInt("femininity"); }
            set { SetValue("femininity", value); }
        }

        public int SkinType
        {
            get { return GetInt("skinType"); }
            set { SetValue("skinType", value); }
        }

        public string SkinTone
        {
            get { return GetString("skinTone"); }
            set { SetValue("skinTone", value); }
        }

        public int LowerBodyType
        {
            get { return GetInt("lowerBody"); }
            set { SetValue("lowerBody", value); }
        }

        public int TailType
        {
            get { return GetInt("tailType"); }
            set { SetValue("tailType", value); }
        }

        public int WingType
        {
            get { return GetInt("wingType"); }
            set { SetValue("wingType", value); }
        }



        public int Fertility
        {
            get { return GetInt("fertility"); }
            set { SetValue("fertility", value); }
        }

        public int PregnancyType
        {
            get { return GetInt("pregnancyType"); }
            set 
            {
                if (!SetValue("pregnancyType", value)) return;
                OnPropertyChanged("PregnancyVisibility");
            }
        }

        public int PregnancyTime
        {
            get { return GetInt("pregnancyIncubation"); }
            set { SetValue("pregnancyIncubation", value); }
        }

        public Visibility PregnancyVisibility
        {
            get { return PregnancyType == 0 ? Visibility.Collapsed : Visibility.Visible; }
        }

        public int ButtPregnancyType
        {
            get { return GetInt("buttPregnancyType"); }
            set 
            {
                if (!SetValue("buttPregnancyType", value)) return;
                OnPropertyChanged("ButtPregnancyVisibility");
            }
        }

        public int ButtPregnancyTime
        {
            get { return GetInt("buttPregnancyIncubation"); }
            set { SetValue("buttPregnancyIncubation", value); }
        }


        public int Balls
        {
            get { return GetInt("balls"); }
            set
            {
                if (!SetValue("balls", value)) return;
                OnPropertyChanged("CumVolume");
            }
        }

        public double BallSize
        {
            get { return GetDouble("ballSize"); }
            set
            {
                if (!SetValue("ballSize", value)) return;
                OnPropertyChanged("CumVolume");
            }
        }

        public double CumMultiplier
        {
            get { return GetDouble("cumMultiplier"); }
            set 
            {
                if (!SetValue("cumMultiplier", value)) return;
                OnPropertyChanged("CumVolume");
            }
        }

        public double ClitLength
        {
            get { return GetDouble("clitLength"); }
            set { SetValue("clitLength", value); }
        }

        public double NippleLength
        {
            get { return GetDouble("nippleLength"); }
            set { SetValue("nippleLength", value); }
        }

        public string CumVolume
        {
            get 
            { 
                var qty = BallSize * Balls * CumMultiplier;
                if (qty == 0) return "";
                return qty < 1000 ? String.Format("{0:0} mL/h (base)", qty) : String.Format("{0:0.00} L/h (base)", qty * 0.001);
            }
        }

        public Visibility ButtPregnancyVisibility
        {
            get { return ButtPregnancyType == 0 ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility NippleVisibility
        {
            get { return Breasts.Count == 0 ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility ClitVisibility
        {
            get { return Vaginas.Count == 0 ? Visibility.Collapsed : Visibility.Visible; }
        }

        public bool HasMetTamani
        {
            get { return HasStatus("Tamani"); }
        }

        public int TamaniChildren
        {
            get { return GetStatusInt("Tamani", "2"); }
            set { SetStatusValue("Tamani", "2", value); }
        }

        public int Exagartuan
        {
            get { return GetStatusInt("Exgartuan", "1", 0); }
            set 
            {
                if (value == Exagartuan) return;
                if (Exagartuan == 0) EnsureStatusExists("Exgartuan", value, 0, 0, 0);
                else if (value == 0) RemoveStatus("Exgartuan");
                else SetStatusValue("Exgartuan", "1", value);
            }
        }

        int GetStatusInt(string name, string index, int defaultValue = 0)
        {
            var node = GetStatus(name);
            if (node == null) return defaultValue;
            return node.GetInt("value" + index);
        }

        void SetStatusValue(string name, string index, dynamic value, [CallerMemberName] string propertyName = null)
        {
            if (GetStatus(name)["value" + index] == value) return;
            GetStatus(name)["value" + index] = value;
            OnPropertyChanged(propertyName);
        }

        bool HasStatus(string name)
        {
            return GetStatus(name) != null;
        }

        AmfNode GetStatus(string name)
        {
            AmfNode statuses = _node["statusAffects"];
            return statuses.Select(x => x.Value).Cast<AmfNode>().FirstOrDefault(x => name == x["statusAffectName"] as string);
        }

        void RemoveStatus(string name, [CallerMemberName] string propertyName = null)
        {
            AmfNode statuses = _node["statusAffects"];
            AmfPair pair = statuses.FirstOrDefault(x => name == x.Value["statusAffectName"] as string);
            if (pair == null) return;

            object node;
            statuses.Remove(pair.Key, true, out node);
            OnPropertyChanged("propertyName");
        }

        void EnsureStatusExists(string name, dynamic defaultValue1, dynamic defaultValue2, dynamic defaultValue3, dynamic defaultValue4, [CallerMemberName] string propertyName = null)
        {
            var node = GetStatus(name);
            if (node != null) return;

            node = new AmfArray();
            node["statusAffectName"] = name;
            node["value1"] = defaultValue1;
            node["value2"] = defaultValue2;
            node["value3"] = defaultValue3;
            node["value4"] = defaultValue4;

            AmfNode statuses = _node["statusAffects"];
            statuses.Add(node);

            OnPropertyChanged(propertyName);
            return;
        }

        void OnGenitalsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Cocks.Count != 0 && Vaginas.Count != 0) SetValue("gender", 3);
            else if (Vaginas.Count != 0) SetValue("gender", 2);
            else if (Cocks.Count != 0) SetValue("gender", 1);
            else SetValue("gender", 0);

            OnPropertyChanged("NippleVisibility");
            OnPropertyChanged("ClitVisibility");
        }
    }

    public sealed class AssVM : NodeVM
    {
        public AssVM(AmfNode node)
            : base(node)
        {
        }

        public int Looseness
        {
            get { return GetInt("analLooseness"); }
            set { SetValue("analLooseness", value); }
        }

        public int Wetness
        {
            get { return GetInt("analWetness"); }
            set { SetValue("analWetness", value); }
        }
    }

    public sealed class PiercingVM : NodeVM
    {
        readonly string _prefix;

        public PiercingVM(AmfNode node, string prefix)
            : base(node)
        {
            _prefix = prefix;
        }

        public IEnumerable<XmlEnum> AllTypes
        {
            get { return XmlData.Instance.Body.PiercingTypes; }
        }

        public int Type
        {
            get { return GetInt(_prefix == "" ? "pierced" : _prefix + "Pierced"); }
            set 
            { 
                SetValue(_prefix == "" ? "pierced" : _prefix + "Pierced", value);
                OnPropertyChanged("CanEditName");
            }
        }

        public string UpperName
        {
            get { return GetString(_prefix == "" ? "pLong" : _prefix + "PLong"); }
            set { SetValue(_prefix == "" ? "pLong" : _prefix + "PLong", value); }
        }

        public string LowerName
        {
            get { return GetString(_prefix == "" ? "pShort" : _prefix + "PShort"); }
            set { SetValue(_prefix == "" ? "pShort" : _prefix + "PShort", value); }
        }

        public bool CanEditName
        {
            get { return Type != 0; }
        }
    }
}
