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
    public sealed class XmlData
    {
        // Kind of hacky I suppose, but for something this simple it beats creating a discriminated union
        // or juggling a filename list/enum pair
        public static class Files
        {
            public const string Vanilla = "CoCEd.Data.xml";
            public const string Revamp = "CoCEd.DataRevamp.xml";
            public const string Xianxia = "CoCEd.DataXianxia.xml";
            public static readonly IEnumerable<string> All = new string[]
            {
                Vanilla,
                Revamp,
                Xianxia,
            };
        }
        private static readonly XmlPerkGroup UnknownPerks = new XmlPerkGroup { Name = "Unknown", Perks = new List<XmlNamedVector4>() };
        private static readonly XmlItemGroup UnknownItems = new XmlItemGroup { Name = "Unknown", Items = new List<XmlItem>(), Category = ItemCategories.Unknown };

        private static Dictionary<string, XmlDataSet> _files = new Dictionary<string, XmlDataSet>();

        private static string _selectedFile { get; set; }

        public static void Select(string xmlFile) { _selectedFile = xmlFile; }

        public static XmlDataSet Current { get { return _files[_selectedFile]; } }

        public static XmlDataSet _GetFileData(string xmlFile) { 
            if (!Files.All.Contains(xmlFile))
            {
                return null;
            }
            return _files[xmlFile];
        }
        public static XmlLoadingResult _SaveXml(string path, XmlDataSet fileData)
        {
            try
            {
                using (var stream = File.OpenWrite(path))
                {
                    XmlSerializer s = new XmlSerializer(typeof(XmlDataSet));
                    s.Serialize(stream, fileData);
                    stream.Close();
                }
            }
            catch (UnauthorizedAccessException)
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

            return XmlLoadingResult.Success;

        }

        public static XmlLoadingResult SaveXml(string xmlFile)
        {
            if (!Files.All.Contains(xmlFile))
            {
                return XmlLoadingResult.InvalidFile;
            }
            var path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            path = Path.Combine(path, xmlFile);
            var fileData = _GetFileData(xmlFile);
            if (fileData == null)
            {
                return XmlLoadingResult.MissingFile;
            }
            bool AddUnknownPerks = !fileData.PerkGroups.Remove(UnknownPerks);
            bool AddUnknownItems = !fileData.ItemGroups.Remove(UnknownItems);
            var result = _SaveXml(xmlFile, fileData);
            if (AddUnknownPerks) 
                fileData.PerkGroups.Add(UnknownPerks);
            if (AddUnknownItems)
                fileData.ItemGroups.Add(UnknownItems);
            return result;
        }

        public static XmlDataSet _LoadXmlData(string path)
        {
            XmlDataSet fileData;
            using (var stream = File.OpenRead(path))
            {
                XmlSerializer s = new XmlSerializer(typeof(XmlDataSet));
                fileData = s.Deserialize(stream) as XmlDataSet;
                stream.Close();
            }
            return fileData;
        }
        public static bool IsValidDataSet(string xmlFile, XmlDataSet fileData)
        {
            if (fileData == null)
            {
                return false;
            }
            switch (xmlFile)
            {
                case XmlData.Files.Vanilla:
                    if (!fileData.Flags.Any(x => x.ID == 1279 && x.Name == "GAME_END")) return false;
                    break;

                case XmlData.Files.Revamp:
                    if (!fileData.Flags.Any(x => x.ID == 1279 && x.Name == "GAME_END")) return false;
                    if (fileData.Body.LowerBodyTypes.Any(x => x.ID == 24 && x.Name == "Deertaur")) return false;
                    if (!fileData.Body.LowerBodyTypes.Any(x => x.ID == 25 && x.Name == "Salamander")) return false;
                    if (!fileData.PerkGroups.Any(x => x.Name == "Tier 1" && x.Perks.Any(p => p.Name == "Iron Fists 3"))) return false;
                    if (!fileData.PerkGroups.Any(x => x.Name == "Events" && x.Perks.Any(p => p.Name == "Lustserker"))) return false;
                    break;

                case XmlData.Files.Xianxia:
                    if (!fileData.Flags.Any(x => x.ID == 1279 && x.Name == "GAME_END")) return false;
                    if (!fileData.Flags.Any(x => x.ID == 2147 && x.Name == "PRISON_DOOR_UNLOCKED")) return false;

                    //FIXME: Add additional Xianxia tests, if necessary.
                    break;
                // Not an actual file?
                default:
                    return false;
            }
            return true;
        }
        public static XmlLoadingResult SetXmlFileData(string xmlFile, XmlDataSet fileData, bool overwrite = true)
        {
            // If it's invalid, don't set it.
            if (!IsValidDataSet(xmlFile, fileData))
            {
                return XmlLoadingResult.InvalidFile;
            }

            // add Unknown groups
            if (!fileData.PerkGroups.Contains(UnknownPerks))
            {
                fileData.PerkGroups.Add(UnknownPerks);
            } 
            if (!fileData.ItemGroups.Contains(UnknownItems))
            {
                fileData.ItemGroups.Add(UnknownItems);
            }
            
            if (!_files.ContainsKey(xmlFile))
            {
                _files.Add(xmlFile, fileData);
            }
            else if (overwrite)
            {
                _files[xmlFile] = fileData;
            }
            else
            {
                return XmlLoadingResult.AlreadyLoaded;
            }
            if (_files.Count == 1) Select(xmlFile);
            return XmlLoadingResult.Success;
        }

        public static XmlLoadingResult SetAndSaveXmlData(string xmlFile, XmlDataSet fileData)
        {
            var result = SetXmlFileData(xmlFile, fileData);
            if (result != XmlLoadingResult.Success)
            {
                return result;
            }
            return SaveXml(xmlFile);
        }

        public static XmlLoadingResult LoadXml(string xmlFile)
        {
            try
            {
                var path = xmlFile;
                xmlFile = Path.GetFileName(xmlFile);
                if (!Path.IsPathRooted(path))
                {
                    path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                    path = Path.Combine(path, xmlFile);
                }

                var fileData = _LoadXmlData(path);
                return SetXmlFileData(xmlFile, fileData);
            }
            catch (UnauthorizedAccessException)
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
            catch (ArgumentException)
            {
                return XmlLoadingResult.AlreadyLoaded;
            }
        }
    }

    [XmlRoot("CoCEd")]
    public sealed class XmlDataSet
    {
        [XmlElement("Version")]
        public string Version { get; set; }

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

        [XmlArray, XmlArrayItem("PropertyCount")]
        public XmlPropCount[] PropertyCounts { get; set; }
    }

    public sealed class XmlBodySet
    {
        [XmlArray, XmlArrayItem("SkinType")]
        public XmlEnum[] SkinTypes { get; set; }
        [XmlArray, XmlArrayItem("SkinPattern")]
        public XmlEnum[] SkinPatterns { get; set; }
        [XmlArray, XmlArrayItem("SkinDescription")]
        public String[] SkinDescriptions { get; set; }
        [XmlArray, XmlArrayItem("SkinAdjective")]
        public String[] SkinAdjectives { get; set; }
        [XmlArray, XmlArrayItem("SkinTone")]
        public String[] SkinTones { get; set; }
        [XmlArray, XmlArrayItem("HairType")]
        public XmlEnum[] HairTypes { get; set; }
        [XmlArray, XmlArrayItem("HairColor")]
        public String[] HairColors { get; set; }
        [XmlArray, XmlArrayItem("CoatCoverage")]
        public XmlEnum[] CoatCoverages { get; set; }
        [XmlArray, XmlArrayItem("FurColor")]
        public String[] FurColors { get; set; }

        [XmlArray, XmlArrayItem("BeardType")]
        public XmlEnum[] BeardTypes { get; set; }

        [XmlArray, XmlArrayItem("FaceType")]
        public XmlEnum[] FaceTypes { get; set; }
        [XmlArray, XmlArrayItem("TongueType")]
        public XmlEnum[] TongueTypes { get; set; }
        [XmlArray, XmlArrayItem("EyeType")]
        public XmlEnum[] EyeTypes { get; set; }
        [XmlArray, XmlArrayItem("EyeColor")]
        public string[] EyeColors { get; set; }
        [XmlArray, XmlArrayItem("EarType")]
        public XmlEnum[] EarTypes { get; set; }
        [XmlArray, XmlArrayItem("HornType")]
        public XmlEnum[] HornTypes { get; set; }
        [XmlArray, XmlArrayItem("AntennaeType")]
        public XmlEnum[] AntennaeTypes { get; set; }

        [XmlArray, XmlArrayItem("ArmType")]
        public XmlEnum[] ArmTypes { get; set; }
        [XmlArray, XmlArrayItem("ClawType")]
        public XmlEnum[] ClawTypes { get; set; }
        [XmlArray, XmlArrayItem("ClawTone")]
        public String[] ClawTones { get; set; }
        [XmlArray, XmlArrayItem("TailType")]
        public XmlEnum[] TailTypes { get; set; }
        [XmlArray, XmlArrayItem("RearBodyType")]
        public XmlEnum[] RearBodyTypes { get; set; }
        [XmlArray, XmlArrayItem("GillType")]
        public XmlEnum[] GillTypes { get; set; }
        [XmlArray, XmlArrayItem("WingType")]
        public XmlEnum[] WingTypes { get; set; }
        [XmlArray, XmlArrayItem("WingDescription")]
        public String[] WingDescriptions { get; set; }
        [XmlArray, XmlArrayItem("LowerBodyType")]
        public XmlEnum[] LowerBodyTypes { get; set; }
        [XmlArray, XmlArrayItem("PiercingType")]
        public XmlEnum[] PiercingTypes { get; set; }
        [XmlArray, XmlArrayItem("PiercingMaterial")]
        public String[] PiercingMaterials { get; set; }

        [XmlArray, XmlArrayItem("CockType")]
        public XmlEnum[] CockTypes { get; set; }
        [XmlArray, XmlArrayItem("CockSockType")]
        public XmlItem[] CockSockTypes { get; set; }
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
        ArmorCursed = 8,
        Shield = 16,
        Undergarment = 32,
        Jewelry = 64,
        Unknown = 128,
        All = Other | Weapon | Armor | ArmorCursed | Shield | Undergarment | Jewelry | Unknown,
    }

    public sealed class XmlItemGroup
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public ItemCategories Category { get; set; }

        [XmlElement("Item")]
        public List<XmlItem> Items { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class XmlPerkGroup
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement("Perk")]
        public List<XmlNamedVector4> Perks { get; set; }


        public override string ToString()
        {
            return Name;
        }
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

        public override string ToString()
        {
            return Name;
        }
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
        public string Type1 { get; set; }
        [XmlAttribute]
        public string Type2 { get; set; }
        [XmlAttribute]
        public string Type3 { get; set; }
        [XmlAttribute]
        public string Type4 { get; set; }

        [XmlAttribute]
        public string Label1 { get; set; }
        [XmlAttribute]
        public string Label2 { get; set; }
        [XmlAttribute]
        public string Label3 { get; set; }
        [XmlAttribute]
        public string Label4 { get; set; }

        public bool ShouldSerializeValue1() { return Value1 > 0; }
        public bool ShouldSerializeValue2() { return Value2 > 0; }
        public bool ShouldSerializeValue3() { return Value3 > 0; }
        public bool ShouldSerializeValue4() { return Value4 > 0; }
        
        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class XmlPropCount
    {
        [XmlAttribute]
        public string Version { get; set; }
        [XmlAttribute]
        public int Count { get; set; }

        public override string ToString()
        {
            return Version + " - " + Count;
        }
    }

    public enum XmlLoadingResult
    {
        Success,
        InvalidFile,
        NoPermission,
        MissingFile,
        AlreadyLoaded
    }
}
