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
        public StatusVM(AmfObject allStatuses, string name)
            : base(allStatuses, XmlData.Instance.Statuses.FirstOrDefault(x => x.Name == name), name)
        {
        }

        protected override void InitializeObject(AmfObject obj)
        {
            obj["statusAffectName"] = _name;
        }

        protected override bool IsObject(AmfObject obj)
        {
            return obj.GetString("statusAffectName") == _name;
        }

        protected override void NotifyGameVM()
        {
            VM.Instance.Game.OnStatusChanged(_name);
        }
    }
}
