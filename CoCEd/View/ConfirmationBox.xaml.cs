using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace CoCEd.View
{
    public enum ConfirmationResult
    {
        Quit,
        Save,
        Cancel,
    }

    public partial class ConfirmationBox : Window
    {
        ConfirmationResult _result;

        public ConfirmationBox()
        {
            InitializeComponent();
            _result = ConfirmationResult.Cancel;
        }

        public static new ConfirmationResult Show()
        {
            var box = new ConfirmationBox();
            box.ShowDialog();
            return box._result;
        }

        void save_Click(object sender, RoutedEventArgs e)
        {
            _result = ConfirmationResult.Save;
            Close();
        }

        void close_Click(object sender, RoutedEventArgs e)
        {
            _result = ConfirmationResult.Quit;
            Close();
        }

        void cancel_Click(object sender, RoutedEventArgs e)
        {
            _result = ConfirmationResult.Cancel;
            Close();
        }
    }
}
