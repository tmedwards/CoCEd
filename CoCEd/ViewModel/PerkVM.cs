using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class PerkGroupVM
    {
        public PerkGroupVM(string name, AmfObject character, IEnumerable<XmlPerk> perks)
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

    public sealed class PerkVM : BindableBase
    {
        readonly AmfObject _perksArray;
        readonly XmlPerk _xml;

        public PerkVM(AmfObject perksArray, XmlPerk xml)
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
                    AmfObject perk = new AmfObject(AmfTypes.Array);
                    perk["perkDesc"] = _xml.Description;
                    perk["perkName"] = _xml.Name;
                    perk["value1"] = _xml.Value1;
                    perk["value2"] = _xml.Value2;
                    perk["value3"] = _xml.Value3;
                    perk["value4"] = _xml.Value4;
                    _perksArray.Push(perk);
                }
                else
                {
                    _perksArray.Pop((int)pair.Key);
                }
                OnPropertyChanged("IsOwned");
                VM.Instance.NotifySaveRequiredChanged();
            }
        }

        public AmfPair Pair
        {
            get { return _perksArray.FirstOrDefault(x => String.Equals(x.ValueAsObject.GetString("perkName"), _xml.Name, StringComparison.InvariantCultureIgnoreCase)); }
        }
    }
}
