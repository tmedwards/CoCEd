using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        [XmlArray("Perks"), XmlArrayItem("PerkGroup")]
        public List<XmlPerkGroup> PerkGroups { get; set; }

        [XmlArray("Items"), XmlArrayItem("ItemGroup")]
        public List<XmlItemGroup> ItemGroups { get; set; }

        [XmlArray, XmlArrayItem("Status")]
        public List<XmlNamedVector4> Statuses { get; set; }

        [XmlArray, XmlArrayItem("KeyItem")]
        public List<XmlNamedVector4> KeyItems { get; set; }

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
                var path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                path = Path.Combine(path, "CoCEd.xml");

                using (var stream = File.OpenRead(path))
                {
                    XmlSerializer s = new XmlSerializer(typeof(XmlData));
                    Instance = s.Deserialize(stream) as XmlData;

                    var unknwonPerks = new XmlPerkGroup { Name = "Unknown", Perks = new List<XmlNamedVector4>() };
                    Instance.PerkGroups.Add(unknwonPerks);

                    var unknwonItems = new XmlItemGroup { Name = "Unknown", Items = new List<XmlItem>(), Category = ItemCategories.Unknown };
                    Instance.ItemGroups.Add(unknwonItems);

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
        [XmlArray, XmlArrayItem("HairType")]
        public XmlEnum[] HairTypes { get; set; }
        [XmlArray, XmlArrayItem("HairColor")]
        public String[] HairColors { get; set; }
        [XmlArray, XmlArrayItem("SkinTone")]
        public String[] SkinTones { get; set; }
        [XmlArray, XmlArrayItem("SkinAdjective")]
        public String[] SkinAdjectives { get; set; }
        [XmlArray, XmlArrayItem("SkinDescription")]
        public String[] SkinDescriptions { get; set; }


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
        [XmlArray, XmlArrayItem("WingDescription")]
        public String[] WingDescriptions { get; set; }
        [XmlArray, XmlArrayItem("LowerBodyType")]
        public XmlEnum[] LowerBodyTypes { get; set; }
        [XmlArray, XmlArrayItem("PiercingType")]
        public XmlEnum[] PiercingTypes { get; set; }
        [XmlArray, XmlArrayItem("PiercingMaterial")]
        public XmlEnum[] PiercingMaterials { get; set; }

        [XmlArray, XmlArrayItem("CockType")]
        public XmlEnum[] CockTypes { get; set; }
        [XmlArray, XmlArrayItem("CockSockType")]
        public String[] CockSockTypes { get; set; }
        [XmlArray, XmlArrayItem("VaginaType")]
        public XmlEnum[] VaginaTypes { get; set; }
        [XmlArray, XmlArrayItem("VaginalWetnessLevel")]
        public XmlEnum[] VaginalWetnessLevels { get; set; }
        [XmlArray, XmlArrayItem("VaginalLoosenessLevel")]
        public XmlEnum[] VaginalLoosenessLevels { get; set; }
        [XmlArray, XmlArrayItem("AnalLoosenessLevel")]
        public XmlEnum[] AnalLoosenessLevels { get; set; }
        [XmlArray, XmlArrayItem("AnalWetnessLevel")]
        public XmlEnum[] AnalWetnessLevels { get; set; }

        [XmlArray, XmlArrayItem("PregnancyType")]
        public XmlEnum[] PregnancyTypes { get; set; }
        [XmlArray, XmlArrayItem("AnalPregnancyType")]
        public XmlEnum[] AnalPregnancyTypes { get; set; }
        [XmlArray, XmlArrayItem("EggPregnancyType")]
        public XmlEnum[] EggPregnancyTypes { get; set; }
    }

    [Flags]
    public enum ItemCategories
    {
        Other = 1,
        Weapon = 2,
        Armor = 4,
        Unknown = 8,
        All = Other | Weapon | Armor | Unknown,
    }

    public sealed class XmlItemGroup
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public ItemCategories Category { get; set; }

        [XmlElement("Item")]
        public List<XmlItem> Items { get; set; }
    }

    public sealed class XmlPerkGroup
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement("Perk")]
        public List<XmlNamedVector4> Perks { get; set; }
    }

    public sealed class XmlEnum
    {
        [XmlAttribute]
        public int ID { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlIgnore]
        public bool IsGrayedOut { get; set; }

        public override string ToString()
        {
            return ID + " - " + Name;
        }
    }

    public sealed class XmlItem
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

    public sealed class XmlName
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
    }

    public sealed class XmlNamedVector4
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

        [XmlAttribute]
        public string Label1 { get; set; }
        [XmlAttribute]
        public string Label2 { get; set; }
        [XmlAttribute]
        public string Label3 { get; set; }
        [XmlAttribute]
        public string Label4 { get; set; }
    }

    public enum XmlLoadingResult
    {
        Success,
        NoPermission,
        MissingFile,
    }
}
