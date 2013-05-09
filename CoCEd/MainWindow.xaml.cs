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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoCEd.Common;
using CoCEd.Model;
using CoCEd.View;
using CoCEd.ViewModel;

namespace CoCEd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ((FrameworkElement)Content).QueryContinueDrag += OnQueryContinueDrag;
        }

        public NamedVector4Popup ValuesPopup
        {
            get { return valuesPopup; }
        }

        void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if ((e.KeyStates & DragDropKeyStates.LeftMouseButton) != DragDropKeyStates.LeftMouseButton)
            {
                e.Action = DragAction.Cancel;
            }
        }


#if !DEBUG
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!VM.Instance.SaveRequired) return;

            var result = ConfirmationBox.Show();
            switch (result)
            {
                case ConfirmationResult.Cancel:
                    e.Cancel = true;
                    break;

                case ConfirmationResult.Quit:
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
#endif             

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            App.Current.Shutdown();
        }
    }
}
