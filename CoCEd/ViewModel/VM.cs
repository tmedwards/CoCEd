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

            Game = new GameVM(_currentFile);

            OnPropertyChanged("Game");
            OnPropertyChanged("HasData");
            OnPropertyChanged("FileLabel");
            OnPropertyChanged("FileLabelVisibility");
            VM.Instance.NotifySaveRequiredChanged(false);
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
                MessageBox.Show("CoCEd does not have the permission do to this.");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("CoCEd does not have the permission to do this.");
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

    public abstract class NodeVM : BindableBase
    {
        protected readonly AmfNode _node;

        protected NodeVM(AmfNode node)
        {
            _node = node;
        }

        public dynamic GetValue(string name)
        {
            return _node[name];
        }

        public double GetDouble(string name)
        {
            return _node.GetDouble(name);
        }

        public int GetInt(string name, int? defaultValue = null)
        {
            return _node.GetInt(name, defaultValue);
        }

        public string GetString(string name)
        {
            return _node.GetString(name);
        }

        public bool GetBool(string name)
        {
            return _node.GetBool(name);
        }

        public bool SetValue(string name, dynamic value, [CallerMemberName] string propertyName = null)
        {
            if (_node[name] == value) return false;
            _node[name] = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetDouble(string name, double value, [CallerMemberName] string propertyName = null)
        {
            return SetValue(name, value, propertyName);
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            VM.Instance.NotifySaveRequiredChanged(true);
        }
    }

    public abstract class ArrayVM<TResult> : UpdatableCollection<AmfNode, TResult>, IArrayVM
    {
        readonly AmfNode _node;

        protected ArrayVM(AmfNode node, Func<AmfNode, TResult> selector)
            : base(node.Select(x => x.Value as AmfNode), selector)
        {
            _node = node;
        }

        void IArrayVM.Create()
        {
            AmfNode node = CreateNewNode();
            _node.Add(node);
            Update();
            VM.Instance.NotifySaveRequiredChanged(true);
        }

        void IArrayVM.Delete(int index)
        {
            Object removedValue;
            _node.Remove(index.ToString(), true, out removedValue);
            Update();
            VM.Instance.NotifySaveRequiredChanged(true);
        }

        void IArrayVM.MoveItemToIndex(int sourceIndex, int destIndex)
        {
            if (sourceIndex == destIndex) return;
            if (sourceIndex < destIndex) --destIndex;

            Object removedValue;
            if (!_node.Remove(sourceIndex.ToString(), true, out removedValue)) throw new InvalidOperationException();
            _node.Insert(removedValue, destIndex);
            Update();
            VM.Instance.NotifySaveRequiredChanged(true);
        }

        protected abstract AmfNode CreateNewNode();
    }
}

namespace System.Runtime.CompilerServices
{
    public class CallerMemberNameAttribute : Attribute
    {
    }
}