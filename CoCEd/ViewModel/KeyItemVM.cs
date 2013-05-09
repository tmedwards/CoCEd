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
        public KeyItemVM(AmfObject keyItems, string name)
            : base(keyItems, XmlData.Instance.KeyItems.FirstOrDefault(x => x.Name == name), name)
        {
        }

        protected override void InitializeObject(AmfObject obj)
        {
            obj["keyName"] = _name;
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
