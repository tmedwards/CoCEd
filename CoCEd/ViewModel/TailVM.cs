using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
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
            get
            {
                if (_game.IsXianxia)
                    return GetInt("count", 0);
                if (IsTailCountEnabled)
                    return _game.GetInt("tailVenum", 0);
                return 0;
            }
            set
            {
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

}
