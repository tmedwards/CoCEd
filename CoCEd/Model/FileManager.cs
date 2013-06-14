using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using CoCEd.ViewModel;

namespace CoCEd.Model
{
    public class FlashDirectory
    {
        public string Name;
        public string Path;
        public bool IsExternal;
        public bool HasSeparatorBefore;
        public readonly List<AmfFile> Files = new List<AmfFile>();

        public FlashDirectory(string name, string path, bool hasSeparatorBefore, bool isExternal)
        {
            Name = name;
            Path = path;
            HasSeparatorBefore = hasSeparatorBefore;
            IsExternal = isExternal;
        }
    }

    public static class FileManager
    {
        static readonly List<string> _externalPaths = new List<string>();
        static readonly List<FlashDirectory> _directories = new List<FlashDirectory>();

        public static string MissingPermissionPath { get; private set; }

        public static void BuildPaths()
        {
            bool separatorBefore = false;
            const string standardPath = @"Macromedia\Flash Player\#SharedObjects\";
            const string chromePath = @"Google\Chrome\User Data\Default\Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects\";


            BuildPath(Environment.SpecialFolder.ApplicationData,        "Offline (standard{0})",   standardPath,   "localhost",                        ref separatorBefore);
            BuildPath(Environment.SpecialFolder.LocalApplicationData,   "Offline (chrome{0})",     chromePath,     "localhost",                        ref separatorBefore);
            BuildPath(Environment.SpecialFolder.ApplicationData,        "Offline (metro{0})",      standardPath,   @"#AppContainer\localhost",         ref separatorBefore);

            separatorBefore = true;
            BuildPath(Environment.SpecialFolder.ApplicationData,        "Online (standard{0})",   standardPath,    "www.fenoxo.com",                   ref separatorBefore);
            BuildPath(Environment.SpecialFolder.LocalApplicationData,   "Online (chrome{0})",      chromePath,     "www.fenoxo.com",                   ref separatorBefore);
            BuildPath(Environment.SpecialFolder.ApplicationData,        "Online (metro{0})",      standardPath,    @"#AppContainer\www.fenoxo.com",    ref separatorBefore);
        }

        static void BuildPath(Environment.SpecialFolder root, string nameFormat, string middle, string suffix, ref bool separatorBefore)
        {
            var path = "";
            try
            {
                // User\AppData\Roaming 
                path = Environment.GetFolderPath(root);
                if (path == null) return;

                // User\AppData\Roaming\Macromedia\Flash Player\#SharedObjects\
                path = Path.Combine(path, middle);
                if (!Directory.Exists(path)) return;

                // User\AppData\Roaming\Macromedia\Flash Player\#SharedObjects\qsdj8HdT7
                var profileDirectories = Directory.GetDirectories(path);

                // User\AppData\Roaming\Macromedia\Flash Player\#SharedObjects\qsdj8HdT7\localhost
                var cocDirectories = new List<String>();
                for (int i = 0; i < profileDirectories.Length; ++i)
                {
                    path = Path.Combine(profileDirectories[i], suffix);
                    if (Directory.Exists(path)) cocDirectories.Add(path);
                }

                // Create items now that we know how many of them there are.
                for (int i = 0; i < cocDirectories.Count; ++i)
                {
                    var name = cocDirectories.Count > 1 ? String.Format(nameFormat, " #" + (i + 1)) : String.Format(nameFormat, "");
                    var flash = new FlashDirectory(name, cocDirectories[i], separatorBefore, false);
                    separatorBefore = false;
                    _directories.Add(flash);
                }
            }
            catch (SecurityException)
            {
                MissingPermissionPath = path;
            }
            catch (UnauthorizedAccessException)
            {
                MissingPermissionPath = path;
            }
            catch (IOException)
            {
            }
        }

        public static IEnumerable<FlashDirectory> GetDirectories()
        {
            foreach(var dir in _directories)
            {
                yield return CreateDirectory(dir);
            }
            yield return CreateExternalDirectory();
        }

        static FlashDirectory CreateExternalDirectory()
        {
            var dir = new FlashDirectory("External", "", true, true);
            foreach (var path in _externalPaths) dir.Files.Add(new AmfFile(path));
            return dir;
        }

        static FlashDirectory CreateDirectory(FlashDirectory dir)
        {
            dir = new FlashDirectory(dir.Name, dir.Path, dir.HasSeparatorBefore, false);
            if (String.IsNullOrEmpty(dir.Path)) return dir;

            for (int i = 1; i <= 10; i++)
            {
                var filePath = Path.Combine(dir.Path, "CoC_" + i + ".sol");
                try
                {
                    if (!File.Exists(filePath)) continue;
                    dir.Files.Add(new AmfFile(filePath));
                }
                catch (SecurityException)
                {
                    MissingPermissionPath = filePath;
                }
                catch (UnauthorizedAccessException)
                {
                    MissingPermissionPath = filePath;
                }
                catch (IOException)
                {
                }
            }
            return dir;
        }

        public static void AddExternalFile(string path)
        {
            path = Canonize(path);

            if (_externalPaths.Contains(path)) return;
            foreach (var dir in _directories)
            {
                if (AreParentAndChild(dir.Path, path)) return;
            }
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
            return path.ToLowerInvariant().Replace("/", "\\");
        }
    }
}
