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

        public ItemContainerVM(string name, ItemCategories categories)
        {
            Name = name;
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
            _slots.Add(new ItemSlotVM(obj, Categories));
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
        ItemGroupVM[] _allGroups;

        public ItemSlotVM(AmfObject obj, ItemCategories categories)
            : base(obj)
        {
            Categories = categories;
            _allGroups = XmlData.Instance.ItemGroups.Where(group => Categories.HasFlag(group.Category) && group.Items.Count > 0).Select(x => new ItemGroupVM(x, this)).ToArray();
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
                OnPropertyChanged("TypeDescription");
                OnPropertyChanged("QuantityDescription");
            }
        }

        public string Type
        {
            get { return GetString("shortName"); }
            set
            {
                var oldType = Type;
                if (!SetValue("shortName", value)) return;
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
                var type = XmlData.Instance.ItemGroups.SelectMany(x => x.Items).FirstOrDefault(x => x.ID == Type);
                if (Quantity == 0 || type == null) return "<empty>";
                return type.Name;
            }
        }

        public string QuantityDescription
        {
            get
            {
                var type = XmlData.Instance.ItemGroups.SelectMany(x => x.Items).FirstOrDefault(x => x.ID == Type);
                if (Quantity == 0 || type == null) return "";
                return "x" + Quantity.ToString();
            }
        }
    }


    public sealed class ItemGroupVM
    {
        public ItemGroupVM(XmlItemGroup group, ItemSlotVM slot)
        {
            Name = group.Name;
            Items = new UpdatableCollection<ItemVM>(group.Items.Where(x => Match(x)).OrderBy(x => x.Name).Select(x => new ItemVM(slot, x)));
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

        static bool Match(XmlItem item)
        {
            if (VM.Instance == null || VM.Instance.Game == null) return true;

            var str = VM.Instance.Game.ItemSearchText;
            if (str == null || str.Length < 3) return true;

            int index = (item.Name ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            index = (item.Description ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
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
            get { return _xml.Name; }
        }

        public bool IsSelected
        {
            get { return _slot.Type == _xml.ID; }
            set
            {
                if (!value) return;
                _slot.Type = _xml.ID;
                if (_slot.Quantity == 0) _slot.Quantity = 1;
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
