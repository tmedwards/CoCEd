using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class VaginaArrayVM : ArrayVM<VaginaVM>
    {
        public VaginaArrayVM(AmfObject obj)
            : base(obj, x => new VaginaVM(x))
        {
        }

        protected override AmfObject CreateNewObject()
        {
            var obj = new AmfObject(AmfTypes.Array)
            {
                ["clitLength"] = 0.5,
                ["clipPLong"] = "",
                ["clitPShort"] = "",
                ["clitPierced"] = false,
                ["labiaPLong"] = "",
                ["labiaPShort"] = "",
                ["labiaPierced"] = false,
                ["type"] = 0,
                ["virgin"] = true,
                ["vaginalWetness"] = 2,
                ["vaginalLooseness"] = 0,
            };
            return obj;
        }
    }

    public class VaginaVM : ObjectVM
    {
        public VaginaVM(AmfObject node)
            : base(node)
        {
            ClitPiercing = new PiercingVM(node, "clit", PiercingLocation.Clitoris);
            LabiaPiercing = new PiercingVM(node, "labia", PiercingLocation.Labia);
        }

        public PiercingVM ClitPiercing { get; private set; }
        public PiercingVM LabiaPiercing { get; private set; }

        public XmlEnum[] AllTypes
        {
            get { return XmlData.Current.Body.VaginaTypes; }
        }

        public XmlEnum[] AllLoosenessLevels
        {
            get { return XmlData.Current.Body.VaginalLoosenessLevels; }
        }

        public XmlEnum[] AllWetnessLevels
        {
            get { return XmlData.Current.Body.VaginalWetnessLevels; }
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

        public double ClitLength
        {
            get { return GetDouble("clitLength"); }
            set { SetValue("clitLength", value); }
        }

        public string Description
        {
            get { return "One Vagina"; }
        }
    }
}
