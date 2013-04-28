using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CoCEd.Model;

namespace CoCEd.ViewModel
{
    public sealed class VM : BindableBase
    {
        public event EventHandler<bool> SaveRequiredChanged;

        private VM()
        {
            Data = XmlData.Instance;
            FileLabelVisibility = Visibility.Collapsed;
            Files = new FilesVM();
        }

        public static void Create()
        {
            Instance = new VM();
        }

        public static VM Instance
        {
            get;
            private set;
        }

        public FilesVM Files { get; private set; }
        public XmlData Data { get; private set; }
        public Visibility FileLabelVisibility { get; private set; }
        public GameVM Game { get; private set; }
        public string FileLabel { get; private set; }
        public bool SaveRequired { get; private set; }
        public bool HasData { get; private set; }

        public void SetCurrentFile(AmfFile file, CocDirectory directory)
        {
            FileLabelVisibility = Visibility.Visible;
            FileLabel = Path.GetFileNameWithoutExtension(file.FilePath);
            Game = new GameVM(file);
            HasData = true;

            OnPropertyChanged("Game");
            OnPropertyChanged("FileLabelVisibility");
            OnPropertyChanged("FileLabel");
            OnPropertyChanged("HasData");
        }

        const string _appTitle = "CoC Editor";
        public void NotifySaveRequiredChanged(bool saveRequired = true)
        {
            if (saveRequired == SaveRequired) return;

            SaveRequired = saveRequired;
            Application.Current.MainWindow.Title = saveRequired ? _appTitle + "*" : _appTitle;  // Databinding does not work for this
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

        protected dynamic GetValue(string name)
        {
            return _node[name];
        }

        protected double GetDouble(string name)
        {
            return (double)GetValue(name);
        }

        protected bool SetValue(string name, dynamic value, [CallerMemberName] string propertyName = null)
        {
            if (_node[name] == value) return false;
            _node[name] = value;
            OnPropertyChanged(propertyName);
            VM.Instance.NotifySaveRequiredChanged(true);
            return true;
        }

        protected bool SetDouble(string name, double value, [CallerMemberName] string propertyName = null)
        {
            return SetValue(name, value, propertyName);
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
