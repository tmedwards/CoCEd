using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoCEd.Common;
using CoCEd.Model;
using CoCEd.ViewModel;
using Microsoft.Win32;

namespace CoCEd.View
{
    /// <summary>
    /// Interaction logic for TopBar.xaml
    /// </summary>
    public partial class TopBar : UserControl
    {
        Style _defaultStyle;
        public TopBar()
        {
            InitializeComponent();

            _defaultStyle = openButton.Style;
#if !DEBUG
            openButton.Style = (Style)Resources["HighlightedButton"];
#endif

            openMenu.PlacementTarget = openButton;
            saveMenu.PlacementTarget = saveButton;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                VM.Instance.SaveRequiredChanged += OnSaveRequiredChanged;
                VM.Instance.FileOpened += OnFileOpened;
            }
        }

        void OnSaveRequiredChanged(object sender, bool saveRequired)
        {
            if (saveRequired) saveButton.Style = (Style)Resources["HighlightedButton"];
            else saveButton.Style = _defaultStyle;
        }

        void OnFileOpened(object sender, EventArgs e)
        {
            openButton.Style = _defaultStyle;
        }

        void openMenu_Closed(object sender, EventArgs e)
        {
            openButton.IsChecked = false;
        }

        void saveMenu_Closed(object sender, EventArgs e)
        {
            saveButton.IsChecked = false;
        }

        void openButton_StateChanged(object sender, RoutedEventArgs e)
        {
            bool isChecked = (openButton.IsChecked == true);
            openButton.IsHitTestVisible = !isChecked;
            if (isChecked) SetItems(openMenu, FileManagerVM.GetOpenMenus());
            openMenu.IsOpen = isChecked;
        }

        void saveButton_StateChanged(object sender, RoutedEventArgs e)
        {
            bool isChecked = (saveButton.IsChecked == true);
            saveButton.IsHitTestVisible = !isChecked;
            if (isChecked) SetItems(saveMenu, FileManagerVM.GetSaveMenus());
            saveMenu.IsOpen = isChecked;
        }

        void SetItems(ContextMenu menu, IEnumerable<IMenuVM> items)
        {
            SetItems(menu, items, true);
        }

        void SetItems(ItemsControl menu, IEnumerable<IMenuBaseVM> items, bool isRoot)
        {
            menu.Items.Clear();
            bool needSeparator = false;
            foreach (var item in items)
            {
                needSeparator |= item.HasSeparatorBefore;
                if (!item.IsVisible) continue;

                if (needSeparator) menu.Items.Add(new Separator());
                needSeparator = false;

                var subMenu = new MenuItem();
                subMenu.Header = item;
                if (isRoot)
                {
                    subMenu.Style = (Style)FindResource("RootMenuStyle");
                    SetItems(subMenu, item.Children, false);
                }
                menu.Items.Add(subMenu);
            }
        }

        void subMenu_Click(object sender, RoutedEventArgs e)
        {
            var menu = (MenuItem)sender;
            var item = (IMenuItemVM)menu.DataContext;
            item.OnClick();
        }

        void menu_Click(object sender, RoutedEventArgs e)
        {
            var menu = (MenuItem)sender;
            var item = (IMenuVM)menu.DataContext;
            item.OnClick();
        }

    }
}
