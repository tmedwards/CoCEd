using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class PerkGroupVM
    {
        public PerkGroupVM(string name, AmfObject character, IEnumerable<XmlNamedVector4> perks)
        {
            Name = name;
            Perks = perks.OrderBy(x => x.Name).Select(x => new PerkVM(character.GetObj("perks"), x)).ToArray();
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

    public sealed class PerkVM : NamedVector4VM
    {
        public PerkVM(AmfObject perksArray, XmlNamedVector4 xml)
            : base(perksArray, xml)
        {
        }

        public override string Comment
        {
            get { return String.IsNullOrEmpty(_xml.Description) ? "<no description>" : _xml.Description; }
        }

        protected override void InitializeObject(AmfObject obj)
        {
            obj["perkName"] = _xml.Name;
            obj["perkDesc"] = _xml.Description;
            obj["value1"] = _xml.Value1;
            obj["value2"] = _xml.Value2;
            obj["value3"] = _xml.Value3;
            obj["value4"] = _xml.Value4;
        }

        protected override bool IsObject(AmfObject obj)
        {
            return obj.GetString("perkName") == _xml.Name;
        }

        protected override void NotifyGameVM()
        {
        }
    }
}
