﻿using System;
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
using Microsoft.Win32;

namespace CoCEd.ViewModel
{
    public static class FileManagerVM
    {
        public static IEnumerable<IMenuVM> GetOpenMenus()
        {
            foreach (var dir in FileManager.GetDirectories())
            {
                yield return new SourceDirectoryVM(dir);
            }
            yield return new SourceDirectoryVM(FileManager.CreateBackupDirectory());
            yield return new ImportRootVM();
        }

        public static IEnumerable<IMenuVM> GetSaveMenus()
        {
            foreach (var dir in FileManager.GetDirectories())
            {
                yield return new TargetDirectoryVM(dir);
            }
            yield return new ExportRootVM();
        }
    }

    public interface IMenuBaseVM
    {
        bool IsVisible { get; }
        bool HasSeparatorBefore { get; }
        IEnumerable<IMenuItemVM> Children { get; }
    }

    public interface IMenuVM : IMenuBaseVM
    {
        String Label { get; }
        String ChildrenCount { get; }
        Visibility ChildrenCountVisibility { get; }
        Brush Foreground { get; }
        void OnClick();
    }

    public interface IMenuItemVM : IMenuBaseVM
    {
        string Path { get; }
        string Label { get; }
        string SubLabel { get; }
        Visibility SubLabelVisibility { get; }
        Brush Foreground { get; }
        Image Icon { get; }

        void OnClick();
    }

    public sealed class SourceDirectoryVM : IMenuVM
    {
        readonly FlashDirectory _directory;

        public SourceDirectoryVM(FlashDirectory directory)
        {
            _directory = directory;
        }

        public string Label
        {
            get { return _directory.Name; }
        }

        public IEnumerable<IMenuItemVM> Children
        {
            get 
            { 
                foreach(var file in _directory.Files) yield return new FileVM(file, _directory.Kind, true);
                if (!String.IsNullOrEmpty(_directory.Path)) yield return new OpenDirectoryItemVM(_directory.Path);
            }
        }

        public string ChildrenCount
        {
            get { return _directory.Files.Count.ToString(); }
        }

        public Visibility ChildrenCountVisibility
        {
            get { return Visibility.Visible; }
        }

        public bool HasSeparatorBefore
        {
            get { return _directory.HasSeparatorBefore; }
        }

        public bool IsVisible
        {
            get { return _directory.Files.Count != 0; }
        }

        public Brush Foreground
        {
            get { return Brushes.Black; }
        }

        public void OnClick()
        {
        }
    }

    public sealed class TargetDirectoryVM : IMenuVM
    {
        readonly FlashDirectory _directory;

        public TargetDirectoryVM(FlashDirectory directory)
        {
            _directory = directory;
        }

        public string Label
        {
            get { return _directory.Name; }
        }

        public bool HasSeparatorBefore
        {
            get { return _directory.HasSeparatorBefore; }
        }

        public bool IsVisible
        {
            get { return _directory.Files.Count != 0 || !String.IsNullOrEmpty(_directory.Path); }
        }

        public Brush Foreground
        {
            get { return _directory.Files.Count == 0 ? Brushes.DarkGray : Brushes.Black; }
        }

        public IEnumerable<IMenuItemVM> Children
        {
            get
            {
                if (_directory.Kind == DirectoryKind.External)
                {
                    foreach (var file in _directory.Files) yield return new FileVM(file, _directory.Kind, false);
                    yield break;
                }

                if (_directory.Kind == DirectoryKind.Backup)
                {
                    foreach (var file in _directory.Files) yield return new FileVM(file, _directory.Kind, false);
                    yield return new OpenDirectoryItemVM(_directory.Path); // Open directory
                    yield break;
                }

                // Path not found?
                if (String.IsNullOrEmpty(_directory.Path)) yield break;

                // Return either a SaveTargetVM or a FileVM for every slot
                for (int i = FileManager.SaveSlotsLowerBound; i <= FileManager.SaveSlotsUpperBoundByGame; i++)
                {
                    var name = "CoC_" + i + ".sol";
                    var file = _directory.Files.FirstOrDefault(x => x.FilePath.EndsWith(name, StringComparison.InvariantCultureIgnoreCase));
                    if (file != null)
                    {
                        yield return new FileVM(file, _directory.Kind, false);
                    }
                    else
                    {
                        var path = Path.Combine(_directory.Path, name);
                        var target = new SaveSlotVM { Label = "CoC_" + i, Path = path };
                        yield return target;
                    }
                }

                // "Open directory" entry
                yield return new OpenDirectoryItemVM(_directory.Path);
            }
        }

        public string ChildrenCount
        {
            get { return _directory.Files.Count.ToString(); }
        }

        public Visibility ChildrenCountVisibility
        {
            get { return Visibility.Visible; }
        }

        public void OnClick()
        {
        }
    }

    public sealed class ImportRootVM : IMenuVM
    {
        public string Label
        {
            get { return "Import…"; }
        }

        public IEnumerable<IMenuItemVM> Children
        {
            get { yield break; }
        }

        public string ChildrenCount
        {
            get { return ""; }
        }

        public Visibility ChildrenCountVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public bool HasSeparatorBefore
        {
            get { return false; }
        }

        public bool IsVisible
        {
            get { return true; }
        }

        public Brush Foreground
        {
            get { return Brushes.Black; }
        }

        public void OnClick()
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "\"Slot\" format|*.sol|\"Save to File\" format|*";
            dlg.DefaultExt = ".sol";
            dlg.CheckFileExists = true;
            dlg.Multiselect = false;
            dlg.RestoreDirectory = true;

            var result = dlg.ShowDialog();
            if (result == false) return;

            string path = dlg.FileName;
            VM.Instance.Load(path, dlg.FilterIndex == 1 ? SerializationFormat.Slot : SerializationFormat.Exported, createBackup: true);
        }
    }

    public sealed class ExportRootVM : IMenuVM
    {
        public ExportRootVM()
        {
        }

        public string Label
        {
            get { return "Export…"; }
        }

        public IEnumerable<IMenuItemVM> Children
        {
            get { yield break; }
        }

        public string ChildrenCount
        {
            get { return ""; }
        }

        public Visibility ChildrenCountVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public bool HasSeparatorBefore
        {
            get { return false; }
        }

        public bool IsVisible
        {
            get { return true; }
        }

        public Brush Foreground
        {
            get { return Brushes.Black; }
        }

        public void OnClick()
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "\"Slot\" format (.sol)|*.sol|\"Save to File\" format|*";
            dlg.AddExtension = true;
            dlg.OverwritePrompt = true;
            dlg.RestoreDirectory = true;
            dlg.ValidateNames = true;

            var result = dlg.ShowDialog();
            if (result == false) return;

            string path = dlg.FileName;
            var format = (SerializationFormat)(dlg.FilterIndex - 1);

            /*// CoC cannot read files from an external location, so prompt the user for confirmation.
            if (format == SerializationFormat.Exported && !FileManager.IsCoCPath(path))
            {
                var confirmation = MessageBox.Show("The directory you selected is not a CoC folder.\nCoC will cause an error if you try to load this file from this location.\nDo you want to cancel?", "CoC cannot read this location.", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (confirmation == MessageBoxResult.Cancel) return;
            }*/
            // Ok, so those paths still not work. I wish I could one day manage to load a file through "load file" to understand how what is the prerequisite.

            VM.Instance.Save(path, format);
        }
    }

    public class FileVM : IMenuItemVM
    {
        readonly DirectoryKind _directoryKind;
        readonly bool _openOnClick;

        public FileVM(AmfFile source, DirectoryKind directoryKind, bool openOnClick)
        {
            Source = source;
            _openOnClick = openOnClick;
            _directoryKind = directoryKind;
        }

        public AmfFile Source 
        { 
            get; 
            private set; 
        }

        public bool IsVisible
        {
            get { return true; }
        }

        public bool HasSeparatorBefore
        {
            get { return false; }
        }

        public IEnumerable<IMenuItemVM> Children
        {
            get { yield break; }
        }

        public string Path
        {
            get { return Source.FilePath; }
        }

        public string Label
        {
            get 
            {
                switch (_directoryKind)
                {
                    case DirectoryKind.External: return System.IO.Path.GetFileNameWithoutExtension(Source.FilePath);
                    case DirectoryKind.Regular: return Source.Name;
                    case DirectoryKind.Backup: return Source.Name + " - " + Source.Date.ToString();
                    default: throw new NotImplementedException();
                }
            }
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

        public void OnClick()
        {
            if (_openOnClick) VM.Instance.Load(Source.FilePath, SerializationFormat.Slot, createBackup: _directoryKind != DirectoryKind.Backup);
            else VM.Instance.Save(Source.FilePath, Source.Format);
        }
    }

    public class SaveSlotVM : IMenuItemVM
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

        public bool IsVisible
        {
            get { return true; }
        }

        public bool HasSeparatorBefore
        {
            get { return false; }
        }

        public IEnumerable<IMenuItemVM> Children
        {
            get { yield break; }
        }

        public Image Icon
        {
            get { return null; }
        }

        public Brush Foreground
        {
            get { return Brushes.DarkGray; }
        }

        public Visibility SubLabelVisibility
        {
            get { return Visibility.Collapsed; }
        }

        void IMenuItemVM.OnClick()
        {
            VM.Instance.Save(Path, SerializationFormat.Slot);
        }
    }

    public sealed class OpenDirectoryItemVM : IMenuItemVM
    {
        public OpenDirectoryItemVM(string path)
        {
            Path = path;
        }

        public string Path
        {
            get;
            set;
        }

        public string Label
        {
            get { return "Open directory…"; }
        }

        public string SubLabel
        {
            get { return null; }
        }

        public bool IsVisible
        {
            get { return true; }
        }

        public bool HasSeparatorBefore
        {
            get { return true; }
        }

        public IEnumerable<IMenuItemVM> Children
        {
            get { yield break; }
        }

        public Image Icon
        {
            get { return null; }
        }

        public Brush Foreground
        {
            get { return Brushes.Black; }
        }

        public Visibility SubLabelVisibility
        {
            get { return Visibility.Collapsed; }
        }

        void IMenuItemVM.OnClick()
        {
            Process.Start(Path);
        }
    }
}
