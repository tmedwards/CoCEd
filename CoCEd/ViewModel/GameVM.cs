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
    public sealed partial class GameVM : ObjectVM
    {
        readonly FlagVM[] _allFlags;
        readonly StatusVM[] _allStatuses;

        public GameVM(AmfFile file)
            : base(file)
        {
            // Unique children
            Ass = new AssVM(file.GetObj("ass"));
            LipPiercing = new PiercingVM(_obj, "lip");
            NosePiercing = new PiercingVM(_obj, "nose");
            EarsPiercing = new PiercingVM(_obj, "ears");
            EyebrowPiercing = new PiercingVM(_obj, "eyebrow");
            NipplesPiercing = new PiercingVM(_obj, "nipples");
            TonguePiercing = new PiercingVM(_obj, "tongue");


            // Collections
            Cocks = new CockArrayVM(file.GetObj("cocks"));
            Vaginas = new VaginaArrayVM(file.GetObj("vaginas"));
            Breasts = new BreastArrayVM(file.GetObj("breastRows"));
            Vaginas.CollectionChanged += OnGenitalCollectionChanged;
            Breasts.CollectionChanged += OnGenitalCollectionChanged;
            Cocks.CollectionChanged += OnGenitalCollectionChanged;


            // Item containers
            var containers = new List<ItemContainerVM>();
            var container = new ItemContainerVM("Inventory", ItemCategories.All);
            for (int i = 0; i < 5; i++) container.Add(file.GetObj("itemSlot" + (i + 1)));
            containers.Add(container);

            container = new ItemContainerVM("Chest", ItemCategories.All);
            foreach(var pair in file.GetObj("itemStorage")) container.Add(pair.ValueAsObject);
            containers.Add(container);

            var gearStorage = file.GetObj("gearStorage");
            container = new ItemContainerVM("Armor rack", ItemCategories.Armor | ItemCategories.Unknown);
            for (int i = 0; i < 9; i++) container.Add(gearStorage.GetObj(i + 9));
            containers.Add(container);

            container = new ItemContainerVM("Weapon rack", ItemCategories.Weapon | ItemCategories.Unknown);
            for (int i = 0; i < 9; i++) container.Add(gearStorage.GetObj(i));
            containers.Add(container);

            ItemContainers = containers.ToArray();

            // Flags
            var flagsArray = GetObj("flags");
            var flagsData = new XmlEnum[flagsArray.Count];
            foreach(var flagData in XmlData.Instance.Flags) flagsData[flagData.ID - 1] = flagData;

            _allFlags = new FlagVM[flagsArray.Count];
            for (int i = 0; i < _allFlags.Length; ++i) _allFlags[i] = new FlagVM(flagsArray, flagsData[i], i + 1);
            Flags = new UpdatableCollection<FlagVM>(_allFlags.Where(x => x.Match(_rawDataSearchText)));


            // Statuses
            var statusArray = file.GetObj("statusAffects");
            ImportMissingNamedVector(statusArray, XmlData.Instance.Statuses, "statusAffectName");
            _allStatuses = XmlData.Instance.Statuses.OrderBy(x => x.Name).Select(x => new StatusVM(statusArray, x)).ToArray();
            Statuses = new UpdatableCollection<StatusVM>(_allStatuses.Where(x => x.Match(_rawDataSearchText)));


            // KeyItems
            var keyItemArray = file.GetObj("keyItems");
            ImportMissingNamedVector(keyItemArray, XmlData.Instance.KeyItems, "keyName");
            var allKeyitems = XmlData.Instance.KeyItems.OrderBy(x => x.Name).Select(x => new KeyItemVM(keyItemArray, x)).ToArray();
            KeyItems = new UpdatableCollection<KeyItemVM>(allKeyitems.Where(x => x.Match(_keyItemSearchText)));


            // Perks
            var perkArray = _obj.GetObj("perks");
            ImportMissingNamedVector(perkArray, XmlData.Instance.PerkGroups.SelectMany(x => x.Perks), "perkName", x => 
                {
                    var help = x.GetString("perkDesc");
                    return String.IsNullOrEmpty(help) ? "<no description>" : help;
                }, 
                XmlData.Instance.PerkGroups.Last().Perks);
            PerkGroups = XmlData.Instance.PerkGroups.Select(x => new PerkGroupVM(x.Name, perkArray, x.Perks)).ToArray();
        }

        static void ImportMissingNamedVector(AmfObject array, IEnumerable<XmlNamedVector4> xmlData, string nameKey, Func<AmfObject, String> descriptionGetter = null, IList<XmlNamedVector4> targetList = null)
        {
            if (targetList == null) targetList = (IList<XmlNamedVector4>)xmlData;
            var xmlNames = new HashSet<String>(xmlData.Select(x => x.Name));

            foreach (var pair in array)
            {
                var name = pair.ValueAsObject.GetString(nameKey);
                if (xmlNames.Contains(name)) continue;
                xmlNames.Add(name);

                var xml = new XmlNamedVector4 { Name = name };
                if (descriptionGetter != null) xml.Description = descriptionGetter(pair.ValueAsObject);
                targetList.Add(xml);
            }
        }

        public CockArrayVM Cocks { get; private set; }
        public BreastArrayVM Breasts { get; private set; }
        public VaginaArrayVM Vaginas { get; private set; }

        public UpdatableCollection<KeyItemVM> KeyItems { get; private set; }
        public UpdatableCollection<StatusVM> Statuses { get; private set; }
        public UpdatableCollection<FlagVM> Flags { get; private set; }
        public ItemContainerVM[] ItemContainers { get; private set; }
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

        public int Days
        {
            get { return GetInt("days"); }
            set { SetValue("days", value); }
        }

        public int Hours
        {
            get { return GetInt("hours"); }
            set { SetValue("hours", value); }
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

        public int HairType
        {
            get { return GetInt("hairType"); }
            set { SetValue("hairType", value); }
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
                OnPropertyChanged("HornsValueEnabled");
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
                if (HornType == 5) return "Antlers' branches";
                if (HornType == 1) return "Horn count";
                return "Horns length";  // 2
            }
        }

        public string HornsValueUnit
        {
            get { return HornType == 2 ? "inches" : ""; }
        }

        public bool HornsValueEnabled
        {
            get { return (HornType == 1 || HornType == 2 || HornType == 5); }
        }



        public double Height
        {
            get { return GetDouble("tallness"); }
            set { SetValue("tallness", value); }
        }

        public int HipRating
        {
            get { return GetInt("hipRating"); }
            set 
            { 
                SetValue("hipRating", value);
                OnPropertyChanged("HipRatingTip");
            }
        }

        public string HipRatingTip
        {
            get 
            {
                if (HipRating >= 20) return IsMale ? "extended" : "breeder";
                if (HipRating >= 15) return IsMale ? "large" : "mother";
                if (HipRating >= 10) return IsMale ? "feminine" : "sexy";
                if (HipRating >= 6) return "graceful";
                return "unremarkable";
            }
        }

        public int ButtRating
        {
            get { return GetInt("buttRating"); }
            set 
            { 
                SetValue("buttRating", value);
                OnPropertyChanged("ButtRatingTip");
            }
        }

        public string ButtRatingTip
        {
            get
            {
                if (ButtRating >= 20) return "obscene";
                if (ButtRating >= 15) return "bust out";
                if (ButtRating >= 10) return "enticing";
                if (ButtRating >= 6) return "nice";
                if (ButtRating >= 4) return "decent";
                return "unremarkable";
            }
        }

        public int Frame
        {
            get { return GetInt("thickness"); }
            set 
            { 
                SetValue("thickness", value);
                OnPropertyChanged("FrameTip");
            }
        }

        public string FrameTip
        {
            get
            {
                if (Frame >= 90) return "wide";
                if (Frame >= 60) return "thick";
                if (Frame >= 40) return "average";
                if (Frame >= 25) return "narrow";
                return "thin";
            }
        }

        public int Muscles
        {
            get { return GetInt("tone"); }
            set 
            { 
                SetValue("tone", value);
                OnPropertyChanged("MusclesTip");
            }
        }

        public string MusclesTip
        {
            get
            {
                if (Muscles > 90) return "rippling muscles";
                if (Muscles > 75) return "showing off";
                if (Muscles > 50) return "visible";
                if (Muscles > 25) return "average";
                return "soft";
            }
        }

        public int Feminity
        {
            get { return GetInt("femininity"); }
            set 
            { 
                SetValue("femininity", value);
                OnPropertyChanged("FeminityTip");
            }
        }

        public string FeminityTip
        {
            get
            {
                if (Feminity > 90) return "hyper-feminine";
                if (Feminity >= 80) return "gorgeous";
                if (Feminity >= 70) return "feminine";
                if (Feminity >= 55) return "feminine touch";
                if (Feminity >= 45) return "androgeneous";
                if (Feminity >= 35) return "barely masculine";
                if (Feminity >= 20) return "handsome";
                return "hyper-masculine";
            }
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

        public string SkinDescription
        {
            get { return GetString("skinDesc"); }
            set { SetValue("skinDesc", value); }
        }

        public string SkinAdjective
        {
            get { return GetString("skinAdj"); }
            set { SetValue("skinAdj", value); }
        }

        public int ArmType
        {
            get { return GetInt("armType"); }
            set { SetValue("armType", value); }
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
            set 
            { 
                SetValue("wingType", value);
                OnPropertyChanged("IsWingEnabled");
            }
        }

        public string WingDescription
        {
            get { return GetString("wingDesc"); }
            set { SetValue("wingDesc", value); }
        }

        public bool IsWingEnabled
        {
            get { return WingType != 0; }
        }


        public int Fertility
        {
            get { return GetInt("fertility"); }
            set { SetValue("fertility", value); }
        }

        public int PregnancyType
        {
            get 
            { 
                int type = GetInt("pregnancyType"); 
                if (type != 5) return type;

                int eggType = GetStatusInt("eggs", "1", 0);
                int eggSize = GetStatusInt("eggs", "2", 0);
                return 10000 + eggType * 100 + eggSize;
            }
            set 
            {
                if (value < 10000)
                {
                    SetValue("pregnancyType", value);
                }
                else
                {
                    value = value % 10000;
                    int eggType = value / 100;
                    int eggSize = value % 100;
                    EnsureStatusExists("eggs", eggType, eggSize, 6, 0);
                    SetStatusValue("eggs", "1", eggType);
                    SetStatusValue("eggs", "2", eggSize);
                    SetValue("pregnancyType", 5);
                }
                OnPropertyChanged("IsPregnancyEnabled");
            }
        }

        public int PregnancyTime
        {
            get { return GetInt("pregnancyIncubation"); }
            set { SetValue("pregnancyIncubation", value); }
        }

        public bool IsPregnancyEnabled
        {
            get { return PregnancyType != 0; }
        }

        public int ButtPregnancyType
        {
            get { return GetInt("buttPregnancyType"); }
            set 
            {
                if (!SetValue("buttPregnancyType", value)) return;
                OnPropertyChanged("IsButtPregnancyEnabled");
            }
        }

        public int ButtPregnancyTime
        {
            get { return GetInt("buttPregnancyIncubation"); }
            set { SetValue("buttPregnancyIncubation", value); }
        }

        public bool IsButtPregnancyEnabled
        {
            get { return ButtPregnancyType != 0; }
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

        public Visibility NippleVisibility
        {
            get { return Breasts.Count == 0 ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility ClitVisibility
        {
            get { return Vaginas.Count == 0 ? Visibility.Collapsed : Visibility.Visible; }
        }

        public int Exgartuan
        {
            get { return GetStatusInt("Exgartuan", "1", 0); }
            set 
            {
                if (value == Exgartuan) return;
                if (Exgartuan == 0) EnsureStatusExists("Exgartuan", value, 0, 0, 0);
                else if (value == 0) RemoveStatus("Exgartuan");
                else SetStatusValue("Exgartuan", "1", value);
            }
        }

        public double VaginalCapacityBonus
        {
            get { return GetStatusDouble("Bonus vCapacity", "1"); }
            set
            {
                EnsureStatusExists("Bonus vCapacity", value, 0, 0, 0);
                SetStatusValue("Bonus vCapacity", "1", value);
            }
        }

        public double AnalCapacityBonus
        {
            get { return GetStatusDouble("Bonus aCapacity", "1"); }
            set 
            {
                EnsureStatusExists("Bonus aCapacity", value, 0, 0, 0);
                SetStatusValue("Bonus aCapacity", "1", value); 
            }
        }

        public bool HasMetTamani
        {
            get { return HasStatus("Tamani"); }
        }

        public int BirthedTamaniChildren
        {
            get { return GetStatusInt("Tamani", "2"); }
            set { SetStatusValue("Tamani", "2", value); }
        }

        public int BirthedImps
        {
            get { return GetStatusInt("Birthed Imps", "1"); }
            set
            {
                EnsureStatusExists("Birthed Imps", value, 0, 0, 0);
                SetStatusValue("Birthed Imps", "1", value);
            }
        }

        public int BirthedMinotaurs
        {
            get { return GetFlagInt(326); }
            set { SetFlag(326, value); }
        }

        public int MinotaurCumAddiction
        {
            get { return GetFlagInt(18); }
            set { SetFlag(18, value); }
        }

        public int MarbleMilkAddiction
        {
            get { return GetStatusInt("Marble", "3", 0); }
            set { SetStatusValue("Marble", "3", value); }
        }

        public bool HasMetMarble
        {
            get { return HasStatus("Marble"); }
        }

        public int WormStatus
        {
            get
            {
                if (HasStatus("infested")) return 2;
                if (HasStatus("wormsOff")) return 0;
                RegisterStatusDependency("wormsOn");
                return 1;
            }
            set
            {
                if (value == WormStatus) return;

                if (value == 0) EnsureStatusExists("wormsOff",0,0,0,0);
                else RemoveStatus("wormsOff");

                if (value >= 1) EnsureStatusExists("wormsOn", 0, 0, 0, 0);
                else RemoveStatus("wormsOn");

                if (value == 2) EnsureStatusExists("infested", 0, 0, 0, 0);
                else RemoveStatus("infested");
            }
        }

        string _rawDataSearchText;
        public string RawDataSearchText
        {
            get { return _rawDataSearchText; }
            set
            {
                if (_rawDataSearchText == value) return;
                _rawDataSearchText = value;
                Statuses.Update();
                Flags.Update();
                base.OnPropertyChanged();
            }
        }

        string _perkSearchText;
        public string PerkSearchText
        {
            get { return _perkSearchText; }
            set
            {
                if (_perkSearchText == value) return;
                _perkSearchText = value;
                foreach (var group in PerkGroups) group.Update();
                base.OnPropertyChanged();
            }
        }

        string _keyItemSearchText;
        public string KeyItemSearchText
        {
            get { return _keyItemSearchText; }
            set
            {
                if (_keyItemSearchText == value) return;
                _keyItemSearchText = value;
                KeyItems.Update();
                base.OnPropertyChanged();
            }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            VM.Instance.NotifySaveRequiredChanged(true);
        }
    }

    public sealed class AssVM : ObjectVM
    {
        public AssVM(AmfObject obj)
            : base(obj)
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
}
