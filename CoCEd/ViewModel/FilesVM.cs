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

        IEnumerable<ISaveTarget> EnumerateSaveTargets()
        {
            // Path not found?
            if (String.IsNullOrEmpty(_path)) yield break;

            // Return either a SaveTargetVM or a FileVM for every slot
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
                    var target = new SaveSlotVM { Label = "Coc_" + i, Path = path };
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

    public interface ISaveTarget
    {
        string Path { get; }
        SerializationFormat Format { get; }
    }

    public class FileVM : ISaveTarget
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

        public SerializationFormat Format 
        {
            get { return Source.Format; }
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

    public class SaveSlotVM : ISaveTarget
    {
        public SerializationFormat Format
        {
            get { return SerializationFormat.Slot; }
        }

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
