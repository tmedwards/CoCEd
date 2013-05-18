using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using CoCEd.ViewModel;

namespace CoCEd.Model
{
    public static class FileManager
    {
        static readonly List<string> _externalPaths = new List<string>();

        public static string MoreThanOneFolderPath { get; private set; }
        public static string MissingPermissionPath { get; private set; }

        public static string StandardOfflinePath { get; private set; }
        public static string StandardOnlinePath { get; private set; }
        public static string ChromeOfflinePath { get; private set; }
        public static string ChromeOnlinePath { get; private set; }

        public static void BuildPaths()
        {
            const string standardPath = @"Macromedia\Flash Player\#SharedObjects\";
            StandardOfflinePath = BuildPath(Environment.SpecialFolder.ApplicationData, standardPath, "localhost");
            StandardOnlinePath = BuildPath(Environment.SpecialFolder.ApplicationData, standardPath, "www.fenoxo.com");

            const string chromePath = @"Google\Chrome\User Data\Default\Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects\";
            ChromeOfflinePath = BuildPath(Environment.SpecialFolder.LocalApplicationData, chromePath, "localhost");
            ChromeOnlinePath = BuildPath(Environment.SpecialFolder.LocalApplicationData, chromePath, "www.fenoxo.com");
        }

        static string BuildPath(Environment.SpecialFolder root, string middle, string suffix)
        {
            var path = "";
            try
            {
                // User\AppData\Roaming 
                path = Environment.GetFolderPath(root);
                if (path == null) return "";

                // User\AppData\Roaming\Macromedia\Flash Player\#SharedObjects\
                path = Path.Combine(path, middle);
                if (!Directory.Exists(path)) return "";

                // User\AppData\Roaming\Macromedia\Flash Player\#SharedObjects\qsdj8HdT7
                var subDirectories = Directory.GetDirectories(path);
                if (subDirectories.Length > 1) MoreThanOneFolderPath = path;
                if (subDirectories.Length != 1) return "";
                path = subDirectories[0];

                // User\AppData\Roaming\Macromedia\Flash Player\#SharedObjects\qsdj8HdT7\localhost
                path = Path.Combine(path, suffix);
                if (Directory.Exists(path)) return path;
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
            return "";
        }

        public static FileGroupSetVM CreateSet()
        {
            var set = new FileGroupSetVM();

            set.StandardOfflineFiles = CreateGroup(StandardOfflinePath);
            set.StandardOnlineFiles = CreateGroup(StandardOnlinePath);
            set.ChromeOfflineFiles = CreateGroup(ChromeOfflinePath);
            set.ChromeOnlineFiles = CreateGroup(ChromeOnlinePath);
            set.ExternalFiles = CreateExternalGroup();
            return set;
        }

        static FileGroupVM CreateExternalGroup()
        {
            var externalFiles = _externalPaths.Select(x => new AmfFile(x));
            return new FileGroupVM("", externalFiles, true);
        }

        static FileGroupVM CreateGroup(string path)
        {
            if (String.IsNullOrEmpty(path)) return new FileGroupVM("", new AmfFile[0]);

            List<AmfFile> files = new List<AmfFile>();
            for (int i = 1; i <= 10; i++)
            {
                var filePath = Path.Combine(path, "Coc_" + i + ".sol");
                try
                {
                    if (!File.Exists(filePath)) continue;
                    files.Add(new AmfFile(filePath));
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

            // Create group
            return new FileGroupVM(path, files);
        }

        public static void AddExternalFile(string path)
        {
            path = Canonize(path);

            if (_externalPaths.Contains(path)) return;
            if (AreParentAndChild(StandardOfflinePath, path)) return;
            if (AreParentAndChild(StandardOnlinePath, path)) return;
            if (AreParentAndChild(ChromeOfflinePath, path)) return;
            if (AreParentAndChild(ChromeOnlinePath, path)) return;

            _externalPaths.Add(path);
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
