using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class ItemContainerVM
    {
        readonly ObservableCollection<ItemSlotVM> _slots = new ObservableCollection<ItemSlotVM>();
        readonly GameVM _game;

        public ItemContainerVM(GameVM game, string name, ItemCategories categories)
        {
            Name = name;
            _game = game;
            Categories = categories;
        }

        public string Name
        {
            get;
            private set;
        }

        public ItemCategories Categories
        {
            get;
            private set;
        }

        public ObservableCollection<ItemSlotVM> Slots
        {
            get { return _slots; }
        }

        public void Add(AmfObject obj)
        {
            _slots.Add(new ItemSlotVM(_game, obj, Categories));
        }

        public void Clear()
        {
            _slots.Clear();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class ItemSlotVM : ObjectVM
    {
        readonly ItemGroupVM[] _allGroups;
        readonly GameVM _game;

        public ItemSlotVM(GameVM game, AmfObject obj, ItemCategories categories)
            : base(obj)
        {
            Categories = categories;

            _game = game;
            _allGroups = XmlData.Current.ItemGroups.Where(group => Categories.HasFlag(group.Category)).Select(x => new ItemGroupVM(_game, x, this)).ToArray();
            AllGroups = new UpdatableCollection<ItemGroupVM>(_allGroups.Where(x => x.Items.Count != 0));
        }

        public void UpdateGroups()
        {
            foreach (var group in _allGroups) group.Items.Update();
            AllGroups.Update();
        }

        public ItemCategories Categories
        {
            get;
            private set;
        }

        public UpdatableCollection<ItemGroupVM> AllGroups
        {
            get;
            private set;
        }

        public int Quantity
        {
            get { return GetInt("quantity"); }
            set
            {
                SetValue("quantity", value);

                // Fix type
                if (value == 0) Type = "NOTHING!";

                // Property change
                OnPropertyChanged("TypeDescription");
                OnPropertyChanged("QuantityDescription");
            }
        }

        public string Type
        {
            get
            {
                var id = GetString("id");

                // Save format fixup, only needed when editing older saves
                if (id == null && _obj.Contains("shortName"))
                {
                    _obj["id"] = _obj["shortName"];
                    _obj["shortName"] = null;
                    id = GetString("id");
                }

                // Temporary Special Honey ID snafu workaround, until a game release fixes the issue
                if (id == "Sp Honey") id = "SpHoney";

                return id == "NOTHING!" ? "" : id;
            }
            set
            {
                var oldType = Type;
                if (value == "") value = "NOTHING!";

                // Temporary Special Honey ID snafu workaround, until a game release fixes the issue
                if (value == "SpHoney") value = "Sp Honey";

                if (!SetValue("id", value)) return;

                // Fix quantity

                // Temporary Special Honey ID snafu workaround, until a game release fixes the issue
                //var xml = XmlData.Current.ItemGroups.SelectMany(x => x.Items).FirstOrDefault(x => x.ID == value);
                var xml = XmlData.Current.ItemGroups.SelectMany(x => x.Items).FirstOrDefault(x => x.ID == Type); // FIXME: Would using `oldType` be better here?

                if (xml != null && Quantity == 0) Quantity = 1;
                else if (xml == null && Quantity != 0) Quantity = 0;

                // Property change
                OnPropertyChanged("QuantityDescription");
                OnPropertyChanged("TypeDescription");
                InvalidateItem(oldType);
                InvalidateItem(value);
            }
        }

        void InvalidateItem(string type)
        {
            var item = AllGroups.SelectMany(x => x.Items).FirstOrDefault(x => x.ID == type);
            if (item == null) return;
            item.NotifyIsSelectedChanged();
        }

        public string TypeDescription
        {
            get
            {
                var xml = XmlData.Current.ItemGroups.SelectMany(x => x.Items).FirstOrDefault(x => x.ID == Type);
                if (Quantity == 0 || xml == null) return "<empty>";
                return xml.Name;
            }
        }

        public string QuantityDescription
        {
            get
            {
                var type = XmlData.Current.ItemGroups.SelectMany(x => x.Items).FirstOrDefault(x => x.ID == Type);
                if (Quantity == 0 || type == null) return "";
                return "\u00D7" + Quantity.ToString();
            }
        }
    }


    public sealed class ItemGroupVM
    {
        readonly GameVM _game;

        public ItemGroupVM(GameVM game, XmlItemGroup group, ItemSlotVM slot)
        {
            _game = game;
            Name = group.Name;
            Items = new UpdatableCollection<ItemVM>(group.Items.Where(x => Match(x, _game.ItemSearchText)).OrderBy(x => x.Name).Select(x => new ItemVM(slot, x)));
        }

        public string Name
        {
            get;
            private set;
        }

        public UpdatableCollection<ItemVM> Items
        {
            get;
            private set;
        }

        static bool Match(XmlItem item, string searchText)
        {
            if (searchText == null || searchText.Length < 3) return true;

            int index = (item.Name ?? "").IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            index = (item.Description ?? "").IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            return false;
        }

        public override string ToString()
        {
            return Name;
        }

    }

    public sealed class ItemVM : BindableBase
    {
        readonly ItemSlotVM _slot;
        readonly XmlItem _xml;

        public ItemVM(ItemSlotVM slot, XmlItem item)
        {
            _slot = slot;
            _xml = item;
        }

        public string ID
        {
            get { return _xml.ID; }
        }

        public string Name
        {
            get
            {
                if (ToolTip == null) return _xml.Name;
                else return _xml.Name + "\u202F*";
            }
        }

        public string ToolTip
        {
            get { return _xml.Description; }
        }

        public bool IsSelected
        {
            get { return _slot.Type == _xml.ID; }
            set
            {
                if (!value) return;
                _slot.Type = _xml.ID;
            }
        }

        public void NotifyIsSelectedChanged()
        {
            OnPropertyChanged("IsSelected");
        }


        public override string ToString()
        {
            return Name;
        }
    }
}
