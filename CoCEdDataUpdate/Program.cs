using System;
using System.Net.Http;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuGet.Versioning;
using CoCEd.Common;
using CoCEd.Model;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Collections;

namespace CocEdDataUpdate
{
    public enum UpdateResult
    {
        UpdatedFiles,
        NoneNecessary,
        Error
    }

    class Program
    {
        static readonly string XianxiaFlagsFile = @"classes/classes/GlobalFlags/kFLAGS.as";
        static readonly string XianxiaStatusEffectsFile = @"classes/classes/StatusEffects.as";
        static readonly string XianxiaRepo = "Ormael7/Corruption-of-Champions";
        
        // first group is flag name, second is number, optional third is description
        static readonly string flagRegExString = @"^[\s]*public\s+static\s+const\s+([\w\d]+)\s*(?:\:\s*\w+)?\s*=\s*(\d+);\s*(?:\/\/[ \t]*(.*))?$";
        static readonly string quotecap = @"""(.*?)""";
        static readonly string statusEffectRegExString = @"^[\s]*public\s+static\s+const\s+([\w\d]+)\s*(?:\:\s*\w+)?\s*=\s*mk\s*\(\s*" + quotecap + @"\s*\)\s*;(?:[\t ]*\/\/([^\r\f\n]+))?";
        static readonly string statusEffectTypeRegExString = @"^[\s]*public\s+static\s+const\s+([\w\d]+)\s*(?:\:\s*\w+)?\s*=\s*(\w+\.TYPE)\s*;(?:[\t ]*\/\/([^\r\f\n]+))?";
        static async Task<int> Main(string[] args)
        {
            bool stop_at_end = true;
            if (args.Length > 0)
            {
                if (args.Contains("--no-stop"))
                {
                    stop_at_end = false;
                }
            }
            var result = await DoUpdate();
            Console.WriteLine("Done!");
            if (stop_at_end)
            {
                Console.WriteLine("Press any key to exit...");
                Console.Read();
            }
            return (int)result;
        }
        static bool IsLoadSuccess(XmlLoadingResult result)
        {
            return result == XmlLoadingResult.Success || result == XmlLoadingResult.AlreadyLoaded;
        }
        static string NormalizeXmlString(string s)
        {
            return s.Replace("\r", "");
        }
        static XmlEnum[] ParseFlags(string fileContent)
        {
            var flags = new List<XmlEnum>();
            Regex regex = new Regex(flagRegExString, RegexOptions.Multiline);
            var matches = regex.Matches(fileContent);
            foreach (Match match in matches)
            {
                string flag_name = match.Groups[1].Value;
                int flag_value = Int32.Parse(match.Groups[2].Value);
                string description = "";
                if (match.Groups.Count > 3)
                {
                    description = NormalizeXmlString(match.Groups[3].Value);
                }
                XmlEnum flag = new XmlEnum
                {
                    Name = flag_name,
                    ID = flag_value,
                };
                if (description != "")
                {
                    flag.Description = description;
                }
                Console.WriteLine("Name: " + flag_name + " value: " + flag_value + " description: " + description);
                flags.Add(flag);
            }
            return flags.ToArray();
        }

        static async Task<UpdateResult> UpdateFlags(string repo, string tree, XmlDataSet fileData)
        {
            var file = XianxiaFlagsFile;
            string content;
            try
            {
                content = await GitHubGetters.GetContentFile(repo, tree, file);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting " + file + ":");
                Console.WriteLine(e);
                return UpdateResult.Error;
            }
            XmlEnum[] flags = ParseFlags(content);
            fileData.Flags = flags;
            return UpdateResult.UpdatedFiles;
        }
        static XmlNamedVector4 HandleSpecialSE(XmlNamedVector4 special)
        {
            if (special.Name == "HeatEffect.TYPE")
            {
                special.Name = "heat";
                special.Description = "You are in a temporary state of constant feminine need, and are especially fertile. The only way to quench the intense heat in your snatch is to satiate your lust by finding a mate to make you pregnant.";
                special.Label1 = "Fertility bonus";
                special.Value1 = 10;

                special.Label2 = "Libido bonus";
                special.Value2 = 15;
                special.Label3 = "Hours remaining";
            }
            else if (special.Name == "RutEffect.TYPE")
            {
                special.Name = "rut";
                special.Description = "You are in an insatiable state of pure, carnal, masculine need. You just feel the urge to fuck pretty much anything and everything in sight. Unfortunately for you, you can't just beat the meat to beat the heat, so no matter how many living or debatably living things you fuck and manage to impregnate, you won't be able to escape this frenzied bout of lust until it passes of its own accord in due time. But then again, since you're reading this, why would you let it go when you can prolong and augment it indefinitely?";
                special.Label1 = "Cum production bonus";
                special.Value1 = 150;

                special.Label2 = "Libido bonus";
                special.Value2 = 5;
                special.Label3 = "Hours remaining";
            }
            else if(special.Name == "VampireThirstEffect.TYPE")
            {
                special.Name = "Vampire Thirst";
                special.Description = "I don't know how this works, don't fuck with this value";
            }
            else
            {
                //uhhhh, throw?
                throw new Exception("unrecognized typed status effect while parsing!");
            }
            return special;
        }

        static List<XmlNamedVector4> ParseStatusContentSection(string content)
        {
            List<XmlNamedVector4> ret = new List<XmlNamedVector4>();
            Regex regex = new Regex(statusEffectRegExString, RegexOptions.Multiline);
            //Regex regex = new Regex(thing);
            var matches = regex.Matches(content);
            foreach (Match match in matches)
            {
                string status_varname = match.Groups[1].Value;
                string status_name = match.Groups[2].Value;
                string description = "";
                if (match.Groups.Count > 3)
                {
                    description = NormalizeXmlString(match.Groups[3].Value);
                }
                XmlNamedVector4 status_effect = new XmlNamedVector4
                {
                    Name = status_name
                };
                if (description != "")
                {
                    // check if the description is actually a set of labels divided by '/' or ';'
                    var labels = description.Split('/');
                    if (labels.Length == 1) labels = description.Split(';');
                    if (labels.Length == 1)
                    {
                        status_effect.Description = description;
                    }
                    else
                    {
                        for (int i = 0; i < labels.Length; i++)
                        {
                            //normalize the label name by stripping out "v1", "=", ":", etc. and trimming whitespace
                            string label = labels[i].Replace("v" + (i + 1), "").Replace("=", "").Replace(":","").Trim();
                            switch (i)
                            {
                                case 0:
                                    status_effect.Label1 = label;
                                    Console.WriteLine("Label 1:"+ label);
                                    break;
                                case 1:
                                    status_effect.Label2 = label;
                                    Console.WriteLine("Label 2:" + label);
                                    break;
                                case 2:
                                    status_effect.Label3 = label;
                                    Console.WriteLine("Label 3:" + label);
                                    break;
                                case 3:
                                    status_effect.Label4 = label;
                                    Console.WriteLine("Label 4:" + label);
                                    break;
                            }
                        }
                        // hmm?
                        status_effect.Description = description;
                    }
                }
                ret.Add(status_effect);
            }

            //do the special ones now
            regex = new Regex(statusEffectTypeRegExString, RegexOptions.Multiline);
            matches = regex.Matches(content);
            foreach (Match match in matches)
            {
                XmlNamedVector4 status_effect = new XmlNamedVector4
                {
                    Name = match.Groups[2].Value
                };
                if (match.Groups.Count > 3 && match.Groups[3].Value != "")
                {
                    status_effect.Description = match.Groups[3].Value;
                }
                ret.Add(HandleSpecialSE(status_effect));
            }
            // Sort them alphabetically
            ret.Sort(Comparer<XmlNamedVector4>.Create((s1, s2)=> s1.Name.CompareTo(s2.Name)));
            return ret;
        }
        
        private class StatusParseStruct
        {
            public string Name;
            public string Content;
            public string EndString;
            public List<XmlNamedVector4> Statuses;
            public StatusParseStruct(string name, string end)
            {
                Name = name;
                Content = "";
                EndString = end;
                Statuses = new List<XmlNamedVector4>();
            }
        }
        static List<XmlNamedVector4> ParseStatusEffects(string content, Dictionary<string, string> oldDescs = null)
        {  
            if (oldDescs == null)
            {
                oldDescs = new Dictionary<string, string>();
            }
            List<XmlNamedVector4> ret = new List<XmlNamedVector4>();
            StringReader rstream = new StringReader(content);
            var structs = new Dictionary<string, StatusParseStruct>();

            string lastSection = ""; //For debugging
            string line = ""; // We declare it up here for debugging

            //We seperate the sections by type of status effect to affect ordering.
            //lines that contain these values means that the content section has come to an end.
            string metamorphEnd = "StrTouSpeCounter1";
            string counterEnd = "// Non-combat player perks";
            string nonCombatEnd = "//Old status plots";
            string oldStatusEnd = "//Prisoner status effects";
            string prisonerEnd = "DianaOff:StatusEffectType";
            string otherEnd = "// monster";
            // the rest below "monster" are all temporary/non-player effects, we don't add those

            structs.Add("metamorph", new StatusParseStruct("metamorph", metamorphEnd));
            structs.Add("counter", new StatusParseStruct("counter", counterEnd));
            structs.Add("nonCombatPlayer", new StatusParseStruct("nonCombatPlayer", nonCombatEnd));
            structs.Add("oldStatus", new StatusParseStruct("oldStatus", oldStatusEnd));
            structs.Add("prisoner", new StatusParseStruct("prisoner", prisonerEnd));
            structs.Add("other", new StatusParseStruct("other", otherEnd));

            try
            {
                var lastKey = structs.Keys.Last();
                var nextContent = "";
                foreach (string key in structs.Keys)
                {
                    var pstruct = structs[key];
                    pstruct.Content = nextContent;
                    // Read status effects for this section
                    while (true)
                    {
                        line = rstream.ReadLine();
                        // If we're at the end...
                        if (line.Contains(pstruct.EndString))
                        {
                            // set this for debugging
                            lastSection = key;
                            // make sure that the next content section gets this line, as it's been
                            // shifted off the stream
                            nextContent = line + "\r\n";
                            break;
                        }
                        else
                        {
                            pstruct.Content += line + "\r\n";
                        }
                    }
                    // parse this content section
                    pstruct.Statuses = ParseStatusContentSection(pstruct.Content);
                    // now add the old descriptions.
                    foreach (XmlNamedVector4 sts in pstruct.Statuses)
                    {
                        if (oldDescs.ContainsKey(sts.Name))
                        {
                            sts.Description = oldDescs[sts.Name];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing status effects!");
                Console.WriteLine("Was after reading section " + lastSection);
                Console.WriteLine("last line read was: " + line);
                Console.WriteLine(e);
                throw e;
            }
            // add them in this order
            ret = ret.Concat(structs["metamorph"].Statuses)
                     .Concat(structs["nonCombatPlayer"].Statuses)
                     .Concat(structs["counter"].Statuses)
                     .Concat(structs["prisoner"].Statuses)
                     .Concat(structs["other"].Statuses)
                     .Concat(structs["oldStatus"].Statuses)
                     .ToList();
            return ret;
        }
        static async Task<UpdateResult> UpdateStatusEffects(string repo, string tree, XmlDataSet fileData)
        {
            var file = XianxiaStatusEffectsFile;
            string content;
            try
            {
                content = await GitHubGetters.GetContentFile(repo, tree, file);

                Dictionary<string, string> olddescs = new Dictionary<string, string>();
                //collect the old descriptions
                foreach (XmlNamedVector4 status in fileData.Statuses)
                {
                    if (status.Description != null && status.Description != "")
                    {
                        olddescs.Add(status.Name, status.Description);
                    }
                }
                var statusEffects = ParseStatusEffects(content, olddescs);
                if (statusEffects == null)
                {
                    return UpdateResult.Error;
                }
                fileData.Statuses = statusEffects;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error with getting/parsing " + file + ":");
                Console.WriteLine(e);
                return UpdateResult.Error;
            }

            return UpdateResult.UpdatedFiles;
        }

        static async Task<UpdateResult> DoUpdate(bool useMaster = false)
        {
            try
            {
                string tree;
                var repo = XianxiaRepo;

                if (useMaster)
                {
                    tree = "master-wip";
                }
                else
                {
                    tree = await GitHubGetters.GetLatestVersion(repo);
                }
                var xmlPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                xmlPath = Path.Combine(xmlPath, XmlData.Files.Xianxia);
                XmlDataSet fileData = XmlData._LoadXmlData(xmlPath);

                if (useMaster == false)
                {
                    var currentVersion = NuGetVersion.Parse(fileData.Version);
                    var availableVersion = NuGetVersion.Parse(tree);
                    //If current version is at or greater than the available version, return
                    if (VersionComparer.Compare(currentVersion, availableVersion, VersionComparison.Default) >= 0)
                    {
                        Console.WriteLine("Xianxia data file is current, no need for update");
                        return UpdateResult.NoneNecessary;
                    }
                }
                if (await UpdateStatusEffects(repo, tree, fileData) != UpdateResult.UpdatedFiles)
                {
                    Console.WriteLine("Error! Could not parse status file!");
                    return UpdateResult.Error;
                }
                if (await UpdateFlags(repo, tree, fileData) != UpdateResult.UpdatedFiles)
                {
                    Console.WriteLine("Error! Could not parse flag file!");
                    return UpdateResult.Error;
                }
                fileData.Version = tree;
                if (XmlData._SaveXml(xmlPath, fileData) != XmlLoadingResult.Success)
                {
                    Console.WriteLine("Error! Could not save xml!");
                    return UpdateResult.Error;
                }
                Console.WriteLine("saved xml!");

            }
            catch (Exception e)
            {
                Console.WriteLine("Error! " + e.ToString());
                return UpdateResult.Error;
            }
            return UpdateResult.UpdatedFiles;
        }
    }
}
