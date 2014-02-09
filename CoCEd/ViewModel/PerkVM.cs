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
            obj["id"] = _xml.Name;
        }

        protected override bool IsObject(AmfObject obj)
        {
            var id = obj.GetString("id");

            // Save format fixup, only needed when editing older saves
            if (id == null && obj.Contains("perkName"))
            {
                obj["id"] = obj["perkName"];
                obj["perkName"] = null;
                obj["perkDesc"] = null;
                id = obj.GetString("id");
            }

            // Fixes saves which have NaNs for some perk values, which crashes CoC
            if (double.IsNaN(obj.GetDouble("value1"))) obj["value1"] = 0;
            if (double.IsNaN(obj.GetDouble("value2"))) obj["value2"] = 0;
            if (double.IsNaN(obj.GetDouble("value3"))) obj["value3"] = 0;
            if (double.IsNaN(obj.GetDouble("value4"))) obj["value4"] = 0;

            return id == _xml.Name;
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
