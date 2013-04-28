using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CoCEd.Model;

namespace CoCEd.Common
{
    [TemplatePart(Name = "combo", Type = typeof(ComboBox))]
    public class MyComboBox : Control
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MyComboBox), new PropertyMetadata(null, OnPropertiesChanged));
        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(Object), typeof(MyComboBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPropertiesChanged));

        static MyComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MyComboBox), new FrameworkPropertyMetadata(typeof(MyComboBox)));
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        void OnPropertiesChanged()
        {
            if (ItemsSource == null) return;
            if (SelectedValue == null) return;

            if (SelectedValue is string)
            {
                var value = (string)SelectedValue;
                var items = ItemsSource.Cast<XmlEnumWithStringID>().ToList();
                if (items.Any(x => x.ID == value)) return;
                items.Add(new XmlEnumWithStringID { ID = value, Name = value });
                ItemsSource = items;
            }
            else
            {
                var value = (int)SelectedValue;
                var items = ItemsSource.Cast<XmlEnum>().ToList();
                if (items.Any(x => x.ID == value)) return;
                items.Add(new XmlEnum { ID = value, Name = "<unknown>" });
                ItemsSource = items;
            }
        }

        static void OnPropertiesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MyComboBox box = (MyComboBox)obj;
            box.OnPropertiesChanged();
        }
    }
}
