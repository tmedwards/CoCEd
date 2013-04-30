using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CoCEd.Model;

namespace CoCEd.ViewModel
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

            var externalFiles = _externalPaths.Select(x => new AmfFile(x));
            set.ExternalFiles = new FileGroupVM("", externalFiles);
            return set;
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

        public static void StoreExternal(string path)
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

    public sealed class FileGroupSetVM
    {
        public FileGroupVM StandardOfflineFiles { get; set; }
        public FileGroupVM StandardOnlineFiles { get; set; }
        public FileGroupVM ChromeOfflineFiles { get; set; }
        public FileGroupVM ChromeOnlineFiles { get; set; }
        public FileGroupVM ExternalFiles { get; set; }

        // Fake properties for import/export menus in order to avoid binding errors
        public FileVM[] Files { get { return new FileVM[0]; } }
        public Object[] Targets { get { return new Object[0]; } }
        public Visibility MenuVisibility { get { return Visibility.Visible; } }
    }

    public class FileGroupVM : BindableBase
    {
        readonly string _path;

        public FileGroupVM(string path, IEnumerable<AmfFile> files, bool isExternal = false)
        {
            _path = path;
            IsExternal = isExternal;
            Files = files.Select(x => new FileVM(x, isExternal)).ToArray();

            if (IsExternal) Targets = Files;
            else Targets = EnumerateSaveTargets().ToArray();

            MenuVisibility = Files.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
            TargetForeground = Files.Length == 0 ? Brushes.DarkGray : Brushes.Black;
            TargetMenuVisibility = Targets.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        IEnumerable<Object> EnumerateSaveTargets()
        {
            if (String.IsNullOrEmpty(_path)) yield break;

            // Return either a SaveTargetVM or a FileVM
            for (int i = 1; i <= 10; i++)
            {
                var name = "Coc_" + i + ".sol";
                var file = Files.FirstOrDefault(x => x.Source.FilePath.EndsWith(name, StringComparison.InvariantCultureIgnoreCase));
                if (file != null)
                {
                    yield return file;
                }
                else
                {
                    var path = Path.Combine(_path, name);
                    var target = new SaveTargetVM { Label = "Coc_" + i, Path = path };
                    yield return target;
                }
            }
        }

        public bool IsExternal
        {
            get;
            private set;
        }

        public FileVM[] Files
        {
            get;
            private set;
        }

        public Object[] Targets
        {
            get;
            private set;
        }

        public Visibility MenuVisibility
        {
            get;
            private set;
        }

        public Visibility TargetMenuVisibility
        {
            get;
            private set;
        }

        public Brush TargetForeground
        {
            get;
            private set;
        }
    }

    public class FileVM
    {
        readonly bool _isExternal;

        public FileVM(AmfFile source, bool isExternal)
        {
            Source = source;
            _isExternal = isExternal;
        }

        public AmfFile Source 
        { 
            get; 
            private set; 
        }

        public string Path
        {
            get { return Source.FilePath; }
        }

        public string Label
        {
            get { return _isExternal ? System.IO.Path.GetFileNameWithoutExtension(Source.FilePath) : Source.Name; }
        }

        public string SubLabel
        {
            get { return Source["short"] + " - " + Source["days"] + " days" + " - " + Elapsed(); }
        }

        string Elapsed()
        {
            var elapsed = DateTime.Now - Source.Date;
            if (elapsed.TotalDays > 1) return (int)elapsed.TotalDays + " days ago";
            if (elapsed.TotalHours > 1) return (int)elapsed.TotalHours + " hours ago";
            if (elapsed.TotalMinutes > 1) return (int)elapsed.TotalMinutes + " minutes ago";
            return "1 minute ago";
        }

        public Brush Foreground
        {
            get { return Brushes.Black; }
        }

        public Visibility SubLabelVisibility
        {
            get { return Visibility.Visible; }
        }

        public Image Icon
        {
            get
            {
                if (String.IsNullOrEmpty(Source.Error)) return null;

                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri("pack://application:,,,/assets/cross.png", UriKind.Absolute);
                bmp.EndInit();

                var img = new Image();
                img.Source = bmp;
                return img;
            }
        }
    }

    public class SaveTargetVM
    {
        public string Path
        {
            get;
            set;
        }

        public string Label
        {
            get;
            set;
        }

        public string SubLabel
        {
            get;
            set;
        }

        public Brush Foreground
        {
            get { return Brushes.DarkGray; }
        }

        public Visibility SubLabelVisibility
        {
            get { return Visibility.Collapsed; }
        }
    }
}
