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
    public sealed class StatusVM : NamedVector4VM
    {
        readonly XmlName _data;
        readonly string _name;

        public StatusVM(AmfObject allStatuses, string name)
            : base(allStatuses)
        {
            _data = XmlData.Instance.Statuses.FirstOrDefault(x => x.Name == name);
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
            obj["statusAffectName"] = _name;
            obj["value1"] = 0;
            obj["value2"] = 0;
            obj["value3"] = 0;
            obj["value4"] = 0;
        }

        protected override bool IsObject(AmfObject obj)
        {
            return obj.GetString("statusAffectName") == _name;
        }

        protected override void NotifyGameVM()
        {
            VM.Instance.Game.OnStatusChanged(_name);
        }

        public bool Match(string str)
        {
            if (str == null || str.Length < 3) return true;

            int index = (Name ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            index = (Comment ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            return false;
        }
    }
}
