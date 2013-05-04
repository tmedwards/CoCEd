using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoCEd.View
{
    public enum ExceptionBoxResult
    {
        Continue,
        Cancel,
        Quit,
    }

    public enum ExceptionBoxButtons
    {
        Continue,
        Cancel,
        Quit,
    }

    public partial class ExceptionBox : Window
    {
        ExceptionBoxResult _result;

        public ExceptionBox()
        {
            InitializeComponent();
        }

        public bool IsWarning { get; set; }
        public string ExceptionMessage { get; set; }
        public string Message { get; set; }
        public string Path { get; set; }

        public static new ExceptionBoxResult Show()
        {
            var box = new ExceptionBox();
            return box.ShowDialog();
        }

        public new ExceptionBoxResult ShowDialog(params ExceptionBoxButtons[] buttons)
        {
            // http://forum.fenoxo.com/thread-6324.html
            if (App.Current.MainWindow != this)
            {
                Owner = App.Current.MainWindow;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            text.Text = Message;

            if (IsWarning) image.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Error.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            else image.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Error.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            if (String.IsNullOrEmpty(Path) && String.IsNullOrEmpty(Path)) folderGrid.Visibility = Visibility.Collapsed;
            else if (!String.IsNullOrEmpty(Path)) folderText.Text = Path;
            else if (!String.IsNullOrEmpty(Path)) folderText.Text = Path;

            if (String.IsNullOrEmpty(ExceptionMessage)) exceptionGrid.Visibility = Visibility.Collapsed;
            else exceptionText.Text = ExceptionMessage;

            Button lastButton = null;
            foreach (var choice in buttons)
            {
                lastButton = new Button();
                lastButton.Content = choice.ToString();
                switch (choice)
                {
                    case ExceptionBoxButtons.Quit:
                        lastButton.Click += quit_Click;
                        _result = ExceptionBoxResult.Quit;
                        break;

                    case ExceptionBoxButtons.Cancel:
                        lastButton.Click += cancel_Click;
                        _result = ExceptionBoxResult.Cancel;
                        break;

                    case ExceptionBoxButtons.Continue:
                        lastButton.Click += continue_Click;
                        _result = ExceptionBoxResult.Continue;
                        break;

                    default:
                        throw new NotImplementedException();
                }
                buttonStack.Children.Add(lastButton);
            }
            lastButton.IsDefault = true;

            base.ShowDialog();
            return _result;
        }

        void openFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = Path;
            if (String.IsNullOrEmpty(folderPath)) folderPath = System.IO.Path.GetDirectoryName(Path);
            while (!Directory.Exists(folderPath)) folderPath = System.IO.Path.GetDirectoryName(folderPath);
            Process.Start(folderPath);
        }

        void copyException_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, ExceptionMessage);
        }

        void continue_Click(object sender, RoutedEventArgs e)
        {
            _result = ExceptionBoxResult.Continue;
            Close();
        }

        void cancel_Click(object sender, RoutedEventArgs e)
        {
            _result = ExceptionBoxResult.Cancel;
            Close();
        }

        void quit_Click(object sender, RoutedEventArgs e)
        {
            _result = ExceptionBoxResult.Quit;
            Close();
        }

        void requestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        }
    }
}
