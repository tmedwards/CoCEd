using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{

    public abstract class BodyPartVM : ObjectVM
    {
        protected readonly GameVM _game;
        public abstract string Name { get; }
        public BodyPartVM(GameVM game, AmfObject obj) : base(obj) => _game = game;


        public AmfObject GetSelf()
        {
            return _obj;
        }

        public abstract IEnumerable<XmlEnum> AllTypes
        {
            get;
        }
        protected int GenericGetIntProp(string prop)
        {
            if (prop == "")
            {
                return -1;
            }
            if (_game.IsXianxia)
            {

                return GetInt(prop, 0);
            }
            string camelCasedProp = prop.Substring(0, 1).ToUpper() + prop.Substring(1);
            return _game.GetInt(Name + camelCasedProp, 0);
        }
        protected string GenericGetStringProp(string prop)
        {
            if (prop == "")
            {
                return "";
            }
            if (_game.IsXianxia)
            {

                return GetString(prop);
            }
            string camelCasedProp = prop.Substring(0, 1).ToUpper() + prop.Substring(1);
            return _game.GetString(Name + camelCasedProp);
        }

        protected void GenericSetProp(string prop, object value)
        {
            if (_game.IsXianxia)
            {
                SetValue(prop, value);
            }
            string camelCasedProp = prop.Substring(0, 1).ToUpper() + prop.Substring(1);
            _game.SetValue(Name + camelCasedProp, value);
        }

        virtual public int Type
        {
            get {
                return GenericGetIntProp("type");
            }
            set {
                GenericSetProp("type", value);
            }
        }
    }
    public class FaceVM : BodyPartVM
    {
        public override string Name { get { return "face"; } }
        public FaceVM(GameVM game, AmfObject obj) : base(game, obj) { }

        public override IEnumerable<XmlEnum> AllTypes
        {
            get
            {
                foreach (var type in XmlData.Current.Body.FaceTypes)
                {
                    yield return type;
                }
            }
        }
    }
    public class LowerBodyVM : BodyPartVM
    {
        public override string Name { get { return "lowerBody"; } }
        public LowerBodyVM(GameVM game, AmfObject obj) : base(game, obj) { }

        public override IEnumerable<XmlEnum> AllTypes
        {
            get
            {
                foreach (var type in XmlData.Current.Body.LowerBodyTypes)
                {
                    yield return type;
                }
            }
        }
        public int LegConfigs
        {
            get
            {
                if (!HasLegConfigs) return -1;
                return (LegCount / 2) - 1;
            }
            set
            {
                if (_game.IsRevampOrXianxia)
                {
                    if (value == LegConfigs) return;
                    LegCount = (value + 1) * 2;
                }
            }
        }

        //named legCount in both xianxia and base
        public int LegCount
        {
            get {
                if (_game.IsXianxia)
                {
                    return GetInt("legCount", 0);
                }
                // lowerBody
                return _game.GetInt("legCount", 0);
            }
            set {
                if (_game.IsXianxia)
                {
                    SetValue("legCount", value);
                }
                else
                {
                    _game.SetValue("legCount", value);
                }
            }
        }
        public override int Type
        {
            get {
                if (_game.IsXianxia)
                {
                    return GetInt("type", 0);
                }
                // lowerBody
                return _game.GetInt("lowerBody", 0);
            }
            set
            {
                int type = value;
                int count = LegCount; 
                // Set the default `LegCount` value when the lower body type is changed.
                switch (value)
                {
                    case 3: // Naga
                    case 8: // Goo
                    case 51: // Hydra
                        count = 1;
                        break;

                    case 1: //Hooves
                    case 21: //Cloven Hooves
                        if (count != 4) count = 2;
                        break;

                    case 11: // Pony
                        count = 4;
                        break;

                    case 4: // Centaur, deprecated
                        count = 4;
                        type = 1; // change to Hoofed
                        break;

                    case 24: // Deertaur, deprecated
                        count = 4;
                        type = 21; // change to Cloven Hoofed
                        break;

                    case 16: // Drider
                        count = 8;
                        break;

                    default:
                        count = 2;
                        break;
                }
                LegCount = count;
                if (_game.IsXianxia)
                {
                    SetValue("type", type);
                }
                else
                {
                    _game.SetValue("lowerBody", value);
                }

                OnPropertyChanged("LegCount");
                OnPropertyChanged("LegConfigs");
                OnPropertyChanged("HasLegConfigs");
            }
        }
        public bool HasLegConfigs
        {
            get
            {
                if (_game.IsVanilla)
                    return false;
                if (_game.IsXianxia)
                {
                    switch (Type)
                    {
                        // Types which definitely have only a single allowed leg configuration.
                        case 1: // Hoofed
                        case 21: // Cloven Hoofed
                            return true;
                        default:
                            return false;
                    }
                }
                switch (Type)
                {
                    // Types which definitely have only a single allowed leg configuration.
                    case 0: // Human (biped)
                    case 3: // Naga (uniped)
                    case 8: // Goo (uniped)
                    case 11: // Pony (quadruped)
                    case 16: // Drider (octoped) → Biped form is lower body type #15 (Chitinous spider legs).
                        return false;

                    // Types which probably should have only a single allowed leg configuration.
                    case 5: // Demonic high-heels (biped)    → Funny human legs, should have the same limitations as lower body type #0 (Human).
                    case 6: // Demonic claws (biped)         → (same as the above)
                    case 7: // Bee (biped)                   → (same as the above)
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
    }

    public class TailVM : BodyPartVM
    {
        public TailVM(GameVM game, AmfObject obj) : base(game, obj) { }
        public override string Name { get { return "tail"; } }

        public override IEnumerable<XmlEnum> AllTypes
        {
            get
            {
                foreach (var type in XmlData.Current.Body.TailTypes)
                {
                    yield return type;
                }
            }
        }
        public bool IsTailVenomEnabled
        {
            get { return (Type == 5 || Type == 6); }
        }
        public bool IsTailCountEnabled
        {
            get { return (Type == 13); }
        }


        public bool IsTailRechargeEnabled
        {
            get { return (Type == 5 || Type == 6); }
        }

        public int Count
        {
            get { 
                if (_game.IsXianxia)
                    return GetInt("count", 0); 
                if (IsTailCountEnabled)
                    return _game.GetInt("tailVenum", 0);
                return 0;
            }
            set {
                if (_game.IsXianxia)
                    SetValue("count", value);
                if (IsTailCountEnabled)
                    _game.SetValue("tailVenum", value);
            }
        }
        public int Venom
        {
            get
            {
                if (_game.IsXianxia)
                    return GetInt("venom", 0);
                if (IsTailVenomEnabled)
                    return _game.GetInt("tailVenum", 0);
                return 0;
            }
            set
            {
                if (_game.IsXianxia)
                    SetValue("venom", value);
                if (IsTailVenomEnabled)
                    _game.SetValue("tailVenum", value);
            }
        }
        public int Recharge
        {
            get { return GenericGetIntProp("recharge"); }
            set { GenericSetProp("recharge", value); }
        }
        public override int Type
        {
            get { return GetInt("type", 0); }
            set
            {
                int type = value;
                int count = GetInt("count", 0);
                if (count < 1 && type != 13) // If not Fox (which can have multiple tails), set count to 1;
                {
                    count = 1;
                }
                SetValue("count", count);
                SetValue("type", type);
            }
        }

    }

    public class ClawsVM: BodyPartVM
    {
        public override string Name { get { return "claw"; } }

        public ClawsVM(GameVM game, AmfObject obj) : base(game, obj) { }

        public override IEnumerable<XmlEnum> AllTypes
        {
            get
            {
                foreach (var type in XmlData.Current.Body.ClawTypes)
                {
                    yield return type;
                }
            }
        }

        public string Tone
        {
            get { return GenericGetStringProp("tone"); }
            set { GenericSetProp("tone", value); }
        }

        public IEnumerable<string> AllTones
        {
            get
            {
                foreach (var tone in XmlData.Current.Body.ClawTones)
                {
                    yield return tone;
                }
            }
        }

    }
}
