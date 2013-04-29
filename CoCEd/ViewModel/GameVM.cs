using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
            Ass = new AssVM(file["ass"]);
            Cocks = new CockArrayVM(file["cocks"]);
            Breasts = new BreastArrayVM(file["breastRows"]);
            Vaginas = new VaginaArrayVM(file["vaginas"]);

            Vaginas.CollectionChanged += OnGenitalsCollectionChanged;
            Breasts.CollectionChanged += OnGenitalsCollectionChanged;
            Cocks.CollectionChanged += OnGenitalsCollectionChanged;

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


            NosePiercing = new PiercingVM(_node, "nose");


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

        public AssVM Ass
        {
            get;
            private set;
        }

        public CockArrayVM Cocks
        {
            get;
            private set;
        }

        public BreastArrayVM Breasts
        {
            get;
            private set;
        }

        public VaginaArrayVM Vaginas
        {
            get;
            private set;
        }

        public ItemSlotGroupVM[] ItemGroups
        {
            get;
            private set;
        }

        public PerkGroupVM[] PerkGroups
        {
            get;
            private set;
        }

        public PiercingVM NosePiercing
        {
            get;
            private set;
        }

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

    public class CockVM : NodeVM
    {
        public CockVM(AmfNode node)
            : base(node)
        {
        }

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


    public class VaginaVM : NodeVM
    {
        public VaginaVM(AmfNode node)
            : base(node)
        {
        }

        public XmlEnum[] AllTypes
        {
            get { return XmlData.Instance.Body.VaginaTypes; }
        }

        public XmlEnum[] AllLoosenessLevels
        {
            get { return XmlData.Instance.Body.VaginalLoosenessLevels; }
        }

        public XmlEnum[] AllWetnessLevels
        {
            get { return XmlData.Instance.Body.VaginalWetnessLevels; }
        }

        public int Type
        {
            get { return GetInt("type"); }
            set { SetValue("type", value);  }
        }

        public int Looseness
        {
            get { return GetInt("vaginalLooseness"); }
            set { SetValue("vaginalLooseness", value); }
        }

        public int Wetness
        {
            get { return GetInt("vaginalWetness"); }
            set { SetValue("vaginalWetness", value); }
        }

        public bool Virgin
        {
            get { return GetBool("virgin"); }
            set { SetValue("virgin", value); }
        }

        public string Description
        {
            get { return "One Vagina"; }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            base.OnPropertyChanged("Description");
        }
    }

    public sealed class VaginaArrayVM : ArrayVM<VaginaVM>
    {
        public VaginaArrayVM(AmfNode node)
            : base(node, x => new VaginaVM(x))
        {
        }

        protected override AmfNode CreateNewNode()
        {
            var node = new AmfArray();

            node["clipPLong"] = "";
            node["clitPShort"] = "";
            node["clitPierced"] = false;

            node["labiaPLong"] = "";
            node["labiaPShort"] = "";
            node["labiaPierced"] = false;

            node["type"] = 0;
            node["virgin"] = true;
            node["vaginalWetness"] = 1;
            node["vaginalLooseness"] = 0;
            return node;
        }
    }

    public sealed class ItemSlotGroupVM
    {
        readonly List<ItemSlotVM> _slots = new List<ItemSlotVM>();

        public ItemSlotGroupVM(string name, ItemCategories categories)
        {
            Name = name;
            Categories = categories;
        }

        public string Name
        {
            get;
            private set;
        }

        public ItemCategories Categories
        {
            get;
            private set;
        }

        public IEnumerable<ItemSlotVM> Slots
        {
            get { return _slots; }
        }

        public void Add(AmfNode node)
        {
            _slots.Add(new ItemSlotVM(node, Categories));
        }
    }

    public sealed class ItemSlotVM : NodeVM
    {
        public ItemSlotVM(AmfNode node, ItemCategories categories)
            : base(node)
        {
            Categories = categories;
        }

        public ItemCategories Categories
        {
            get;
            private set;
        }

        public IEnumerable<ItemGroupVM> AllGroups
        {
            get 
            {
                foreach(var group in XmlData.Instance.ItemGroups)
                {
                    if (Categories.HasFlag(group.Category)) yield return new ItemGroupVM(group, this);
                }
            }
        }

        public int Quantity
        {
            get { return GetInt("quantity"); }
            set 
            {
                if (!SetValue("quantity", value)) return;
                OnPropertyChanged("TypeDescription");
                OnPropertyChanged("QuantityDescription");
            }
        }

        public string Type
        {
            get { return GetString("shortName"); }
            set
            {
                if (!SetValue("shortName", value)) return;
                OnPropertyChanged("TypeDescription");
                OnPropertyChanged("QuantityDescription");
            }
        }

        public string TypeDescription
        {
            get
            {
                var type = XmlData.Instance.ItemGroups.SelectMany(x => x.Items).FirstOrDefault(x => x.ID == Type);
                if (Quantity == 0 || type == null) return "<empty>";
                return type.Name;
            }
        }

        public string QuantityDescription
        {
            get
            {
                var type = XmlData.Instance.ItemGroups.SelectMany(x => x.Items).FirstOrDefault(x => x.ID == Type);
                if (Quantity == 0 || type == null) return "";
                return "x" + Quantity.ToString();
            }
        }
    }


    public sealed class ItemGroupVM
    {
        const int Columns = 3;
        readonly ItemVM[] _items;

        public ItemGroupVM(XmlItemGroup group, ItemSlotVM slot)
        {
            Name = group.Name;
            _items = group.Items.OrderBy(x => x.Name).Select(x => new ItemVM(slot, x)).ToArray();
        }

        public string Name
        {
            get;
            private set;
        }

        public IEnumerable<ItemVM> Items
        {
            get { return _items; }
        }
    }

    public sealed class ItemVM : BindableBase
    {
        readonly ItemSlotVM _slot;
        readonly XmlEnumWithStringID _item;

        public ItemVM(ItemSlotVM slot, XmlEnumWithStringID item)
        {
            _slot = slot;
            _item = item;
        }

        public string Name
        {
            get { return _item.Name; }
        }

        public bool IsSelected
        {
            get { return _slot.Type == _item.ID; }
            set
            {
                if (value)
                {
                    _slot.Type = _item.ID;
                    if (_slot.Quantity == 0) _slot.Quantity = 1;
                }
                else if (_slot.Type == _item.ID)
                {
                    _slot.Type = "";
                }
                OnPropertyChanged();
                VM.Instance.NotifySaveRequiredChanged();
            }
        }
    }

    public sealed class PerkGroupVM
    {
        public PerkGroupVM(string name, AmfNode character, IEnumerable<XmlPerk> perks)
        {
            Name = name;
            Perks = perks.OrderBy(x => x.Name).Select(x => new PerkVM(character["perks"] as AmfNode, x)).ToArray();
        }

        public string Name
        {
            get;
            private set;
        }

        public PerkVM[] Perks
        {
            get;
            private set;
        }
    }

    public sealed class PerkVM : BindableBase
    {
        readonly AmfNode _perksArray;
        readonly XmlPerk _xml;

        public PerkVM(AmfNode perksArray, XmlPerk xml)
        {
            _perksArray = perksArray;
            _xml = xml;
        }

        public string Name
        {
            get { return _xml.Name; }
        }

        public string Description
        {
            get { return String.IsNullOrEmpty(_xml.Description) ? "<no description>" : _xml.Description; }
        }

        public bool IsOwned
        {
            get { return Pair != null; }
            set
            {
                var pair = Pair;
                if (value == (pair != null)) return;
                if (value)
                {
                    AmfNode perk = new AmfArray();
                    perk["perkDesc"] = _xml.Description;
                    perk["perkName"] = _xml.Name;
                    perk["value1"] = _xml.Value1;
                    perk["value2"] = _xml.Value2;
                    perk["value3"] = _xml.Value3;
                    perk["value4"] = _xml.Value4;
                    _perksArray.Add(perk);
                }
                else
                {
                    object removed;
                    _perksArray.Remove(pair.Key, true, out removed);
                }
                OnPropertyChanged("IsOwned");
                VM.Instance.NotifySaveRequiredChanged();
            }
        }

        public AmfPair Pair
        {
            get { return _perksArray.FirstOrDefault(x => String.Equals((x.Value as AmfNode)["perkName"] as string, _xml.Name, StringComparison.InvariantCultureIgnoreCase)); }
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
            get { return GetInt(_prefix + "Pierced"); }
            set 
            { 
                SetValue(_prefix + "Pierced", value);
                OnPropertyChanged("CanEditName");
            }
        }

        public string UpperName
        {
            get { return GetString(_prefix + "PLong"); }
            set { SetValue(_prefix + "PLong", value); }
        }

        public string LowerName
        {
            get { return GetString(_prefix + "PShort"); }
            set { SetValue(_prefix + "PShort", value); }
        }

        public bool CanEditName
        {
            get { return Type != 0; }
        }
    }
}
