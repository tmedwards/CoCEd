using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public class FaceVM : BodyPartVM
    {
        public override string Name { get { return "face"; } }
        public FaceVM(GameVM game, AmfObject obj) : base(game, obj) { }

        public override IEnumerable<XmlEnum> AllTypes
        {
            get
            {
                foreach (var type in XmlData.Current.Body.FaceTypes)
                {
                    yield return type;
                }
            }
        }
    }
}
