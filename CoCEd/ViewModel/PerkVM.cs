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
        readonly GameVM _game;

        public PerkGroupVM(GameVM game, string name, PerkVM[] perks)
        {
            _game = game;
            Name = name;
            Perks = new UpdatableCollection<PerkVM>(perks.Where(x => x.Match(_game.PerkSearchText)));
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
        public PerkVM(GameVM game, AmfObject perksArray, XmlNamedVector4 xml)
            : base(game, perksArray, xml)
        {
        }

        protected override void InitializeObject(AmfObject obj)
        {
#if !PRE_SAVE_REFACTOR
            obj["id"] = _xml.Name;
#else
            obj["perkName"] = _xml.Name;
            obj["perkDesc"] = _xml.Description;
#endif
        }

        protected override bool IsObject(AmfObject obj)
        {
#if !PRE_SAVE_REFACTOR
            return obj.GetString("id") == _xml.Name;
#else
            return obj.GetString("perkName") == _xml.Name;
#endif
        }

        protected override void NotifyGameVM()
        {
            _game.OnPerkChanged(_xml.Name);
        }

        protected override void OnIsOwnedChanged()
        {
            _game.OnPerkAddedOrRemoved(_xml.Name, IsOwned);
        }
    }
}
