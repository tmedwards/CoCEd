using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
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
            get
            {
                if (_game.IsXianxia)
                {
                    return GetInt("legCount", 0);
                }
                // lowerBody
                return _game.GetInt("legCount", 0);
            }
            set
            {
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
            get
            {
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


}
