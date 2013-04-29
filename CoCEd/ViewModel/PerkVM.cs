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
}
