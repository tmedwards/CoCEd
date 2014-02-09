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
        readonly List<PerkVM> _allPerks = new List<PerkVM>();

        ItemContainerVM _chest;
        ItemContainerVM _armorRack;
        ItemContainerVM _weaponRack;
        ItemContainerVM _inventory;

        public GameVM(AmfFile file, GameVM previousVM)
            : base(file)
        {
            if (previousVM != null)
            {
                _itemSearchText = previousVM._itemSearchText;
                _perkSearchText = previousVM._perkSearchText;
                _rawDataSearchText = previousVM._rawDataSearchText;
                _keyItemSearchText = previousVM._keyItemSearchText;
            }

            // Unique children
            Ass = new AssVM(file.GetObj("ass"));
            LipPiercing = new PiercingVM(file, "lip", PiercingLocation.Lip);
            NosePiercing = new PiercingVM(file, "nose", PiercingLocation.Nose);
            EarsPiercing = new PiercingVM(file, "ears", PiercingLocation.Ears);
            EyebrowPiercing = new PiercingVM(file, "eyebrow", PiercingLocation.Eyebrow);
            NipplesPiercing = new PiercingVM(file, "nipples", PiercingLocation.Nipples);
            TonguePiercing = new PiercingVM(file, "tongue", PiercingLocation.Tongue);


            // Collections
            Cocks = new CockArrayVM(file.GetObj("cocks"));
            Vaginas = new VaginaArrayVM(file.GetObj("vaginas"));
            Breasts = new BreastArrayVM(file.GetObj("breastRows"));
            Vaginas.CollectionChanged += OnGenitalCollectionChanged;
            Breasts.CollectionChanged += OnGenitalCollectionChanged;
            Cocks.CollectionChanged += OnGenitalCollectionChanged;


            // Flags
            int numFlags = XmlData.Instance.Flags.Max(x => x.ID) + 200;
            var xmlFlagByID = new XmlEnum[numFlags];
            foreach(var xml in XmlData.Instance.Flags) xmlFlagByID[xml.ID] = xml;

            var flagsArray = GetObj("flags");
            if (flagsArray == null) 
            {
                // For very old versions of CoC
                _obj["flags"] = flagsArray = new AmfObject(AmfTypes.Array);
                for (int i = 0; i < 3000; ++i) flagsArray.Push(0);
            }
            _allFlags = new FlagVM[numFlags];
            for (int i = 0; i < _allFlags.Length; ++i) _allFlags[i] = new FlagVM(this, flagsArray, xmlFlagByID[i], i);
            Flags = new UpdatableCollection<FlagVM>(_allFlags.Where(x => x.Index > 0 && x.Match(_rawDataSearchText)));


            // Statuses
            var cocStatuses = file.GetObj("statusAffects");
            var xmlStatuses = XmlData.Instance.Statuses;
            ImportMissingNamedVectors(cocStatuses, xmlStatuses, "statusAffectName");
            _allStatuses = XmlData.Instance.Statuses.OrderBy(x => x.Name).Select(x => new StatusVM(this, cocStatuses, x)).ToArray();
            Statuses = new UpdatableCollection<StatusVM>(_allStatuses.Where(x => x.Match(_rawDataSearchText)));


            // KeyItems
            var cocKeys = file.GetObj("keyItems");
            var xmlKeys = XmlData.Instance.KeyItems;
            ImportMissingNamedVectors(cocKeys, xmlKeys, "keyName");
            var allKeyitems = XmlData.Instance.KeyItems.OrderBy(x => x.Name).Select(x => new KeyItemVM(this, cocKeys, x)).ToArray();
            KeyItems = new UpdatableCollection<KeyItemVM>(allKeyitems.Where(x => x.Match(_keyItemSearchText)));


            // Perks
            var cocPerks = _obj.GetObj("perks");
            var xmlPerks = XmlData.Instance.PerkGroups.SelectMany(x => x.Perks).ToArray();
            var unknownPerkGroup = XmlData.Instance.PerkGroups.Last();
            ImportMissingNamedVectors(cocPerks, xmlPerks, "id", null, unknownPerkGroup.Perks);

            PerkGroups = new List<PerkGroupVM>();
            foreach (var xmlGroup in XmlData.Instance.PerkGroups)
            {
                var perksVM = xmlGroup.Perks.OrderBy(x => x.Name).Select(x => new PerkVM(this, cocPerks, x)).ToArray();
                _allPerks.AddRange(perksVM);

                var groupVM = new PerkGroupVM(this, xmlGroup.Name, perksVM);
                PerkGroups.Add(groupVM);
            }


            // Item containers
            var containers = new List<ItemContainerVM>();
            _inventory = new ItemContainerVM(this, "Inventory", ItemCategories.All);
            containers.Add(_inventory);
            UpdateInventory();

            _chest = new ItemContainerVM(this, "Chest", ItemCategories.All);
            containers.Add(_chest);
            UpdateChest();

            _armorRack = new ItemContainerVM(this, "Armor rack", ItemCategories.Armor | ItemCategories.ArmorCursed | ItemCategories.Unknown);
            containers.Add(_armorRack);
            UpdateArmorRack();

            _weaponRack = new ItemContainerVM(this, "Weapon rack", ItemCategories.Weapon | ItemCategories.Unknown);
            containers.Add(_weaponRack);
            UpdateWeaponRack();

            // Import missing items
            var unknownItemGroup = XmlData.Instance.ItemGroups.Last();

            foreach (var slot in containers.SelectMany(x => x.Slots))
            {
                // Add this item to the DB if it does not exist
                var type = slot.Type;
                if (String.IsNullOrEmpty(type)) continue;
                if (XmlData.Instance.ItemGroups.SelectMany(x => x.Items).Any(x => x.ID == type)) continue;

                var xml = new XmlItem { ID = type, Name = type };
                unknownItemGroup.Items.Add(xml);
            }
            foreach (var slot in containers.SelectMany(x => x.Slots)) slot.UpdateGroups();  // Update item groups after new items have been added

            // Complete slots creation
            ItemContainers = new UpdatableCollection<ItemContainerVM>(containers.Where(x => x.Slots.Count != 0));
        }

        static void ImportMissingNamedVectors(AmfObject cocItems, IEnumerable<XmlNamedVector4> xmlItems, string cocNameProperty, Func<AmfObject, String> descriptionGetter = null, IList<XmlNamedVector4> targetXmlList = null)
        {
            if (targetXmlList == null) targetXmlList = (IList<XmlNamedVector4>)xmlItems;
            var xmlNames = new HashSet<String>(xmlItems.Select(x => x.Name));

            foreach (var pair in cocItems)
            {
                var name = pair.ValueAsObject.GetString(cocNameProperty);
                if (xmlNames.Contains(name)) continue;
                xmlNames.Add(name);

                var xml = new XmlNamedVector4 { Name = name };
                if (descriptionGetter != null) xml.Description = descriptionGetter(pair.ValueAsObject);
                targetXmlList.Add(xml);
            }
        }

        public CockArrayVM Cocks { get; private set; }
        public BreastArrayVM Breasts { get; private set; }
        public VaginaArrayVM Vaginas { get; private set; }

        public UpdatableCollection<ItemContainerVM> ItemContainers { get; private set; }
        public UpdatableCollection<KeyItemVM> KeyItems { get; private set; }
        public UpdatableCollection<StatusVM> Statuses { get; private set; }
        public UpdatableCollection<FlagVM> Flags { get; private set; }
        public List<PerkGroupVM> PerkGroups { get; private set; }

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
            set 
            { 
                SetDouble("tou", value);
                OnPropertyChanged("MaxHP");
            }
        }

        public int Speed
        {
            get { return GetInt("spe"); }
            set 
            {
                SetDouble("spe", value);
            }
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

        public int MaxHP
        {
            get
            {
                var max = 50 + Toughness * 2 + Math.Min(20, Level) * 15;
                if (GetPerk("Tank 2").IsOwned) max += Toughness;
                if (GetPerk("Tank").IsOwned) max += 50;
                return max;
            }
        }

        public int XP
        {
            get { return GetInt("XP"); }
            set { SetValue("XP", value); }
        }

        public int Level
        {
            get { return GetInt("level"); }
            set 
            {
                SetValue("level", value);
                OnPropertyChanged("MaxHP");
                XP = 0;
            }
        }

        public int TeaseXP
        {
            get { return GetInt("teaseXP"); }
            set { SetValue("teaseXP", value); }
        }

        public int TeaseLevel
        {
            get { return GetInt("teaseLevel"); }
            set
            {
                SetValue("teaseLevel", value);
                TeaseXP = 0;
            }
        }

        public int PerkPoints
        {
            get { return GetInt("perkPoints"); }
            set { SetValue("perkPoints", value); }
        }

        public int Lust
        {
            get { return GetInt("lust"); }
            set 
            { 
                SetDouble("lust", value);
                OnPropertyChanged("CumProduction");
                OnPropertyChanged("CumVolume");
            }
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
                SetValue("hornType", value);
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
            get { return (HornType == 1 || HornType == 2 || HornType == 3 || HornType == 5); }
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
            set 
            { 
                SetValue("tailType", value);
                OnPropertyChanged("IsTailValueEnabled");
                OnPropertyChanged("TailValueLabel");
            }
        }

        public int TailValue
        {
            get { return GetInt("tailVenum"); }
            set { SetValue("tailVenum", value); }
        }

        public bool IsTailValueEnabled
        {
            get { return TailType == 13; }
        }

        public string TailValueLabel
        {
            get { return "Tail count"; }
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

        public bool HasGills
        {
            get { return GetBool("gills"); }
            set { SetValue("gills", value); }
        }



        public bool UsedContraceptives
        {
            get { return GetStatus("Contraceptives").IsOwned; }
            set { GetStatus("Contraceptives").IsOwned = value; }
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

                int eggType = (int)GetStatus("eggs").Value1;
                int eggSize = (int)GetStatus("eggs").Value2;
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

                    GetStatus("eggs").IsOwned = true;
                    GetStatus("eggs").Value1 = eggType;
                    GetStatus("eggs").Value2 = eggSize;
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
                SetValue("buttPregnancyType", value);
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
                SetValue("balls", value);
                OnPropertyChanged("CumProduction");
                OnPropertyChanged("CumVolume");
            }
        }

        public double BallSize
        {
            get { return GetDouble("ballSize"); }
            set
            {
                SetValue("ballSize", value);
                OnPropertyChanged("CumProduction");
                OnPropertyChanged("CumVolume");
            }
        }

        public double CumMultiplier
        {
            get { return GetDouble("cumMultiplier"); }
            set 
            {
                SetValue("cumMultiplier", value);
                OnPropertyChanged("CumProduction");
                OnPropertyChanged("CumVolume");
            }
        }

        public double HoursSinceCum
        {
            get { return GetDouble("hoursSinceCum"); }
            set 
            { 
               SetValue("hoursSinceCum", value);
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
                var baseQty = (Lust + 50) / 10;
                if (GetPerk("Pilgrim's Bounty").IsOwned) baseQty = 150 / 10;

                // Default values for balles (same as CoC)
                int balls = Balls;
                double ballSize = BallSize;
                if (balls == 0)
                {
                    balls = 2;
                    ballSize = 1.25;
                }

                var qty = (ballSize * balls * CumMultiplier * 2 * baseQty * (HoursSinceCum + 10) / 24) / 10;
                if (GetPerk("Bro Body").IsOwned) qty *= 1.3;
                if (GetPerk("Fertility+").IsOwned) qty *= 1.5;
                if (GetPerk("Messy Orgasms").IsOwned) qty *= 1.5;
                if (GetPerk("One Track Mind").IsOwned) qty *= 1.1;
                if (GetPerk("Marae's Gift - Stud").IsOwned) qty += 350;
                if (GetPerk("Fera's Boon - Alpha").IsOwned) qty += 200;
                if (GetPerk("Magical Virility").IsOwned) qty += 200;
                if (GetPerk("Bro Body").IsOwned) qty += 200;
                qty += GetPerk("Elven Bounty").Value1;
                qty += GetStatus("rut").Value1;
                qty *= 1 + 0.02 * GetPerk("Pierced: Fertite").Value1;

                return FormatVolume(qty);
            }
        }

        public string CumProduction
        {
            get
            {
                var baseQty = (Lust + 50) / 10;
                if (GetPerk("Pilgrim's Bounty").IsOwned) baseQty = 150 / 10;

                // Default values for balles (same as CoC)
                int balls = Balls;
                double ballSize = BallSize;
                if (balls == 0)
                {
                    balls = 2;
                    ballSize = 1.25;
                }

                var qty = (ballSize * balls * CumMultiplier * 2 * baseQty / 24) / 10;
                if (GetPerk("Bro Body").IsOwned) qty *= 1.3;
                if (GetPerk("Fertility+").IsOwned) qty *= 1.5;
                if (GetPerk("Messy Orgasms").IsOwned) qty *= 1.5;
                if (GetPerk("One Track Mind").IsOwned) qty *= 1.1;
                qty *= 1 + 0.02 * GetPerk("Pierced: Fertite").Value1;

                return FormatVolume(qty, "/h");
            }
        }

        public static string FormatVolume(double qty, string suffix = "")
        {
            if (qty <= 0) return "";
            if (qty <= 1000) return String.Format("{0:0} mL", qty) + suffix;
            if (qty <= 10000) return String.Format("{0:0.0} L", qty * 0.001) + suffix;
            return String.Format("{0:0} L", qty * 0.001) + suffix;
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
            get { return (int)GetStatus("Exgartuan").Value1; }
            set 
            {
                if (value == Exgartuan) return;
                GetStatus("Exgartuan").IsOwned = (value != 0);
                GetStatus("Exgartuan").Value1 = value;
            }
        }

        public double VaginalCapacityBonus
        {
            get { return GetStatus("Bonus vCapacity").Value1; }
            set
            {
                GetStatus("Bonus vCapacity").IsOwned = (value != 0);
                GetStatus("Bonus vCapacity").Value1 = value;
            }
        }

        public double AnalCapacityBonus
        {
            get { return GetStatus("Bonus aCapacity").Value1; }
            set
            {
                GetStatus("Bonus aCapacity").IsOwned = (value != 0);
                GetStatus("Bonus aCapacity").Value1 = value;
            }
        }

        public bool HasMetTamani
        {
            get { return GetStatus("Tamani").IsOwned; }
        }

        public int BirthedTamaniChildren
        {
            get { return (int)GetStatus("Tamani").Value2; }
            set { GetStatus("Tamani").Value2 = value; }
        }

        public int BirthedImps
        {
            get { return (int)GetStatus("Birthed Imps").Value1; }
            set
            {
                GetStatus("Birthed Imps").IsOwned = (value != 0);
                GetStatus("Birthed Imps").Value1 = value;
            }
        }

        public int BirthedMinotaurs
        {
            get { return GetFlag(326).AsInt(); }
            set 
            { 
                GetFlag(326).SetValue(value);
                OnPropertyChanged();
            }
        }

        public int MinotaurCumAddiction
        {
            get { return GetFlag(18).AsInt(); }
            set { GetFlag(18).SetValue(value); }
        }

        public int MarbleMilkAddiction
        {
            get { return (int)GetStatus("Marble").Value2; }
            set { GetStatus("Marble").Value2 = value; }
        }

        public bool HasMetMarble
        {
            get { return GetStatus("Marble").IsOwned; }
        }

        public int RapierSkill
        {
            get { return GetFlag(137).AsInt(); }
            set { GetFlag(137).SetValue(value); }
        }

        public int ArcherySkill
        {
            get { return (int)GetStatus("Kelt").Value1; }
            set { GetStatus("Kelt").Value1 = value; }
        }

        public int KeltSubmissiveness
        {
            get 
            {
                double value = GetStatus("Kelt").Value2;
                return (int)Math.Round(value / 1.3);
            }
            set 
            {
                GetStatus("Kelt").Value2 = (int)Math.Round(value * 1.3);
            }
        }

        public bool HasMetKelt
        {
            get { return GetStatus("Kelt").IsOwned; }
        }

        public int WormStatus
        {
            get
            {
                if (GetStatus("infested").IsOwned) return 2;
                if (GetStatus("wormsOff").IsOwned) return 0;
                GetStatus("wormsOn");
                return 1;
            }
            set
            {
                if (value == WormStatus) return;

                GetStatus("wormsOn").IsOwned = (value >= 1);
                GetStatus("wormsOff").IsOwned = (value == 0);
                GetStatus("infested").IsOwned = (value == 2);
            }
        }

        public int HeatTime
        {
            get { return (int)GetStatus("heat").Value3; }
            set { SetHeatOrRutTime("heat", value); }
        }

        public int RutTime
        {
            get { return (int)GetStatus("rut").Value3; }
            set { SetHeatOrRutTime("rut", value); }
        }

        void SetHeatOrRutTime(string name, int time)
        {
            bool isOwned = (time > 0);
            var status = GetStatus(name);
            if (status.IsOwned != isOwned)
            {
                if (!isOwned) Libido -= (int)status.Value2;
                status.IsOwned = isOwned;
                if (isOwned) Libido += (int)status.Value2;
            }
            status.Value3 = time;
        }

        public int LustStickTime
        {
            get { return (int)GetStatus("Lust Stick Applied").Value1; }
            set 
            {
                var status = GetStatus("Lust Stick Applied");
                status.IsOwned = (value > 0);
                status.Value1 = value;
            }
        }

        public int ExploredForest
        {
            get { return GetInt("exploredForest"); }
            set { SetValue("exploredForest", value); }
        }

        public int ExploredDeepwoods
        {
            get { return (int)GetStatus("exploredDeepwoods").Value1; }
            set
            {
                var status = GetStatus("exploredDeepwoods");
                status.IsOwned = (value > 0);
                status.Value1 = value;
            }
        }

        public int ExploredLake
        {
            get { return GetInt("exploredLake"); }
            set { SetValue("exploredLake", value); }
        }

        public int ExploredDesert
        {
            get { return GetInt("exploredDesert"); }
            set { SetValue("exploredDesert", value); }
        }

        public int ExploredMountain
        {
            get { return GetInt("exploredMountain"); }
            set { SetValue("exploredMountain", value); }
        }

        public int ExploredHighMountain
        {
            get { return GetFlag(88).AsInt(); }
            set { GetFlag(88).SetValue(value); }
        }

        public int ExploredPlains
        {
            get { return GetFlag(131).AsInt(); }
            set { GetFlag(131).SetValue(value); }
        }

        public int ExploredSwamp
        {
            get { return GetFlag(272).AsInt(); }
            set { GetFlag(272).SetValue(value); }
        }

        public int ExploredBog
        {
            get { return GetFlag(1016).AsInt(); }
            set { GetFlag(1016).SetValue(value); }
        }

        public bool ExploredTelAdre
        {
            get { return GetStatus("Tel'Adre").Value1 >= 1; }
            set
            {
                GetStatus("Tel'Adre").IsOwned = value;
                if (value && !ExploredTelAdre) GetStatus("Tel'Adre").Value1 = 1.0;
            }
        }

        public bool ExploredBizarreBazaar
        {
            get { return GetFlag(211).AsInt() == 1; }
            set { GetFlag(211).SetValue(value ? 1 : 0); }
        }

        public bool ExploredFarm
        {
            get { return GetStatus("Met Whitney").Value1 == 2; }
            set
            {
                GetStatus("Met Whitney").IsOwned = value;
                if (value && !ExploredFarm) GetStatus("Met Whitney").Value1 = 2.0;
            }
        }

        public bool ExploredOwca
        {
            get { return GetFlag(506).AsInt() == 1; }
            set { GetFlag(506).SetValue(value ? 1 : 0); }
        }

        public bool ExploredTownRuins
        {
            get { return GetFlag(44).AsInt() == 1; }
            set { GetFlag(44).SetValue(value ? 1 : 0); }
        }

        public bool ExploredBarber
        {
            get { return GetStatus("hairdresser meeting").IsOwned; }
            set { GetStatus("hairdresser meeting").IsOwned = value; }
        }

        public bool ExploredBoat
        {
            get { return GetStatus("Boat Discovery").IsOwned; }
            set { GetStatus("Boat Discovery").IsOwned = value; }
        }

        public bool ExploredDungeonFactory
        {
            get { return GetStatus("Found Factory").IsOwned; }
            set { GetStatus("Found Factory").IsOwned = value; }
        }

        public bool ExploredDungeonDeepCave
        {
            get { return GetFlag(113).AsInt() == 1; }
            set { GetFlag(113).SetValue(value ? 1 : 0); }
        }

        public bool ExploredDungeonDesertCave
        {
            get { return GetFlag(856).AsInt() == 1; }
            set { GetFlag(856).SetValue(value ? 1 : 0); }
        }

        public bool HasSandTrapBalls
        {
            get { return GetStatus("Uniball").IsOwned; }
            set { GetStatus("Uniball").IsOwned = value; }
        }

        public bool HasSandTrapNipples
        {
            get { return GetStatus("Black Nipples").IsOwned; }
            set { GetStatus("Black Nipples").IsOwned = value; }
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        string _itemSearchText;
        public string ItemSearchText
        {
            get { return _itemSearchText; }
            set
            {
                if (_itemSearchText == value) return;
                _itemSearchText = value;
                foreach (var slot in ItemContainers.SelectMany(x => x.Slots)) slot.UpdateGroups();
                OnPropertyChanged();
            }
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
