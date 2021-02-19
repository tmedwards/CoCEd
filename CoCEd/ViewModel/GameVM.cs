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
        public ModType Type { get; private set; }

        public bool IsVanilla { get { return Type == ModType.Vanilla; } }
        public Visibility VanillaVisibility
        {
            get { return Type == ModType.Vanilla ? Visibility.Visible : Visibility.Collapsed; }
        }

        public bool IsRevamp { get { return Type == ModType.Revamp; } }
        public Visibility RevampVisibility
        {
            get { return Type == ModType.Revamp ? Visibility.Visible : Visibility.Collapsed; }
        }

        public bool IsXianxia { get { return Type == ModType.Xianxia; } }
        public Visibility XianxiaVisibility
        {
            get { return Type == ModType.Xianxia ? Visibility.Visible : Visibility.Collapsed; }
        }

        public bool IsRevampOrXianxia { get { return Type == ModType.Xianxia || Type == ModType.Revamp; } }
        public Visibility RevampOrXianxiaVisibility
        {
            get { return Type == ModType.Xianxia || Type == ModType.Revamp ? Visibility.Visible : Visibility.Collapsed; }
        }


        readonly FlagVM[] _allFlags;
        readonly StatusVM[] _allStatuses;
        readonly KeyItemVM[] _allKeyitems;
        readonly List<PerkVM> _allPerks = new List<PerkVM>();

        ItemContainerVM _chest;
        ItemContainerVM _weaponRack;
        ItemContainerVM _armorRack;
        ItemContainerVM _jewelryBox;
        ItemContainerVM _dresser;
        ItemContainerVM _shieldRack;
        ItemContainerVM _inventory;

        public GameVM(AmfFile file, GameVM previousVM, ModType modType)
            : base(file)
        {
            if (previousVM != null)
            {
                _itemSearchText = previousVM._itemSearchText;
                _perkSearchText = previousVM._perkSearchText;
                _rawDataSearchText = previousVM._rawDataSearchText;
                _keyItemSearchText = previousVM._keyItemSearchText;
            }


            // Is this save from Vanilla, Revamp, or Xianxia?
            Type = modType;


            // Unique children
            Ass = new AssVM(file.GetObj("ass"));
            LipPiercing = new PiercingVM(file, "lip", PiercingLocation.Lip);
            NosePiercing = new PiercingVM(file, "nose", PiercingLocation.Nose);
            EarsPiercing = new PiercingVM(file, "ears", PiercingLocation.Ears);
            EyebrowPiercing = new PiercingVM(file, "eyebrow", PiercingLocation.Eyebrow);
            NipplesPiercing = new PiercingVM(file, "nipples", PiercingLocation.Nipples);
            TonguePiercing = new PiercingVM(file, "tongue", PiercingLocation.Tongue);


            // Collections
            Cocks = new CockArrayVM(this, file.GetObj("cocks"));
            Vaginas = new VaginaArrayVM(file.GetObj("vaginas"));
            Breasts = new BreastArrayVM(this, file.GetObj("breastRows"));
            Vaginas.CollectionChanged += OnGenitalCollectionChanged;
            Breasts.CollectionChanged += OnGenitalCollectionChanged;
            Cocks.CollectionChanged += OnGenitalCollectionChanged;


            // Flags
            int numFlags = XmlData.Current.Flags.Max(x => x.ID) + 25; // was 200; I'm unsure if there's really a need for a buffer at all anymore
            var xmlFlagByID = new XmlEnum[numFlags];
            foreach(var xml in XmlData.Current.Flags) xmlFlagByID[xml.ID] = xml;

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
            var xmlStatuses = XmlData.Current.Statuses;
            ImportMissingNamedVectors(cocStatuses, xmlStatuses, "statusAffectName");
            _allStatuses = XmlData.Current.Statuses.OrderBy(x => x.Name).Select(x => new StatusVM(this, cocStatuses, x)).ToArray();
            Statuses = new UpdatableCollection<StatusVM>(_allStatuses.Where(x => x.Match(_rawDataSearchText)));


            // KeyItems
            var cocKeys = file.GetObj("keyItems");
            var xmlKeys = XmlData.Current.KeyItems;
            ImportMissingNamedVectors(cocKeys, xmlKeys, "keyName");
            _allKeyitems = XmlData.Current.KeyItems.OrderBy(x => x.Name).Select(x => new KeyItemVM(this, cocKeys, x)).ToArray();
            KeyItems = new UpdatableCollection<KeyItemVM>(_allKeyitems.Where(x => x.Match(_keyItemSearchText)));


            // Perks
            var cocPerks = _obj.GetObj("perks");
            var xmlPerks = XmlData.Current.PerkGroups.SelectMany(x => x.Perks).ToArray();
            var unknownPerkGroup = XmlData.Current.PerkGroups.Last();
            ImportMissingNamedVectors(cocPerks, xmlPerks, "id", null, unknownPerkGroup.Perks);

            PerkGroups = new List<PerkGroupVM>();
            foreach (var xmlGroup in XmlData.Current.PerkGroups)
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

            _chest = new ItemContainerVM(this, IsRevamp || IsXianxia ? "Chest(s)" : "Chest", ItemCategories.All);
            containers.Add(_chest);
            UpdateChest();

            _weaponRack = new ItemContainerVM(this, "Weapon rack", ItemCategories.Weapon | ItemCategories.Unknown);
            containers.Add(_weaponRack);
            UpdateWeaponRack();

            _armorRack = new ItemContainerVM(this, "Armor rack", ItemCategories.Armor | ItemCategories.ArmorCursed | ItemCategories.Unknown);
            containers.Add(_armorRack);
            UpdateArmorRack();

            if (IsRevamp || IsXianxia)
            {
                _shieldRack = new ItemContainerVM(this, "Shield rack", ItemCategories.Shield | ItemCategories.Unknown);
                containers.Add(_shieldRack);
                UpdateShieldRack();

                _dresser = new ItemContainerVM(this, "Dresser", ItemCategories.Undergarment | ItemCategories.Unknown);
                containers.Add(_dresser);
                UpdateDresser();

                _jewelryBox = new ItemContainerVM(this, "Jewelry box", ItemCategories.Jewelry | ItemCategories.Unknown);
                containers.Add(_jewelryBox);
                UpdateJewelryBox();
            }

            // Import missing items
            var unknownItemGroup = XmlData.Current.ItemGroups.Last();

            foreach (var slot in containers.SelectMany(x => x.Slots))
            {
                // Add this item to the DB if it does not exist
                var type = slot.Type;
                if (String.IsNullOrEmpty(type)) continue;
                if (XmlData.Current.ItemGroups.SelectMany(x => x.Items).Any(x => x.ID == type)) continue;

                var xml = new XmlItem { ID = type, Name = type };
                unknownItemGroup.Items.Add(xml);
            }
            foreach (var slot in containers.SelectMany(x => x.Slots)) slot.UpdateGroups(); // Update item groups after new items have been added

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
            set { SetValue("short", value); }
        }

        public string Notes
        {
            get
            {
                var notes = GetString("notes");
                // unfortunately, CoC uses two different sets of text for this same case
                return notes.Equals("no notes available.", StringComparison.OrdinalIgnoreCase) ? "" : notes;
            }
            set { SetValue("notes", String.IsNullOrWhiteSpace(value) ? "No notes available." : value); }
        }

        public int Gems
        {
            get { return GetInt("gems"); }
            set { SetValue("gems", value); }
        }

        public int SpiritStones
        {
            get { return GetFlag(2349).AsInt(); }
            set
            {
                GetFlag(2349).SetValue(value);
                OnPropertyChanged();
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

        public int Wisdom
        {
            get { return GetInt("wis", 0); }
            set { SetDouble("wis", value); }
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
                double tou = GetDouble("tou");
                double max = 50 + tou * 2;

                if (GetPerk("Tank").IsOwned) max += 50;
                if (GetPerk("Tank 2").IsOwned) max += (int)Math.Round(tou);
                if (GetPerk("Chi Reflow - Defense").IsOwned) max += 50; // value: classes\classes\Scenes\Places\TelAdre\UmasShop.as:NEEDLEWORK_DEFENSE_EXTRA_HP
                if (IsRevamp || IsXianxia) max += Level * 15;
                else max += Math.Min(20, Level) * 15;

                if (IsRevamp || IsXianxia)
                {
                    //if (jewelryEffectId == JewelryLib.MODIFIER_HP) max += jewelryEffectMagnitude; // value: classes\classes\Items\JewelryLib.as
                    //if (GetItem(GetString("jewelryId")).EffectId == 5) max += GetItem(GetString("jewelryId")).EffectMagnitude; // value: classes\classes\Items\JewelryLib.as
                    if (GetString("jewelryId") == "LifeRng") max += 25; // value: classes\classes\Items\JewelryLib.as

                    //max *= 1 + (countCockSocks("green") * 0.02);
                    max *= 1 + 0.02 * Cocks.Count(x => x.CockSock == "green");
                }

                max = (int)Math.Round(max);

                return Math.Min(IsRevamp || IsXianxia ? 9999 : 999, (int)max);
            }
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

        public int MaxLust
        {
            get
            {
                double max = 100;

                if (IsRevamp || IsXianxia)
                {
                    if (GetPerk("Improved Self-Control").IsOwned) max += 20;
                    if (GetPerk("Bro Body").IsOwned || GetPerk("Bimbo Body").IsOwned || GetPerk("Futa Form").IsOwned) max += 20;
                    if (GetPerk("Omnibus' Gift").IsOwned) max += 15;

                    var ascensionDesires = GetPerk("Ascension: Desires");
                    if (ascensionDesires.IsOwned) max += ascensionDesires.Value1 * 5;
                }

                return Math.Min(999, (int)max);
            }
        }

        public int Fatigue
        {
            get { return GetInt("fatigue"); }
            set { SetValue("fatigue", value); }
        }

        public int MaxFatigue
        {
            get
            {
                double max = 100;

                if (IsRevamp || IsXianxia)
                {
                    if (GetPerk("Improved Endurance").IsOwned) max += 20;

                    var ascensionEndurance = GetPerk("Ascension: Endurance");
                    if (ascensionEndurance.IsOwned) max += ascensionEndurance.Value1 * 5;
                }

                return Math.Min(999, (int)max);
            }
        }

        public int Soulforce
        {
            get { return GetInt("soulforce", 0); }
            set { SetValue("soulforce", value); }
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

        public string EyeColor
        {
            get { return GetString("eyeColor"); }
            set { SetValue("eyeColor", value); }
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
                return "Horns' length"; // 2
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
                if (HipRating >= 20) return IsMale ? "inhumanly-wide" : "broodmother";
                if (HipRating >= 15) return IsMale ? "voluptuous" : "child-bearing";
                if (HipRating >= 10) return IsMale ? "wide" : "curvy";
                if (HipRating >= 6)  return IsMale ? "ample" : "girly";
                if (HipRating >= 4)  return "well-formed";
                if (HipRating >= 2)  return "slender";
                return "boyish";
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
                if (ButtRating >= 20) return "colossal";
                if (ButtRating >= 16) return "huge";
                if (ButtRating >= 13) return "voluminous";
                if (ButtRating >= 10) return "spacious";
                if (ButtRating >= 8)  return "substantial";
                if (ButtRating >= 6)  return "shapely";
                if (ButtRating >= 4)  return "regular";
                if (ButtRating >= 2)  return "compact";
                return "very small";
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
                if (Frame >= 90) return "very wide";
                if (Frame >= 75) return "wide";
                if (Frame >= 60) return "slightly wide";
                if (Frame >= 40) return "average";
                if (Frame >= 25) return "moderately thin";
                if (Frame >= 10) return "narrow";
                return "lithe";
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
                if (Muscles > 90) return "perfectly defined";
                if (Muscles > 75) return "great";
                if (Muscles > 50) return "visible";
                if (Muscles > 25) return "average";
                return "untoned";
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
                if (Feminity >= 91) return "hyper-feminine";
                if (Feminity >= 81) return "gorgeous";
                if (Feminity >= 73) return "feminine";
                if (Feminity >= 66) return "nicely feminine";
                if (Feminity >= 56) return "feminine touch";
                if (Feminity >= 45) return "androgeneous";
                if (Feminity >= 35) return "barely masculine";
                if (Feminity >= 28) return "fairly masculine";
                if (Feminity >= 20) return "masculine";
                if (Feminity >= 10) return "handsome";
                return "hyper-masculine";
            }
        }

        public int SkinType
        {
            get { return GetInt("skinType"); }
            set
            {
                SetValue("skinType", value);
                OnPropertyChanged("IsFurEnabled");
            }
        }

        public string SkinBase
        {
            get { return GetString("skinBase"); }
            set
            {
                SetValue("skinBase", value);
            }
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

        public int RearBodyType
        {
            get { return GetInt("rearBody"); }
            set { SetValue("rearBody", value); }
        }

        public int LowerBodyType
        {
            get { return GetInt("lowerBody"); }
            set
            {
                SetValue("lowerBody", value);

                if (IsRevamp || IsXianxia)
                {
                    // Set the default `LegCount` value when the lower body type is changed.
                    switch (value)
                    {
                        case 3: // Naga
                        case 8: // Goo
                            LegCount = 1;
                            break;

                        case 11: // Pony
                            LegCount = 4;
                            break;

                        case 16: // Drider
                            LegCount = 8;
                            break;

                        default:
                            LegCount = 2;
                            break;
                    }
                    OnPropertyChanged("LegCount");
                    OnPropertyChanged("LegConfigs");
                    OnPropertyChanged("HasLegConfigs");
                }
            }
        }

        public int TailType
        {
            get { return GetInt("tailType"); }
            set
            {
                SetValue("tailType", value);
                OnPropertyChanged("TailValueLabel");
                OnPropertyChanged("IsTailValueEnabled");
                OnPropertyChanged("IsTailRechargeEnabled");
            }
        }

        public string TailValueLabel
        {
            get { return TailType == 13 ? "Tail count" : "Tail venom"; }
        }

        public int TailValue
        {
            get { return GetInt("tailVenum"); }
            set { SetValue("tailVenum", value); }
        }

        public int TailRecharge
        {
            get { return GetInt("tailRecharge"); }
            set { SetValue("tailRecharge", value); }
        }

        public bool IsTailValueEnabled
        {
            get { return (TailType == 5 || TailType == 6 || TailType == 13); }
        }

        public bool IsTailRechargeEnabled
        {
            get { return (TailType == 5 || TailType == 6); }
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

        public int GillType
        {
            get { return GetInt("gillType"); }
            set { SetValue("gillType", value); }
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

        public string CumVolume // See: classes\classes\Creature.as:cumQ()
        {
            get
            {
                double lustCoefficient = (GetPerk("Pilgrim's Bounty").IsOwned ? 150 : (Lust + 50)) / 10;

                // Default values for balls (same as CoC)
                int balls = Balls;
                double ballSize = BallSize;
                if (balls == 0)
                {
                    balls = 2;
                    ballSize = 1.25;
                }

                double qty = ((int)(ballSize * balls * CumMultiplier * 2 * lustCoefficient * (HoursSinceCum + 10) / 24)) / 10;

                if (GetPerk("Bro Body").IsOwned) qty *= 1.3;
                if (GetPerk("Fertility+").IsOwned) qty *= 1.5;
                if ((IsRevamp || IsXianxia) && GetPerk("Fertility-").IsOwned && Libido < 25) qty *= 0.7;
                if (GetPerk("Messy Orgasms").IsOwned) qty *= 1.5;
                if (GetPerk("One Track Mind").IsOwned) qty *= 1.1;

                if (GetPerk("Bro Body").IsOwned) qty += 200;
                if (GetPerk("Fera's Boon - Alpha").IsOwned) qty += 200;
                if (GetPerk("Fera's Boon - Seeder").IsOwned) qty += 1000;
                if (GetPerk("Magical Virility").IsOwned) qty += 200;
                if (GetPerk("Marae's Gift - Stud").IsOwned) qty += 350;
                qty += GetPerk("Elven Bounty").Value1;
                qty += GetStatus("rut").Value1;

                qty *= 1 + (2 * GetPerk("Pierced: Fertite").Value1) / 100;
                //if (IsRevampMod && jewelryEffectId == JewelryLib.MODIFIER_FERTILITY) qty *= 1 + jewelryEffectMagnitude / 100;
                //if (GetItem(GetString("jewelryId")).EffectId == 2) qty *= 1 + GetItem(GetString("jewelryId")).EffectMagnitude / 100; // value: classes\classes\Items\JewelryLib.as
                //if (GetString("jewelryId") == "FertRng") qty *= 1 + 20 / 100; // value: classes\classes\Items\JewelryLib.as
                if (GetString("jewelryId") == "FertRng") qty *= 1.2; // value: classes\classes\Items\JewelryLib.as

                if (qty < 2) qty = 2;
                if ((IsRevamp || IsXianxia) && qty > Int32.MaxValue) qty = Int32.MaxValue;

                return FormatVolume(qty);
            }
        }

        // [TheMadExile]
        // This is clearly based on CumVolume, but I'm unsure how this was devised.  All it does is remove the
        // additive modifiers, which doesn't make a whole lot of sense to me if this is actually supposed to
        // be showing production over time.  Maybe there was code like this in CoC before the "Great Open
        // Sourcing"?  There certainly isn't now, however, so….
        //
        // Anyway, as CumVolume has evolved, due to CoC's evolution, I've kept this updated, but honestly,
        // I think this either needs completely rewritten, probably by using the returned value of CumVolume as
        // a base, or should simply be removed.
        public string CumProduction
        {
            get
            {
                double lustCoefficient = (GetPerk("Pilgrim's Bounty").IsOwned ? 150 : (Lust + 50)) / 10;

                // Default values for balls (same as CoC)
                int balls = Balls;
                double ballSize = BallSize;
                if (balls == 0)
                {
                    balls = 2;
                    ballSize = 1.25;
                }

                double qty = ((int)(ballSize * balls * CumMultiplier * 2 * lustCoefficient / 24)) / 10;

                if (GetPerk("Bro Body").IsOwned) qty *= 1.3;
                if (GetPerk("Fertility+").IsOwned) qty *= 1.5;
                if ((IsRevamp || IsXianxia) && GetPerk("Fertility-").IsOwned && Libido < 25) qty *= 0.7;
                if (GetPerk("Messy Orgasms").IsOwned) qty *= 1.5;
                if (GetPerk("One Track Mind").IsOwned) qty *= 1.1;

                qty *= 1 + (2 * GetPerk("Pierced: Fertite").Value1) / 100;
                //if (IsRevampMod && jewelryEffectId == JewelryLib.MODIFIER_FERTILITY) qty *= 1 + jewelryEffectMagnitude / 100;
                //if (GetItem(GetString("jewelryId")).EffectId == 2) qty *= 1 + GetItem(GetString("jewelryId")).EffectMagnitude / 100; // value: classes\classes\Items\JewelryLib.as
                //if (GetString("jewelryId") == "FertRng") qty *= 1 + 20 / 100; // value: classes\classes\Items\JewelryLib.as
                if (GetString("jewelryId") == "FertRng") qty *= 1.2; // value: classes\classes\Items\JewelryLib.as

                // unsure if clamping should be done here
                //if (qty < 2) qty = 2;
                //if (IsRevampMod && qty > Int32.MaxValue) qty = Int32.MaxValue;

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
            set
            {
                GetFlag(137).SetValue(value);
                if (IsRevamp || IsXianxia)
                {
                    // CoC-Revamp-Mod also uses this to determine if the "Rapier Training" perk is awarded to the player
                    GetPerk("Rapier Training").IsOwned = value >= 4;
                }
            }
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

        public bool UnlockedTelAdre
        {
            get { return GetStatus("Tel'Adre").Value1 >= 1; }
            set
            {
                GetStatus("Tel'Adre").IsOwned = value;
                if (value && !UnlockedTelAdre) GetStatus("Tel'Adre").Value1 = 1;
            }
        }

        public bool UnlockedBizarreBazaar
        {
            get { return GetFlag(211).AsInt() == 1; }
            set { GetFlag(211).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedFarm
        {
            get
            {
                if (IsRevamp || IsXianxia)
                {
                    // CoC-Revamp-Mod uses this to track achievement progress as well, so values ≥ 2 are inevitable
                    return GetStatus("Met Whitney").Value1 >= 2;
                }
                else // is vanilla CoC
                {
                    return GetStatus("Met Whitney").Value1 == 2;
                }
            }
            set
            {
                GetStatus("Met Whitney").IsOwned = value;
                if (value && !UnlockedFarm) GetStatus("Met Whitney").Value1 = 2;
            }
        }

        public bool UnlockedOwca
        {
            get { return GetFlag(506).AsInt() == 1; }
            set { GetFlag(506).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedTownRuins
        {
            get { return GetFlag(44).AsInt() == 1; }
            set { GetFlag(44).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedSalon
        {
            get { return GetStatus("hairdresser meeting").IsOwned; }
            set { GetStatus("hairdresser meeting").IsOwned = value; }
        }

        public bool UnlockedBoat
        {
            get { return GetStatus("Boat Discovery").IsOwned; }
            set { GetStatus("Boat Discovery").IsOwned = value; }
        }

        public bool UnlockedCathedral
        {
            get { return GetFlag(1165).AsInt() == 1; }
            set { GetFlag(1165).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedOasisTower
        {
            get { return GetFlag(821).AsInt() == 1; }
            set { GetFlag(821).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedShrine
        {
            get { return GetFlag(2490).AsInt() == 1; }
            set { GetFlag(2490).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedTemple
        {
            get { return GetFlag(2443).AsInt() == 1; }
            set { GetFlag(2443).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedWinterGear
        {
            get { return GetFlag(2619).AsInt() == 1; }
            set { GetFlag(2619).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedDungeonFactory
        {
            get
            {
                if (IsRevamp || IsXianxia)
                {
                    return GetFlag(2020).AsInt() == 1;
                }
                else // is vanilla CoC
                {
                    return GetStatus("Found Factory").IsOwned;
                }
            }
            set
            {
                if (IsRevamp || IsXianxia)
                {
                    GetFlag(2020).SetValue(value ? 1 : 0);
                }
                else // is vanilla CoC
                {
                    GetStatus("Found Factory").IsOwned = value;
                }
            }
        }

        public bool UnlockedDungeonDeepCave
        {
            get { return GetFlag(113).AsInt() == 1; }
            set { GetFlag(113).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedDungeonStronghold
        {
            get { return GetFlag(1239).AsInt() == 1; }
            set { GetFlag(1239).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedDungeonDesertCave
        {
            get { return GetFlag(856).AsInt() == 1; }
            set { GetFlag(856).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedDungeonHiddenCave
        {
            get { return GetFlag(2467).AsInt() == 1; }
            set { GetFlag(2467).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedDungeonDenofDesire
        {
            get { return GetFlag(2532).AsInt() == 1; }
            set { GetFlag(2532).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedDungeonEbonLabyrinth
        {
            get { return GetFlag(1239).AsInt() == 1; }
            set { GetFlag(1239).SetValue(value ? 1 : 0); }

        }

        public bool UnlockedLumisLab
        {
            get { return GetFlag(53).AsInt() == 1; }
            set { GetFlag(53).SetValue(value ? 1 : 0); }

        }

        public bool UnlockedAnzusPalace
        {
            get { return GetFlag(2505).AsInt() == 1; }
            set { GetFlag(2505).SetValue(value ? 1 : 0); }

        }


        //SoulSense locations

        public bool UnlockedHexindao
        {
            get { return GetFlag(2294).AsInt() == 1; }
            set { GetFlag(2294).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedSenseGiacomo
        {
            get { return GetFlag(2487).AsInt() >= 3; }
            set { GetFlag(2487).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedSenseTamani
        {
            get { return GetFlag(2461).AsInt() >= 3; }
            set { GetFlag(2461).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedSenseTamaniD
        {
            get { return GetFlag(2462).AsInt() >= 3; }
            set { GetFlag(2462).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedSensePriscilla
        {
            get { return GetFlag(2488).AsInt() >= 3; }
            set { GetFlag(2488).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedSenseKitMansion
        {
            get { return GetFlag(2463).AsInt() >= 3; }
            set { GetFlag(2463).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedSenseIzumi
        {
            get { return GetFlag(2464).AsInt() >= 3; }
            set { GetFlag(2464).SetValue(value ? 1 : 0); }
        }

        public bool UnlockedSenseWorldTree
        {
            get { return GetFlag(2486).AsInt() == 1; }
            set { GetFlag(2486).SetValue(value ? 1 : 0); }
        }


        #region Revamp Specific

        public int ClawType
        {
            get { return GetInt("clawType"); }
            set {
                if (IsRevamp || IsXianxia)
                {
                    SetValue("clawType", value);
                }
            }
        }

        public string ClawTone
        {
            get { return GetString("clawTone"); }
            set {
                if (IsRevamp || IsXianxia)
                {
                    SetValue("clawTone", value);
                }
            }
        }

        public int LegCount
        {
            get { return GetInt("legCount"); }
            set
            {
                if (IsRevamp || IsXianxia)
                {
                    SetValue("legCount", value);
                }
            }
        }

        // Handles biped and quadruped leg configurations.
        public int LegConfigs
        {
            get
            {
                if (!HasLegConfigs) return -1;
                return (LegCount / 2) - 1;
            }
            set
            {
                if (IsRevamp || IsXianxia)
                {
                    if (value == LegConfigs) return;
                    LegCount = (value + 1) * 2;
                }
            }
        }

        public bool HasLegConfigs
        {
            get
            {
                if (!IsRevamp && !IsXianxia) return false;
                switch (LowerBodyType)
                {
                    // Types which definitely have only a single allowed leg configuration.
                    case  0: // Human (biped)
                    case  3: // Naga (uniped)
                    case  8: // Goo (uniped)
                    case 11: // Pony (quadruped)
                    case 16: // Drider (octoped) → Biped form is lower body type #15 (Chitinous spider legs).
                        return false;

                    // Types which probably should have only a single allowed leg configuration.
                    case  5: // Demonic high-heels (biped)    → Funny human legs, should have the same limitations as lower body type #0 (Human).
                    case  6: // Demonic claws (biped)         → (same as the above)
                    case  7: // Bee (biped)                   → (same as the above)
                    case 13: // Harpy (biped)                 → (same as the above)
                    case 15: // Chitinous spider legs (biped) → Octoped form is lower body type #16 (Drider).
                        return false;

                    // Types which I'm unsure about, but I'm allowing because the game currently does.
                    // #14 Kangaroo (triped w/ pentapedal & bipedal-hopping locomotion) → Unsure.  Kangaroos are weird.

                    // All other types may have either biped or quadruped leg configurations.
                    default:
                        return true;
                }
            }
        }

        public string FurColor
        {
            get { return GetString("furColor"); }
            set
            {
                if (IsRevamp || IsXianxia)
                {
                    SetValue("furColor", value);
                }
            }
        }

        public bool IsFurEnabled
        {
            get { return SkinType == 1; }
        }

        public double Hunger
        {
            get { return IsRevamp || IsXianxia ? GetDouble("hunger") : 0.0; }
            set
            {
                if (IsRevamp || IsXianxia)
                {
                    SetValue("hunger", value);
                    OnPropertyChanged("HungerTip");
                }
            }
        }
        public string HungerTip
        {
            get
            {
                if (Hunger >= 100) return "very full";
                if (Hunger >= 90)  return "full";
                if (Hunger >= 75)  return "satiated";
                if (Hunger >= 50)  return "not hungry";
                if (Hunger >= 25)  return "hungry";
                if (Hunger >= 10)  return "very hungry";
                if (Hunger > 0)    return "starving";
                return "dying";
            }
        }

        public int BeardType
        {
            get { return GetInt("beardStyle"); }
            set
            {
                if (IsRevamp || IsXianxia)
                {
                    SetValue("beardStyle", value);
                }
            }
        }

        public double BeardLength
        {
            get { return GetDouble("beardLength"); }
            set
            {
                if (IsRevamp || IsXianxia)
                {
                    SetValue("beardLength", value);
                }
            }
        }

        public int ExploredGlacialRift
        {
            get { return IsRevamp || IsXianxia ? GetFlag(2059).AsInt() : 0; }
            set
            {
                if (IsRevamp || IsXianxia)
                {
                    GetFlag(2059).SetValue(value);
                }
            }
        }
        
        public int ExploredVolcanicCrag
        {
            get { return IsRevamp || IsXianxia ? GetFlag(2060).AsInt() : 0; }
            set
            {
                if (IsRevamp || IsXianxia)
                {
                    GetFlag(2060).SetValue(value);
                }
            }
        }

        public int ExploredOuterBattlefield
        {
            get { return IsXianxia ? GetFlag(2285).AsInt() : 0; }
            set
            {
                if (IsXianxia)
                {
                    GetFlag(2285).SetValue(value);
                }
            }
        }

        public int ExploredBlightRidge
        {
            get { return IsXianxia ? GetFlag(2284).AsInt() : 0; }
            set
            {
                if (IsXianxia)
                {
                    GetFlag(2284).SetValue(value);
                }
            }
        }

        public int ExploredCaves
        {
            get { return IsXianxia ? GetFlag(2667).AsInt() : 0; }
            set
            {
                if (IsXianxia)
                {
                    GetFlag(2667).SetValue(value);
                }
            }
        }

        public int ExploredBeach
        {
            get { return IsXianxia ? GetFlag(2290).AsInt() : 0; }
            set
            {
                if (IsXianxia)
                {
                    GetFlag(2290).SetValue(value);
                }
            }
        }

        public int ExploredOcean
        {
            get { return IsXianxia ? GetFlag(2291).AsInt() : 0; }
            set
            {
                if (IsXianxia)
                {
                    GetFlag(2291).SetValue(value);
                }
            }
        }

        public int ExploredDeepSea
        {
            get { return IsXianxia ? GetFlag(2292).AsInt() : 0; }
            set
            {
                if (IsXianxia)
                {
                    GetFlag(2292).SetValue(value);
                }
            }
        }
        #endregion


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
