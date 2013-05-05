using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace CoCEd.View
{
    /// <summary>
    /// Interaction logic for RawPage.xaml
    /// </summary>
    public partial class RawPage : UserControl
    {
        public RawPage()
        {
            InitializeComponent();
        }

        void flagTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:

                    ((UIElement)Keyboard.FocusedElement).MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                    e.Handled = true;
                    break;

                case Key.Down:
                    ((UIElement)Keyboard.FocusedElement).MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    e.Handled = true;
                    break;
            }
        }
    }
}
