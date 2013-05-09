using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class PerkGroupVM : BindableBase
    {
        public PerkGroupVM(string name, AmfObject perksArray, IEnumerable<XmlNamedVector4> perks)
        {
            Name = name;
            var perksVM = perks.OrderBy(x => x.Name).Select(x => new PerkVM(perksArray, x)).ToArray();
            Perks = new UpdatableCollection<PerkVM>(perksVM.Where(x => x.Match(VM.Instance.Game != null ? VM.Instance.Game.PerkSearchText : null)));
        }

        public string Name
        {
            get;
            private set;
        }

        public UpdatableCollection<PerkVM> Perks
        {
            get;
            private set;
        }

        public Visibility Visibility
        {
            get { return Perks.Count != 0 ? Visibility.Visible : Visibility.Collapsed; }
        }

        public void Update()
        {
            Perks.Update();
            OnPropertyChanged("Visibility");
        }
    }

    public sealed class PerkVM : NamedVector4VM
    {
        public PerkVM(AmfObject perksArray, XmlNamedVector4 xml)
            : base(perksArray, xml)
        {
        }

        protected override void InitializeObject(AmfObject obj)
        {
            obj["perkName"] = _xml.Name;
            obj["perkDesc"] = _xml.Description;
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
