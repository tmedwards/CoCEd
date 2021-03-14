using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{

    public class ClawsVM : BodyPartVM
    {
        public override string Name { get { return "claw"; } }

        public ClawsVM(GameVM game, AmfObject obj) : base(game, obj) { }

        public override IEnumerable<XmlEnum> AllTypes
        {
            get
            {
                foreach (var type in XmlData.Current.Body.ClawTypes)
                {
                    yield return type;
                }
            }
        }

        public string Tone
        {
            get { return GenericGetStringProp("tone"); }
            set { GenericSetProp("tone", value); }
        }

        public IEnumerable<string> AllTones
        {
            get
            {
                foreach (var tone in XmlData.Current.Body.ClawTones)
                {
                    yield return tone;
                }
            }
        }
    }
}
