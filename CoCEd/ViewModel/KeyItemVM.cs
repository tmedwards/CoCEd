using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class KeyItemVM : NamedVector4VM
    {
        readonly XmlName _data;
        readonly string _name;

        public KeyItemVM(AmfObject keyItems, string name)
            : base(keyItems)
        {
            _data = XmlData.Instance.KeyItems.FirstOrDefault(x => x.Name == name);
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Comment
        {
            get { return _data == null ? "" : _data.Description; }
        }

        public Visibility CommentVisibility
        {
            get { return _data != null ? Visibility.Visible : Visibility.Collapsed; }
        }

        protected override void InitializeObject(AmfObject obj)
        {
            obj["keyName"] = _name;
            obj["value1"] = 0;
            obj["value2"] = 0;
            obj["value3"] = 0;
            obj["value4"] = 0;
        }

        protected override bool IsObject(AmfObject obj)
        {
            return obj.GetString("keyName") == _name;
        }

        protected override void NotifyGameVM()
        {
        }
    }
}
