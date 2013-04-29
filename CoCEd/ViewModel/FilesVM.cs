using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public enum CocDirectory
    {
        Custom,
        ChromeOnline,
        ChromeOffline,
        StandardOnline,
        StandardOffline,
    }

    public sealed class FilesVM : BindableBase
    {
        readonly HashSet<String> _backedUpFiles = new HashSet<string>();
        readonly Dictionary<CocDirectory, String> _paths = new Dictionary<CocDirectory, String>();
        readonly Dictionary<CocDirectory, List<AmfFile>> _files = new Dictionary<CocDirectory, List<AmfFile>>();

        public FileGroupVM StandardOfflineFiles { get; private set; }
        public FileGroupVM StandardOnlineFiles { get; private set; }
        public FileGroupVM ChromeOfflineFiles { get; private set; }
        public FileGroupVM ChromeOnlineFiles { get; private set; }
        public FileGroupVM ExternalFiles { get; private set; }

        void UpdateDirectories()
        {
            ExternalFiles.Update();
            StandardOnlineFiles.Update();
            StandardOfflineFiles.Update();
            ChromeOfflineFiles.Update();
            ChromeOnlineFiles.Update();
        }

        public AmfScanResult LoadFiles()
        {
            // Default values
            foreach (CocDirectory value in Enum.GetValues(typeof(CocDirectory)))
            {
                _files[value] = new List<AmfFile>();
                _paths[value] = "";
            }

            // Import files
            var result = new AmfScanResult();
            ImportFiles(Environment.SpecialFolder.ApplicationData, @"Macromedia\Flash Player\#SharedObjects\", 
                CocDirectory.StandardOffline, CocDirectory.StandardOnline, result);

            ImportFiles(Environment.SpecialFolder.LocalApplicationData, @"Google\Chrome\User Data\Default\Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects\", 
                CocDirectory.ChromeOffline, CocDirectory.ChromeOnline, result);

            // Create collections
            ExternalFiles = new FileGroupVM(CocDirectory.Custom, _files[CocDirectory.Custom], "");
            StandardOnlineFiles = new FileGroupVM(CocDirectory.StandardOnline, _files[CocDirectory.StandardOnline], _paths[CocDirectory.StandardOnline]);
            StandardOfflineFiles = new FileGroupVM(CocDirectory.StandardOffline, _files[CocDirectory.StandardOffline], _paths[CocDirectory.StandardOffline]);
            ChromeOfflineFiles = new FileGroupVM(CocDirectory.ChromeOffline, _files[CocDirectory.ChromeOffline], _paths[CocDirectory.ChromeOffline]);
            ChromeOnlineFiles = new FileGroupVM(CocDirectory.ChromeOnline, _files[CocDirectory.ChromeOnline], _paths[CocDirectory.ChromeOnline]);
            UpdateDirectories();

            return result;
        }

        void ImportFiles(Environment.SpecialFolder root, string suffix, CocDirectory offline, CocDirectory online, AmfScanResult result)
        {
            string path = "";
            try
            {
                // User\AppData\Roaming 
                path = Environment.GetFolderPath(root);
                if (path == null) return;

                // User\AppData\Roaming\Macromedia\Flash Player\#SharedObjects\
                path = Path.Combine(path, suffix);

                // User\AppData\Roaming\Macromedia\Flash Player\#SharedObjects\qsdj8HdT7
                var subDirectories = Directory.GetDirectories(path);
                if (subDirectories.Length > 1) result.MoreThanOneFolderPath = path;
                if (subDirectories.Length != 1) return;
                path = subDirectories[0];

                // Scan files
                _paths[offline] = Path.Combine(path, "localhost");
                _files[offline] = ScanFiles(_paths[offline], result).ToList();

                _paths[online] = Path.Combine(path, "www.fenoxo.com");
                _files[online] = ScanFiles(_paths[online], result).ToList();
            }
            catch (SecurityException)
            {
                result.MissingPermissionPath = path;
            }
            catch (UnauthorizedAccessException)
            {
                result.MissingPermissionPath = path;
            }
            catch (DirectoryNotFoundException)
            {
            }
        }

        static IEnumerable<AmfFile> ScanFiles(string dirPath, AmfScanResult result)
        {
            for (int i = 1; i <= 10; i++)
            {
                AmfFile file = null;
                var filePath = Path.Combine(dirPath, "Coc_" + i + ".sol");
                try
                {
                    if (!File.Exists(filePath)) continue;
                    file = new AmfFile(filePath);
                }
                catch (SecurityException)
                {
                    result.MissingPermissionPath = filePath;
                }
                catch (UnauthorizedAccessException)
                {
                    result.MissingPermissionPath = filePath;
                }
                catch (DirectoryNotFoundException)
                {
                }
                if (file != null) yield return file;
            }
        }


        AmfFile _currentFileClone;
        public void Load(string path)
        {
            // Pick original or create
            CocDirectory directory;
            var file = GetFile(path, out directory);
            if (file == null)
            {
                file = new AmfFile(path);
                directory = Store(file);
            }

            // Set clone as "current"
            _currentFileClone = new AmfFile(file);
            VM.Instance.SetCurrentFile(_currentFileClone, directory);
            VM.Instance.NotifySaveRequiredChanged(false);
            UpdateDirectories();
        }

        public void Save(string path)
        {
            try
            {
                EnsureBackupExists(path);
                var file = _currentFileClone.Save(path);
                Store(file);
            }
            catch (SecurityException)
            {
                MessageBox.Show("The editor does not have the permission do to this.");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("The editor does not have the permission to do this.");
            }

            VM.Instance.NotifySaveRequiredChanged(false);
            UpdateDirectories();
        }

        private void EnsureBackupExists(string path)
        {
            try
            {
                // Backups are only done once per file throughout the lifetime of this process.
                var lowerPath = path.ToLowerInvariant();
                if (_backedUpFiles.Contains(lowerPath)) return;

                // Does not backup files created by us during the liftime of this process.
                if (!File.Exists(path))
                {
                    _backedUpFiles.Add(lowerPath);
                    return;
                }

                // Create backup
                var backUpPath = lowerPath.Replace(".sol", ".bak");
                File.Copy(path, backUpPath, true);
                _backedUpFiles.Add(lowerPath);
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (SecurityException)
            {
            }
        }

        AmfFile GetFile(string path, out CocDirectory directory)
        {
            foreach (var groupPair in _files)
            {
                foreach (var file in groupPair.Value)
                {
                    if (String.Equals(file.FilePath, path, StringComparison.InvariantCultureIgnoreCase))
                    {
                        directory = groupPair.Key;
                        return file;
                    }
                }
            }

            directory = CocDirectory.Custom;
            return null;
        }

        CocDirectory Store(AmfFile file)
        {
            foreach (var groupPair in _files)
            {
                for(int i = 0; i < groupPair.Value.Count; i++)
                {
                    var oldFile = groupPair.Value[i];
                    if (String.Equals(oldFile.FilePath, file.FilePath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        groupPair.Value[i] = file;
                        return groupPair.Key;
                    }
                }
            }

            _files[CocDirectory.Custom].Add(file);
            return CocDirectory.Custom;
        }
    }


    public class FileVM
    {
        public FileVM(AmfFile source, CocDirectory directory)
        {
            Source = source;
            Directory = directory;
        }

        public AmfFile Source { get; private set; }
        public CocDirectory Directory { get; private set; }

        public string Path
        {
            get { return Source.FilePath; }
        }

        public string Label
        {
            get { return Directory == CocDirectory.Custom ? System.IO.Path.GetFileNameWithoutExtension(Source.FilePath) : Source.Name; }
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
                if (!Source.HasError) return null;

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

    public class FileGroupVM : BindableBase
    {
        readonly CocDirectory _directory;
        readonly List<AmfFile> _files;
        readonly string _path;

        public FileGroupVM(CocDirectory dir, List<AmfFile> files, string path)
        {
            _path = path;
            _files = files;
            _directory = dir;

            Files = new UpdatableCollection<AmfFile, FileVM>(files, x => new FileVM(x, CocDirectory.Custom));
        }

        public UpdatableCollection<AmfFile, FileVM> Files
        {
            get;
            private set;
        }

        public List<Object> Targets
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

        public void Update()
        {
            Files.Update();
            Targets = EnumerateSaveTargets().ToList();
            MenuVisibility = Files.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            TargetMenuVisibility = Targets.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            TargetForeground = Files.Count == 0 ? Brushes.DarkGray : Brushes.Black;
            OnPropertyChanged("TargetMenuVisibility");
            OnPropertyChanged("TargetForeground");
            OnPropertyChanged("MenuVisibility");
            OnPropertyChanged("Targets");
        }

        IEnumerable<Object> EnumerateSaveTargets()
        {
            if (_directory == CocDirectory.Custom)
            {
                foreach (var file in _files)
                {
                    yield return new FileVM(file, CocDirectory.Custom);
                }
            }
            else
            {
                for (int i = 1; i <= 10; i++)
                {
                    var name = "Coc_" + i + ".sol";
                    var file = _files.FirstOrDefault(x => x.FilePath.EndsWith(name, StringComparison.InvariantCultureIgnoreCase));
                    if (file != null)
                    {
                        yield return new FileVM(file, _directory);
                    }
                    else
                    {
                        var path = Path.Combine(_path, name);
                        var target = new SaveTargetVM { Label = "Coc_" + i, Path = path };
                        yield return target;
                    }
                }
            }
        }
    }
}
