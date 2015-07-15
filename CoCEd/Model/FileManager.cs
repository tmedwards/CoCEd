using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using CoCEd.ViewModel;

namespace CoCEd.Model
{
    public enum DirectoryKind
    {
        Regular,
        External,
        Backup,
    }

    public class FlashDirectory
    {
        public string Name;
        public string Path;
        public bool HasSeparatorBefore;
        public readonly DirectoryKind Kind;
        public readonly List<AmfFile> Files = new List<AmfFile>();

        public FlashDirectory(string name, string path, bool hasSeparatorBefore, DirectoryKind kind)
        {
            Name = name;
            Path = path;
            Kind = kind;
            HasSeparatorBefore = hasSeparatorBefore;
        }
    }

    public static class FileManager
    {
        static readonly List<string> _externalPaths = new List<string>();
        static readonly List<FlashDirectory> _directories = new List<FlashDirectory>();

        const int MaxBackupFiles = 10;

        const int MaxSaveSlotsCoC = 9;
        const int MaxSaveSlotsRevampMod = 14;
        public const int SaveSlotsLowerBound = 1;
        public const int SaveSlotsUpperBound = MaxSaveSlotsRevampMod; // must use largest value here

        public static int SaveSlotsUpperBoundByGame
        {
            get { return VM.Instance.IsRevampMod ? MaxSaveSlotsRevampMod : MaxSaveSlotsCoC; }
        }

        public static string PathWithMissingPermissions { get; private set; }

        public static string BackupPath
        {
            get 
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(path, "CoCEd");
            }
        }

        public static void BuildPaths()
        {
            const string standardPath = @"Macromedia\Flash Player\#SharedObjects\";
            const string chromePath1 = @"Google\Chrome\User Data\Default\Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects\";
            const string chromePath2 = @"Google\Chrome\User Data\Profile 1\Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects\"; // Win 8/8.1 thing, apparently

            string[] standardPaths = { standardPath };
            string[] chromePaths = { chromePath1, chromePath2 };

            bool insertSeparatorBeforeInMenu = false;

            BuildPath("Local (standard{0})",        Environment.SpecialFolder.ApplicationData,      standardPaths,  "localhost",                     ref insertSeparatorBeforeInMenu);
            BuildPath("Local (chrome{0})",          Environment.SpecialFolder.LocalApplicationData, chromePaths,    "localhost",                     ref insertSeparatorBeforeInMenu);
            BuildPath("Local (metro{0})",           Environment.SpecialFolder.ApplicationData,      standardPaths,  @"#AppContainer\localhost",      ref insertSeparatorBeforeInMenu);

            insertSeparatorBeforeInMenu = true;
            BuildPath("LocalWithNet (standard{0})", Environment.SpecialFolder.ApplicationData,      standardPaths,  "#localWithNet",                 ref insertSeparatorBeforeInMenu);
            BuildPath("LocalWithNet (chrome{0})",   Environment.SpecialFolder.LocalApplicationData, chromePaths,    "#localWithNet",                 ref insertSeparatorBeforeInMenu);
            BuildPath("LocalWithNet (metro{0})",    Environment.SpecialFolder.ApplicationData,      standardPaths,  @"#AppContainer\#localWithNet",  ref insertSeparatorBeforeInMenu);

            insertSeparatorBeforeInMenu = true;
            BuildPath("Online (standard{0})",       Environment.SpecialFolder.ApplicationData,      standardPaths,  "www.fenoxo.com",                ref insertSeparatorBeforeInMenu);
            BuildPath("Online (chrome{0})",         Environment.SpecialFolder.LocalApplicationData, chromePaths,    "www.fenoxo.com",                ref insertSeparatorBeforeInMenu);
            BuildPath("Online (metro{0})",          Environment.SpecialFolder.ApplicationData,      standardPaths,  @"#AppContainer\www.fenoxo.com", ref insertSeparatorBeforeInMenu);
        }

        static void BuildPath(string nameFormat, Environment.SpecialFolder root, string[] middle, string suffix, ref bool separatorBefore)
        {
            var path = "";
            try
            {
                // User\AppData\Roaming
                var basePath = Environment.GetFolderPath(root);
                if (basePath == null) return;

                var cocDirectories = new List<String>();
                for (int i = 0; i < middle.Length; ++i)
                {
                    path = basePath;

                    // User\AppData\Roaming\Macromedia\Flash Player\#SharedObjects
                    path = Path.Combine(path, middle[i]);
                    if (!Directory.Exists(path)) continue;

                    // User\AppData\Roaming\Macromedia\Flash Player\#SharedObjects\qsdj8HdT7
                    var profileDirectories = Directory.GetDirectories(path);

                    // User\AppData\Roaming\Macromedia\Flash Player\#SharedObjects\qsdj8HdT7\localhost
                    for (int j = 0; j < profileDirectories.Length; ++j)
                    {
                        path = Path.Combine(profileDirectories[j], suffix);
                        if (Directory.Exists(path)) cocDirectories.Add(path);
                    }

                }

                // Create items now that we know how many of them there are.
                for (int i = 0; i < cocDirectories.Count; ++i)
                {
                    var name = String.Format(nameFormat, cocDirectories.Count > 1 ? " #" + (i + 1) : "");
                    var flash = new FlashDirectory(name, cocDirectories[i], separatorBefore, DirectoryKind.Regular);
                    separatorBefore = false;
                    _directories.Add(flash);
                }
            }
            catch (SecurityException)
            {
                PathWithMissingPermissions = path;
            }
            catch (UnauthorizedAccessException)
            {
                PathWithMissingPermissions = path;
            }
            catch (IOException)
            {
            }
        }

        public static IEnumerable<FlashDirectory> GetDirectories()
        {
            foreach (var dir in _directories)
            {
                yield return CreateDirectory(dir);
            }
            yield return CreateExternalDirectory();
        }

        public static FlashDirectory CreateBackupDirectory()
        {
            var dir = new FlashDirectory("Backup", BackupPath, true, DirectoryKind.Backup);
            
            var dirInfo = new DirectoryInfo(BackupPath);
            foreach (var file in dirInfo.GetFiles("*.bak").OrderByDescending(x => x.LastWriteTimeUtc)) dir.Files.Add(new AmfFile(file.FullName));
            return dir;
        }

        static FlashDirectory CreateExternalDirectory()
        {
            var dir = new FlashDirectory("External", "", true, DirectoryKind.Backup);
            foreach (var path in _externalPaths) dir.Files.Add(new AmfFile(path));
            return dir;
        }

        static FlashDirectory CreateDirectory(FlashDirectory dir)
        {
            dir = new FlashDirectory(dir.Name, dir.Path, dir.HasSeparatorBefore, DirectoryKind.Regular);
            if (String.IsNullOrEmpty(dir.Path)) return dir;

            for (int i = SaveSlotsLowerBound; i <= SaveSlotsUpperBound; i++)
            {
                var filePath = Path.Combine(dir.Path, "CoC_" + i + ".sol");
                try
                {
                    if (!File.Exists(filePath)) continue;
                    dir.Files.Add(new AmfFile(filePath));
                }
                catch (SecurityException)
                {
                    PathWithMissingPermissions = filePath;
                }
                catch (UnauthorizedAccessException)
                {
                    PathWithMissingPermissions = filePath;
                }
                catch (IOException)
                {
                }
            }
            return dir;
        }

        public static void TryRegisterExternalFile(string path)
        {
            path = Canonize(path);

            // Is it a regular file?
            foreach (var dir in _directories)
            {
                if (AreParentAndChild(dir.Path, path)) return;
            }

            // Is it a backup?
            if (Path.GetDirectoryName(path) == BackupPath) return;

            // Is this file already known?
            if (_externalPaths.Contains(path)) return;

            _externalPaths.Add(path);
        }

        public static bool IsCoCPath(string path)
        {
            path = Canonize(path);

            foreach (var dir in _directories)
            {
                if (AreParentAndChild(dir.Path, path)) return true;
            }
            return false;
        }

        static bool AreParentAndChild(string dirPath, string filePath)
        {
            if (String.IsNullOrEmpty(dirPath)) return false;
            dirPath = Canonize(dirPath);
            return filePath.StartsWith(dirPath);
        }

        static string Canonize(string path)
        {
            return path.Replace("/", "\\");
        }

        public static void CreateBackup(string sourcePath)
        {
            var backupDir = new DirectoryInfo(BackupPath);

            var existingFiles = backupDir.GetFiles("*.bak").OrderByDescending(x => x.LastWriteTimeUtc).ToArray();
            CopyToBackupPath(sourcePath);

            if (TryDeleteIdenticalFile(sourcePath, existingFiles)) return;

            for (int i = MaxBackupFiles; i < existingFiles.Length; ++i)
            {
                existingFiles[i].Delete();
            }
        }

        static void CopyToBackupPath(string sourcePath)
        {
            var targetName = DateTime.UtcNow.Ticks + ".bak";
            var targetPath = Path.Combine(BackupPath, targetName);
            File.Copy(sourcePath, targetPath, true);
        }

        static bool TryDeleteIdenticalFile(string sourcePath, FileInfo[] existingFiles)
        {
            var sourceData = File.ReadAllBytes(sourcePath);

            foreach (var file in existingFiles)
            {
                if (AreIdentical(file, sourceData))
                {
                    file.Delete();
                    return true;
                }
            }
            return false;
        }

        static bool AreIdentical(FileInfo x, byte[] yData)
        {
            if (x.Length != yData.Length) return false;

            var xData = File.ReadAllBytes(x.FullName);
            for (int i = 0; i < xData.Length; ++i)
            {
                if (xData[i] != yData[i]) return false;
            }

            return true;
        }
    }
}
