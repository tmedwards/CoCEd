using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class ItemContainerVM
    {
        readonly List<ItemSlotVM> _slots = new List<ItemSlotVM>();

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

        public IEnumerable<ItemSlotVM> Slots
        {
            get { return _slots; }
        }

        public void Add(AmfObject obj)
        {
            _slots.Add(new ItemSlotVM(obj, Categories));
        }
    }

    public sealed class ItemSlotVM : ObjectVM
    {
        public ItemSlotVM(AmfObject obj, ItemCategories categories)
            : base(obj)
        {
            Categories = categories;

            // Add missing items
            var type = Type;
            if (!String.IsNullOrEmpty(type) && XmlData.Instance.ItemGroups.SelectMany(x => x.Items).All(x => x.ID != type))
            {
                var xml = new XmlItem { ID = type, Name = type };
                XmlData.Instance.ItemGroups.Last().Items.Add(xml);
            }
        }

        public ItemCategories Categories
        {
            get;
            private set;
        }

        public IEnumerable<ItemGroupVM> AllGroups
        {
            get
            {
                foreach (var group in XmlData.Instance.ItemGroups)
                {
                    if (Categories.HasFlag(group.Category) && group.Items.Count != 0) yield return new ItemGroupVM(group, this);
                }
            }
        }

        public int Quantity
        {
            get { return GetInt("quantity"); }
            set
            {
                if (!SetValue("quantity", value)) return;
                OnPropertyChanged("TypeDescription");
                OnPropertyChanged("QuantityDescription");
            }
        }

        public string Type
        {
            get { return GetString("shortName").Trim(); }
            set
            {
                if (!SetValue("shortName", value)) return;
                OnPropertyChanged("TypeDescription");
                OnPropertyChanged("QuantityDescription");
            }
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
        const int Columns = 3;
        readonly ItemVM[] _items;

        public ItemGroupVM(XmlItemGroup group, ItemSlotVM slot)
        {
            Name = group.Name;
            _items = group.Items.OrderBy(x => x.Name).Select(x => new ItemVM(slot, x)).ToArray();
        }

        public string Name
        {
            get;
            private set;
        }

        public IEnumerable<ItemVM> Items
        {
            get { return _items; }
        }
    }

    public sealed class ItemVM : BindableBase
    {
        readonly ItemSlotVM _slot;
        readonly XmlItem _item;

        public ItemVM(ItemSlotVM slot, XmlItem item)
        {
            _slot = slot;
            _item = item;
        }

        public string Name
        {
            get { return _item.Name; }
        }

        public bool IsSelected
        {
            get { return _slot.Type == _item.ID; }
            set
            {
                if (value)
                {
                    _slot.Type = _item.ID;
                    if (_slot.Quantity == 0) _slot.Quantity = 1;
                }
                else if (_slot.Type == _item.ID)
                {
                    _slot.Type = "";
                }
                OnPropertyChanged();
                VM.Instance.NotifySaveRequiredChanged();
            }
        }
    }
}
