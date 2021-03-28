using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public class SkinLayerVM : BodyPartVM
    {
        private readonly string _name;
        public override string Name { get { return _name; } }

        public SkinLayerVM(GameVM game, AmfObject obj, string name) : base(game, obj) => _name = name;

        public string DefaultAdjective()
        {
            if (Type == 3) // Goo
            {
                return "goopey";
            }
            return "";
        }
        public string DefaultDescription()
        {
            var matches = AllTypes.Where(s => s.ID == Type);
            if (matches.Count() > 0)
            {
                return matches.First().Name.ToLower();
            }
            return "skin";
        }

        public override IEnumerable<XmlEnum> AllTypes
        {
            get
            {
                if (_game.IsXianxia && _name == "coat")
                {
                    //return blank coat type as well
                    yield return new XmlEnum() { Name = "", ID = 0 };
                }
                foreach (var type in XmlData.Current.Body.SkinTypes)
                {
                    if (_game.IsNotXianxia)
                        yield return type;
                    if (_name == "skin")
                    {
                        switch (type.ID)
                        {
                            case 0: //Plain
                            case 3: //Goo
                            case 7: //Stone
                            case 19: //Aqua-rubber like
                            case 21: //Feather
                            case 22: //Transparent
                                yield return type;
                                break;
                        }
                    }
                    else if (_name == "coat")
                    {
                        switch (type.ID)
                        {
                            case 1: //Fur
                            case 5: //Chitin
                            case 6: //Bark
                            case 9: //Aqua scales
                            case 14: //dragon scales
                            case 15: //moss
                                yield return type;
                                break;
                        }
                    }
                }

            }
        }

        public IEnumerable<XmlEnum> AllPatterns
        {
            get
            {
                /*if (_game.IsNotXianxia)
                {
                    yield return null;
                }*/
                if (_game.IsXianxia)
                {
                    foreach (var type in XmlData.Current.Body.SkinPatterns)
                    {
                        if (_game.IsNotXianxia)
                            yield return type;
                        if (_name == "skin")
                        {
                            switch (type.ID)
                            {
                                case 0: //None
                                case 1: //Magical Tattoo
                                case 2: //Orca Underbody
                                case 5: //Battle Tattoo
                                case 7: //Lightning chaped tattoo
                                case 9: //Scar shaped tattoo
                                case 10: //White/black veins
                                case 11: //Venomous markings
                                case 12: //USHI_ONI_ONNA_TATTOO
                                case 13: //SCAR_WINDSWEPT
                                case 14: //Oil
                                    yield return type;
                                    break;
                            }
                        }
                        else if (_name == "coat")
                        {
                            switch (type.ID)
                            {
                                case 0: //None
                                case 3: //Bee Stripes
                                case 4: //tiger Stripes
                                case 6: //Spotted
                                case 8: //Red Panda Underbody
                                    yield return type;
                                    break;
                            }
                        }
                    }
                }
            }
        }
        public override int Type
        {
            get {
                return GenericGetIntProp("type");
            }
            set
            {
                int currentType = Type;
                if (currentType != value)
                {
                    GenericSetProp("type", value);
                    Description = DefaultDescription();
                    Pattern = 0;
                    Adjective = DefaultAdjective();
                    OnPropertyChanged("Type");
                    OnPropertyChanged("Description");
                    OnPropertyChanged("Pattern");
                    OnPropertyChanged("Adjective");
                    _game.Skin.PokeCoatEnabled();
                }
            }
        }

        public string Color
        {
            get
            {
                if (_game.IsXianxia)
                {
                    var curColor = GetString("color");
                    if (curColor == null || curColor == "")
                    {
                        return _game.HairColor;
                    }
                    return GetString("color");
                }

                //if not xianxia
                if (Name == "skin")
                {
                    return _game.GetString("skinTone");
                }
                return _game.GetString("furColor");
            }
            set
            {
                if (_game.IsXianxia)
                {
                    var curColor = GetString("color");
                    SetValue("color", value);

                    // If there's no pattern, change color2 to equal color
                    if (Pattern == 0)
                    {
                        Color2 = Color;
                    }
                }
                // not xianxia
                else
                {
                    if (Name == "skin")
                    {
                        _game.SetValue("skinTone", value);
                    }
                    else
                    {
                        _game.SetValue("furColor", value);
                    }
                }
            }
        }
        public int Pattern
        {
            get { 
                if (_game.IsNotXianxia)
                {
                    return 0;
                }
                return GetInt("pattern", 0);
            }
            set
            {
                if (_game.IsXianxia)
                {
                    SetValue("pattern", value);
                    var matches = AllPatterns.Where(s => s.ID == Pattern);
                    if (matches.Count() > 0)
                    {
                        Adjective = matches.First().Description;
                        OnPropertyChanged("Adjective");
                        OnPropertyChanged("IsPatternEnabled");
                    }
                    if (value == 0)
                    {
                        Color2 = Color;
                    }
                    OnPropertyChanged("Color2");
                }
            }
        }
        public string Description
        {
            get
            {
                string desc = GenericGetStringProp("desc");
                if (desc == null)
                {
                    return DefaultDescription();
                }
                return desc;
            }
            set { GenericSetProp("desc", value); }

        }
        public string Adjective
        {
            get
            {
                string adj = GenericGetStringProp("adj");
                if (adj == null)
                {
                    return DefaultAdjective();
                }
                return adj;
            }
            set { GenericSetProp("adj", value); }
        }

        public string Color2
        {
            get
            {
                if (GetString("color2") == null || GetString("color2") == "")
                {
                    return Color;
                }
                return GetString("color2");
            }
            set
            {
                if (value == Color)
                {
                    SetValue("color2", "");
                }
                SetValue("color2", value);
            }
        }
        public bool IsPatternEnabled
        {
            get
            {
                return Pattern > 0;
            }
        }
    }

    public class SkinVM : BodyPartVM
    {
        //Do not use this
        public override int Type { get { throw new Exception("don't get skinVM type"); } set { throw new Exception("don't set skinVM type"); } }
        public override string Name { get { return "skin"; } }
        public SkinVM(GameVM game, AmfObject obj) : base(game, obj)
        {
            if (_obj == null || game.IsNotXianxia)
            {
                Base = new SkinLayerVM(game, null, "skin");
                Coat = new SkinLayerVM(game, null, "coat");
                return;
            }
            var _baseobj = GetObj("base");
            if (_baseobj != null)
            {
                Base = new SkinLayerVM(game, _baseobj, "skin");
            }
            else
            {
                Base = new SkinLayerVM(game, new AmfObject(AmfTypes.Dictionary), "skin");
                SetValue("base", Base.GetSelf());
            }
            var _coatobj = GetObj("coat");
            if (_coatobj != null)
            {
                Coat = new SkinLayerVM(game, _coatobj, "coat");
            }
            else
            {
                Coat = new SkinLayerVM(game, new AmfObject(AmfTypes.Dictionary), "coat");
                SetValue("coat", Coat.GetSelf());
            }
        }

        public override IEnumerable<XmlEnum> AllTypes
        {
            get
            {
                foreach (var type in XmlData.Current.Body.SkinTypes)
                {
                    yield return type;
                }
            }
        }
        public IEnumerable<XmlEnum> AllPatterns
        {
            get
            {
                foreach (var type in XmlData.Current.Body.SkinPatterns)
                {
                    yield return type;
                }
            }
        }

        public int Coverage
        {
            get { 
                if (_game.IsXianxia)
                    return GetInt("coverage", 0);
                return 0;
            }
            set {
                if (_game.IsXianxia)
                {
                    SetValue("coverage", value);
                    if (value > 0 && Coat.Type == 0)
                    {
                        Coat.Type = 1;
                    }
                    else if (value == 0)
                    {
                        Coat.Type = 0;
                    }
                    OnPropertyChanged("IsCoatEnabled");
                }
            }
        }
        public void PokeCoatEnabled()
        {
            OnPropertyChanged("IsCoatEnabled");

        }
        public bool IsCoatEnabled
        {
            get
            {
                if (_game.IsXianxia)
                {
                    return Coverage > 0;
                }
                else
                {
                    return Base.Type == 1;
                }
            }
        }
        public SkinLayerVM Base { get; private set; }
        public SkinLayerVM Coat { get; private set; }
    }

}
