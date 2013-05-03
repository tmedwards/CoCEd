using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoCEd.ViewModel;

namespace CoCEd.Common
{
    public class ArrayInsertEventArgs : EventArgs
    {
        public Object Object { get; set; }
    }
    public delegate void ArrayInsertEventHandler(DependencyObject sender, ArrayInsertEventArgs e);


    [TemplatePart(Name = "contentBorder", Type = typeof(Border))]
    [TemplatePart(Name = "removeButton", Type = typeof(Button))]
    [TemplatePart(Name = "addButton", Type = typeof(Button))]
    [TemplatePart(Name = "listBox", Type = typeof(ListBox))]
    public class ArrayEditor : ContentControl
    {
        public static readonly RoutedEvent CreateEvent = EventManager.RegisterRoutedEvent("Create", RoutingStrategy.Direct, typeof(ArrayInsertEventHandler), typeof(ArrayEditor));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IArrayVM), typeof(ArrayEditor), new PropertyMetadata(null, OnPropertiesChanged));
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ArrayEditor), new PropertyMetadata(null, OnPropertiesChanged));
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(ArrayEditor), new PropertyMetadata("", OnPropertiesChanged));

        static ArrayEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ArrayEditor), new FrameworkPropertyMetadata(typeof(ArrayEditor)));
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public IArrayVM Items
        {
            get { return (IArrayVM)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        ListBox _listBox;
        Button _addButton;
        Button _removeButton;
        Border _contentBorder;
        public override void OnApplyTemplate()
        {
            if (_listBox != null) _listBox.Drop -= listBox_Drop;
            if (_listBox != null) _listBox.DragOver += _listBox_DragOver;
            if (_listBox != null) _listBox.DragEnter += _listBox_DragOver;
            if (_listBox != null) _listBox.PreviewMouseLeftButtonDown += _listBox_PreviewMouseLeftButtonDown;
            if (_listBox != null) _listBox.PreviewMouseMove -= listBox_PreviewMouseMove;
            if (_listBox != null) _listBox.SelectionChanged -= listBox_SelectionChanged;
            if (_removeButton != null) _removeButton.Click -= removeButton_Click;
            if (_addButton != null) _addButton.Click -= addButton_Click;


            _listBox = GetTemplateChild("listBox") as ListBox;
            _addButton = GetTemplateChild("addButton") as Button;
            _removeButton = GetTemplateChild("removeButton") as Button;
            _contentBorder = GetTemplateChild("contentBorder") as Border;

            if (_listBox != null) _listBox.Drop += listBox_Drop;
            if (_listBox != null) _listBox.DragOver += _listBox_DragOver;
            if (_listBox != null) _listBox.DragEnter += _listBox_DragOver;
            if (_listBox != null) _listBox.PreviewMouseLeftButtonDown += _listBox_PreviewMouseLeftButtonDown;
            if (_listBox != null) _listBox.PreviewMouseMove += listBox_PreviewMouseMove;
            if (_listBox != null) _listBox.SelectionChanged += listBox_SelectionChanged;
            if (_removeButton != null) _removeButton.Click += removeButton_Click;
            if (_addButton != null) _addButton.Click += addButton_Click;
            OnContentChanged();
        }

        Type _draggedType;
        void _listBox_DragOver(object sender, DragEventArgs e)
        {
            if (_draggedType != null && e.Data.GetDataPresent(_draggedType))
            {
                var data = MoveDraggedItem(e);
                if (data != null) e.Data.SetData(data); // The VM changed after the update
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        void listBox_Drop(object sender, DragEventArgs e)
        {
            MoveDraggedItem(e);
        }

        Object MoveDraggedItem(DragEventArgs e)
        {
            if (_draggedType == null) return null;
            var droppedData = e.Data.GetData(_draggedType);
            if (droppedData == null) return null;

            var targetItem = ((UIElement)e.OriginalSource).AncestorsAndSelf().FirstOrDefault(x => x is ListBoxItem) as ListBoxItem;
            int targetIndex = targetItem != null ? Items.IndexOf(targetItem.DataContext) : Items.Count - 1;
            int sourceIndex = Items.IndexOf(droppedData);

            if (sourceIndex == targetIndex) return null;
            Items.MoveItemToIndex(sourceIndex, targetIndex);
            _listBox.SelectedIndex = targetIndex;
            return Items[targetIndex];
        }

        Point _dragSource;
        void listBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // M1 not pressed?
            if (e.LeftButton != MouseButtonState.Pressed) return;

            // Not engouh distance from start point?
            var diff = e.GetPosition(this) - _dragSource;
            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance) return;

            // Get dragged item
            var draggedItem = ((UIElement)e.OriginalSource).AncestorsAndSelf().FirstOrDefault(x => x is ListBoxItem) as ListBoxItem;
            if (draggedItem == null) return;

            // Do drag drop
            _draggedType = draggedItem.DataContext.GetType();
            DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
            draggedItem.IsSelected = true;
        }

        void _listBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragSource = e.GetPosition(this);
        }

        void addButton_Click(object sender, RoutedEventArgs e)
        {
            Items.Create();
            _listBox.SelectedIndex = Items.Count - 1;
        }

        void removeButton_Click(object sender, RoutedEventArgs e)
        {
            int index = _listBox.SelectedIndex;
            _listBox.SelectedIndex = Math.Min(index + 1, Items.Count - 2);
            Items.Delete(index);
            OnContentChanged();
        }

        static void OnPropertiesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ArrayEditor box = (ArrayEditor)obj;
            if (box._listBox == null) return;
            box.OnContentChanged();
        }

        void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnContentChanged();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            OnContentChanged();
        }

        void OnContentChanged()
        {
            if (_listBox == null) return;

            var item = _listBox.SelectedItem;
            if (item != null)
            {
                _contentBorder.DataContext = item;
                _contentBorder.Visibility = Visibility.Visible;
                _removeButton.IsEnabled = true;
                return;
            }

            if (Items == null || Items.Count == 0)
            {
                _contentBorder.DataContext = null;
                _contentBorder.Visibility = Visibility.Collapsed;
                _removeButton.IsEnabled = false;
                return;
            }

            if (_listBox.SelectedIndex == -1) _listBox.SelectedIndex = 0;
        }
    }
}
