using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public class VaginaVM : ObjectVM
    {
        public VaginaVM(AmfObject node)
            : base(node)
        {
            ClitPiercing = new PiercingVM(node, "clit");
            LabiaPiercing = new PiercingVM(node, "labia");
        }

        public PiercingVM ClitPiercing { get; private set; }
        public PiercingVM LabiaPiercing { get; private set; }

        public XmlEnum[] AllTypes
        {
            get { return XmlData.Instance.Body.VaginaTypes; }
        }

        public XmlEnum[] AllLoosenessLevels
        {
            get { return XmlData.Instance.Body.VaginalLoosenessLevels; }
        }

        public XmlEnum[] AllWetnessLevels
        {
            get { return XmlData.Instance.Body.VaginalWetnessLevels; }
        }

        public int Type
        {
            get { return GetInt("type", 0); }
            set { SetValue("type", value); }
        }

        public int Looseness
        {
            get { return GetInt("vaginalLooseness"); }
            set { SetValue("vaginalLooseness", value); }
        }

        public int Wetness
        {
            get { return GetInt("vaginalWetness"); }
            set { SetValue("vaginalWetness", value); }
        }

        public bool Virgin
        {
            get { return GetBool("virgin"); }
            set { SetValue("virgin", value); }
        }

        public string Description
        {
            get { return "One Vagina"; }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            base.OnPropertyChanged("Description");
        }
    }

    public sealed class VaginaArrayVM : ArrayVM<VaginaVM>
    {
        public VaginaArrayVM(AmfObject obj)
            : base(obj, x => new VaginaVM(x))
        {
        }

        protected override AmfObject CreateNewObject()
        {
            var obj = new AmfObject(AmfTypes.Array);

            obj["clipPLong"] = "";
            obj["clitPShort"] = "";
            obj["clitPierced"] = false;

            obj["labiaPLong"] = "";
            obj["labiaPShort"] = "";
            obj["labiaPierced"] = false;

            obj["type"] = 0;
            obj["virgin"] = true;
            obj["vaginalWetness"] = 1;
            obj["vaginalLooseness"] = 0;
            return obj;
        }
    }
}
