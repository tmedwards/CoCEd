using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CoCEd.Model;
using CoCEd.ViewModel;

namespace CoCEd.ViewModel
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public virtual bool SetValue(AmfObject obj, object key, object value, [CallerMemberName] string propertyName = null)
        {
            if (AmfObject.AreSame(obj[key], value)) return false;
            obj[key] = value;

            OnSavePropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnSavePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(propertyName);
            VM.Instance.NotifySaveRequiredChanged(true);
        }
    }
}

namespace System.Runtime.CompilerServices
{
    public class CallerMemberNameAttribute : Attribute
    {
    }
}