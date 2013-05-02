using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public delegate void SaveRequiredChanged(object sender, bool e);

    public sealed class VM : BindableBase
    {
        public event SaveRequiredChanged SaveRequiredChanged;

        const string AppTitle = "CoCEd";

        readonly List<string> _externalPaths = new List<string>();
        AmfFile _currentFile;

        private VM()
        {
        }

        public static void Create()
        {
            Instance = new VM();
            Instance.Data = XmlData.Instance;
        }

        public static VM Instance { get; private set; }

        public bool SaveRequired { get; private set; }
        public XmlData Data { get; private set; }
        public GameVM Game { get; private set; }

        public Visibility FileLabelVisibility 
        {
            get { return _currentFile == null ? Visibility.Collapsed : Visibility.Visible; }
        }

        public string FileLabel 
        {
            get { return _currentFile == null ? "" : Path.GetFileNameWithoutExtension(_currentFile.FilePath); }
        }

        public bool HasData
        {
            get { return _currentFile != null; }
        }

        public void Load(string path)
        {
            FileManager.StoreExternal(path);
            _currentFile = new AmfFile(path);
            ImportFileData(_currentFile);

            Game = new GameVM(_currentFile);

            OnPropertyChanged("Game");
            OnPropertyChanged("HasData");
            OnPropertyChanged("FileLabel");
            OnPropertyChanged("FileLabelVisibility");
            VM.Instance.NotifySaveRequiredChanged(false);
        }

        void ImportFileData(AmfFile _currentFile)
        {
            // WHICH GROUP?
            /*
            foreach (var perk in _currentFile.GetObj("perks").Select(x => x.ValueAsObject))
            {
                var name = perk.GetString("perkName");
                var perkData =
            }*/
        }

        public void Save(string path)
        {
            try
            {
                FileManager.StoreExternal(path);
                _currentFile.Save(path);
            }
            catch (SecurityException)
            {
                MessageBox.Show("CoCEd does not have the permission to write over this file or its backup.", "Permissions problem", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("CoCEd does not have the permission to write over this file or its backup.", "Permissions problem", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            VM.Instance.NotifySaveRequiredChanged(false);
        }

        public void NotifySaveRequiredChanged(bool saveRequired = true)
        {
            if (saveRequired == SaveRequired) return;

            SaveRequired = saveRequired;
            Application.Current.MainWindow.Title = saveRequired ? AppTitle + "*" : AppTitle;  // Databinding does not work for this
            if (SaveRequiredChanged != null) SaveRequiredChanged(null, saveRequired);
        }
    }


    public interface IArrayVM : IUpdatableList
    {
        void Create();
        void Delete(int index);
        void MoveItemToIndex(int sourceIndex, int destIndex);
    }

    public abstract class ObjectVM : BindableBase
    {
        protected readonly AmfObject _obj;

        protected ObjectVM(AmfObject obj)
        {
            _obj = obj;
        }

        public object GetValue(object key)
        {
            return _obj[key];
        }

        public double GetDouble(object key)
        {
            return _obj.GetDouble(key);
        }

        public int GetInt(object key, int? defaultValue = null)
        {
            return _obj.GetInt(key, defaultValue);
        }

        public string GetString(object key)
        {
            return _obj.GetString(key);
        }

        public bool GetBool(object key)
        {
            return _obj.GetBool(key);
        }

        public AmfObject GetObj(object key)
        {
            return _obj.GetObj(key);
        }

        public virtual bool SetValue(object key, object value, [CallerMemberName] string propertyName = null)
        {
            if (AmfObject.AreSame(_obj[key], value)) return false;
            _obj[key] = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetDouble(object key, double value, [CallerMemberName] string propertyName = null)
        {
            return SetValue(key, value, propertyName);
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            VM.Instance.NotifySaveRequiredChanged(true);
        }
    }

    public abstract class ArrayVM<TResult> : UpdatableCollection<AmfObject, TResult>, IArrayVM
    {
        readonly AmfObject _object;

        protected ArrayVM(AmfObject obj, Func<AmfObject, TResult> selector)
            : base(obj.Select(x => x.Value as AmfObject), selector)
        {
            _object = obj;
        }

        public void Create()
        {
            AmfObject node = CreateNewObject();
            Add(node);
        }

        protected TResult Add(AmfObject node)
        {
            _object.Push(node);
            Update();
            VM.Instance.NotifySaveRequiredChanged(true);
            return this[_object.DenseCount - 1];
        }

        public void Delete(int index)
        {
            _object.Pop(index);
            Update();
            VM.Instance.NotifySaveRequiredChanged(true);
        }

        void IArrayVM.MoveItemToIndex(int sourceIndex, int destIndex)
        {
            if (sourceIndex == destIndex) return;
            _object.Move(sourceIndex, destIndex);
            Update();
            VM.Instance.NotifySaveRequiredChanged(true);
        }

        protected abstract AmfObject CreateNewObject();
    }
}

namespace System.Runtime.CompilerServices
{
    public class CallerMemberNameAttribute : Attribute
    {
    }
}