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

}
