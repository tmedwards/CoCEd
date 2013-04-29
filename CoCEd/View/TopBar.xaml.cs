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
using CoCEd.Common;
using CoCEd.ViewModel;
using Microsoft.Win32;

namespace CoCEd.View
{
    /// <summary>
    /// Interaction logic for TopBar.xaml
    /// </summary>
    public partial class TopBar : UserControl
    {
        public TopBar()
        {
            InitializeComponent();
            openMenu.PlacementTarget = openButton;
            saveMenu.PlacementTarget = saveButton;
            if (!DesignerProperties.GetIsInDesignMode(this)) VM.Instance.SaveRequiredChanged += OnSaveRequiredChanged;
        }

        void OnSaveRequiredChanged(object sender, bool saveRequired)
        {
            if (saveRequired) saveButton.Style = (Style)Resources["HighlightedButton"];
            else saveButton.Style = null;
        }

        void openPopup_Closed(object sender, EventArgs e)
        {
            openButton.IsChecked = false;
        }

        void savePopup_Closed(object sender, EventArgs e)
        {
            saveButton.IsChecked = false;
        }

        void openButton_StateChanged(object sender, RoutedEventArgs e)
        {
            bool isChecked = (openButton.IsChecked == true);
            openButton.IsHitTestVisible = !isChecked;
            openMenu.IsOpen = isChecked;
        }

        void saveButton_StateChanged(object sender, RoutedEventArgs e)
        {
            bool isChecked = (saveButton.IsChecked == true);
            saveButton.IsHitTestVisible = !isChecked;
            saveMenu.IsOpen = isChecked;
        }

        void openMenu_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            var file = (FileVM)item.DataContext;
            if (!String.IsNullOrEmpty(file.Source.Error))
            {
                var result = MessageBox.Show("This file was not loaded correctly, this could lead to corrupted savegames, are you sure you want to load it?\n\n" + file.Source.Error, "This file has errors", MessageBoxButton.OKCancel);
                Logger.Error(file.Source.Error);
                if (result != MessageBoxResult.OK) return;
            }
            VM.Instance.Files.Load(file.Source.FilePath);
        }

        void saveMenu_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            dynamic file = item.DataContext;
            VM.Instance.Files.Save(file.Path);
        }

        void importMenu_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Flash local shared objects (.sol)|*.sol";
            dlg.DefaultExt = ".sol";
            dlg.CheckFileExists = true;
            dlg.Multiselect = false;
            dlg.RestoreDirectory = true;

            var result = dlg.ShowDialog();
            if (result == false) return;

            string path = dlg.FileName;
            VM.Instance.Files.Load(path);
        }

        void exportMenu_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "Flash local shared objects (.sol)|*.sol";
            dlg.DefaultExt = ".sol";
            dlg.AddExtension = true;
            dlg.OverwritePrompt = true;
            dlg.RestoreDirectory = true;
            dlg.ValidateNames = true;

            var result = dlg.ShowDialog();
            if (result == false) return;

            string path = dlg.FileName;
            VM.Instance.Files.Save(path);
        }
    }
}
