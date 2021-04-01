
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Semver;
using CoCEd.Common;
namespace CoCEd.View
{
    public partial class CheckForUpdateBox : Window
    {
        enum UpdateCheckResult
        {
            No,
            Yes,
            Unknown,
        }
        static readonly string ReleasesPageUrl = $"http://github.com/{Settings.Default.CoCEdRepo}/releases/latest";
        static readonly SemVersion CurrentVersion = SemVersion.Parse(((string)Assembly.GetExecutingAssembly()
                                                                                      .GetType("GitVersionInformation")
                                                                                      .GetField("FullSemVer")
                                                                                      .GetValue(null))
                                                                                      .TrimStart('v'));
        //Task _updateCheckTask;

        public CheckForUpdateBox()
        {
            InitializeComponent();
            Owner = App.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            checkingGrid.Visibility = Visibility.Visible;
            statusGrid.Visibility = Visibility.Collapsed;
        }

        void CheckForUpdateBox_Loaded(object sender, RoutedEventArgs e)
        {
            //_updateCheckTask = Task.Factory.StartNew(new Action(() =>
            Task.Factory.StartNew(new Action(() =>
            {
                // check for an update
                var status = CheckForUpdate();

                // update the UI with the results
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateStatus(status);
                }), DispatcherPriority.Input);
            }));
        }

        void close_Click(object sender, RoutedEventArgs e)
        {
            //if (_updateCheckTask != null) _updateCheckTask.Dispose();
            Close();
        }

        void requestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        }

        void UpdateStatus(UpdateCheckResult status)
        {
            switch (status)
            {
                // nothing to do for UpdateCheckResult.Yes, the correct hyperlinked text is in the XAML as the default
                case UpdateCheckResult.No:
                    statusText.Text = "CoCEd is up to date.";
                    break;
                case UpdateCheckResult.Unknown:
                    statusText.Text = "Check failed. An unexpected problem occurred.";
                    break;
            }
            checkingGrid.Visibility = Visibility.Collapsed;
            statusGrid.Visibility = Visibility.Visible;
        }
        UpdateCheckResult CheckForUpdate()
        {
            try {
                var ver_string = Task.Run(GitHubGetters.GetLatestCocEdVersion).Result;
                var availableVersion = SemVersion.Parse(ver_string);
                if (availableVersion > CurrentVersion)
                {
                    return UpdateCheckResult.Yes;
                }
                return UpdateCheckResult.No;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return UpdateCheckResult.Unknown;
        }
    }
}
