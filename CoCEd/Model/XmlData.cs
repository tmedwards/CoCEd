using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CoCEd.Model
{
    [XmlRoot("CoCEd")]
    public sealed class XmlData
    {
        [XmlElement("Body")]
        public XmlBodySet Body { get; set; }

        [XmlElement("Perks")]
        public XmlPerkSet Perks { get; set; }

        [XmlArray("Items"), XmlArrayItem("ItemGroup")]
        public XmlItemGroup[] ItemGroups { get; set; }

        [XmlArray, XmlArrayItem("Status")]
        public XmlEnumWithStringID[] Statuses { get; set; }

        [XmlArray, XmlArrayItem("Flag")]
        public XmlEnum[] Flags { get; set; }
       
        public static XmlData Instance
        {
            get;
            private set;
        }

        public static XmlLoadingResult LoadXml()
        {
            try
            {
                using (var stream = File.OpenRead("CoCEd.xml"))
                {
                    XmlSerializer s = new XmlSerializer(typeof(XmlData));
                    Instance = s.Deserialize(stream) as XmlData;
                    return XmlLoadingResult.Success;
                }
            }
            catch(UnauthorizedAccessException)
            {
                return XmlLoadingResult.NoPermission;
            }
            catch (SecurityException)
            {
                return XmlLoadingResult.NoPermission;
            }
            catch (FileNotFoundException)
            {
                return XmlLoadingResult.MissingFile;
            }
        }
    }

    public sealed class XmlBodySet
    {
        [XmlArray, XmlArrayItem("SkinType")]
        public XmlEnum[] SkinTypes { get; set; }
        [XmlArray, XmlArrayItem("SkinTone")]
        public XmlEnumWithStringID[] SkinTones { get; set; }
        [XmlArray, XmlArrayItem("HairType")]
        public XmlEnum[] HairTypes { get; set; }
        [XmlArray, XmlArrayItem("HairColor")]
        public XmlEnumWithStringID[] HairColors { get; set; }

        [XmlArray, XmlArrayItem("FaceType")]
        public XmlEnum[] FaceTypes { get; set; }
        [XmlArray, XmlArrayItem("TongueType")]
        public XmlEnum[] TongueTypes { get; set; }
        [XmlArray, XmlArrayItem("EyeType")]
        public XmlEnum[] EyeTypes { get; set; }
        [XmlArray, XmlArrayItem("EarType")]
        public XmlEnum[] EarTypes { get; set; }
        [XmlArray, XmlArrayItem("HornType")]
        public XmlEnum[] HornTypes { get; set; }
        [XmlArray, XmlArrayItem("AntennaeType")]
        public XmlEnum[] AntennaeTypes { get; set; }

        [XmlArray, XmlArrayItem("ArmType")]
        public XmlEnum[] ArmTypes { get; set; }
        [XmlArray, XmlArrayItem("TailType")]
        public XmlEnum[] TailTypes { get; set; }
        [XmlArray, XmlArrayItem("WingType")]
        public XmlEnum[] WingTypes { get; set; }
        [XmlArray, XmlArrayItem("LowerBodyType")]
        public XmlEnum[] LowerBodyTypes { get; set; }
        [XmlArray, XmlArrayItem("PiercingType")]
        public XmlEnum[] PiercingTypes { get; set; }

        [XmlArray, XmlArrayItem("CockType")]
        public XmlEnum[] CockTypes { get; set; }
        [XmlArray, XmlArrayItem("VaginaType")]
        public XmlEnum[] VaginaTypes { get; set; }
        [XmlArray, XmlArrayItem("VaginalWetnessLevel")]
        public XmlEnum[] VaginalWetnessLevels { get; set; }
        [XmlArray, XmlArrayItem("VaginalLoosenessLevel")]
        public XmlEnum[] VaginalLoosenessLevels { get; set; }
        [XmlArray, XmlArrayItem("AnalLoosenessLevel")]
        public XmlEnum[] AnalLoosenessLevels { get; set; }

        [XmlArray, XmlArrayItem("PregnancyType")]
        public XmlEnum[] PregnancyTypes { get; set; }
        [XmlArray, XmlArrayItem("AnalPregnancyType")]
        public XmlEnum[] AnalPregnancyTypes { get; set; }
        [XmlArray, XmlArrayItem("EggPregnancyType")]
        public XmlEnum[] EggPregnancyTypes { get; set; }
    }

    public sealed class XmlPerkSet
    {
        [XmlArray, XmlArrayItem("Perk")]
        public XmlPerk[] StarterPerks { get; set; }
        [XmlArray, XmlArrayItem("Perk")]
        public XmlPerk[] HistoryPerks { get; set; }
        [XmlArray, XmlArrayItem("Perk")]
        public XmlPerk[] Tier0Perks { get; set; }
        [XmlArray, XmlArrayItem("Perk")]
        public XmlPerk[] Tier1Perks { get; set; }
        [XmlArray, XmlArrayItem("Perk")]
        public XmlPerk[] Tier2Perks { get; set; }
        [XmlArray, XmlArrayItem("Perk")]
        public XmlPerk[] EventPerks { get; set; }
    }

    [Flags]
    public enum ItemCategories
    {
        Other = 1,
        Weapon = 2,
        Armor = 4,
        All = Other | Weapon | Armor,
    }

    public sealed class XmlItemGroup
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public ItemCategories Category { get; set; }

        [XmlElement("Item")]
        public XmlEnumWithStringID[] Items { get; set; }
    }

    public sealed class XmlEnum
    {
        [XmlAttribute]
        public int ID { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }

        public override string ToString()
        {
            return ID + " - " + Name;
        }
    }

    public sealed class XmlEnumWithStringID
    {
        [XmlAttribute]
        public string ID { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }

        public override string ToString()
        {
            return ID + " | " + Name;
        }
    }

    public sealed class XmlPerk
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlAttribute]
        public double Value1 { get; set; }
        [XmlAttribute]
        public double Value2 { get; set; }
        [XmlAttribute]
        public double Value3 { get; set; }
        [XmlAttribute]
        public double Value4 { get; set; }
    }

    public enum XmlLoadingResult
    {
        Success,
        NoPermission,
        MissingFile,
    }
}
