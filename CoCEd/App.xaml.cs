using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using CoCEd.Common;
using CoCEd.Model;
using CoCEd.View;
using CoCEd.ViewModel;

namespace CoCEd
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if !DEBUG
            DispatcherUnhandledException += OnDispatcherUnhandledException;
#endif
            Initialize();
        }

        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            DispatcherUnhandledException -= OnDispatcherUnhandledException;

            try
            {
                ExceptionBox box = new ExceptionBox();
                box.Title = "Unexpected error";
                box.Message = "An error occured and the application is going to exit.\nThe error below will be saved as CoCEd.log. Please report it on CoC's forums.";
                box.ExceptionMessage = e.Exception.ToString();
                box.ShowDialog(ExceptionBoxButtons.Quit);
            }
            catch(Exception e2)
            {
                MessageBox.Show(e2.ToString(), "Error in error box ?!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Logger.Error(e.Exception);
            Shutdown();
        }

        void Initialize()
        {
            ExceptionBox box;
            var xmlResult = XmlData.LoadXml();
            switch (xmlResult)
            {
                case XmlLoadingResult.Success:
                    break;

                case XmlLoadingResult.MissingFile:
                    box = new ExceptionBox();
                    box.Title = "Fatal error";
                    box.Message = "The CoCEd.xml file could not be found. Did you try to run the program from the archive without extracting all the files first?";
                    box.Path = Environment.CurrentDirectory;
                    box.ShowDialog(ExceptionBoxButtons.Quit);
                    Shutdown();
                    return;

                case XmlLoadingResult.NoPermission:
                    box = new ExceptionBox();
                    box.Title = "Fatal error";
                    box.Message = "The CoCEd.xml file was already in use or this application does not have the permission to read from the folder where it is located.";
                    box.Path = Environment.CurrentDirectory;
                    box.ShowDialog(ExceptionBoxButtons.Quit);
                    Shutdown();
                    return;

                default:
                    throw new NotImplementedException();
            }

            VM.Create();

            FileManager.BuildPaths();
            var directories = FileManager.GetDirectories().ToArray();   // Load all on startup to check for errors
            var result = ExceptionBoxResult.Continue;
            if (FileManager.MissingPermissionPath != null)
            {
                box = new ExceptionBox();
                box.Title = "Could not scan some folders.";
                box.Message = "CoCEd did not get the permission to read a folder or a file.\nSome files won't be displayed in the open/save menus.";
                box.Path = FileManager.MissingPermissionPath;
                box.IsWarning = true;
                result = box.ShowDialog(ExceptionBoxButtons.Quit, ExceptionBoxButtons.Continue);
            }
            if (result == ExceptionBoxResult.Quit)
            {
                Shutdown();
                return;
            }

#if DEBUG
            var file = AutoLoad(directories);
            //new AmfFile("e:\\plainObject.sol").TestSerialization();
            //new AmfFile("e:\\unicode.sol").TestSerialization();
            //DebugStatuses(file);
            //RunSerializationTest(set);
            //ParsePerks();
            //ImportStatuses();
            //ImportFlags();
#endif
        }


#if DEBUG
        static AmfFile AutoLoad(FlashDirectory[] directories)
        {
            var file = directories[0].Files[0];
            VM.Instance.Load(file.FilePath);
            return file;
        }

        static void PrintStatuses(AmfFile file)
        {
            foreach (AmfPair pair in file.GetObj("statusAffects"))
            {
                int key = Int32.Parse(pair.Key as string);
                var name = pair.ValueAsObject.GetString("statusAffectName");
                Debug.WriteLine(key.ToString("000") + " - " + name);
            }
        }

        static void RunSerializationTest(FlashDirectory[] directories)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            foreach (var first in directories[0].Files)
            {
                var outPath = "e:\\" + Path.GetFileName(first.FilePath);
                first.TestSerialization();
                first.Save(outPath, first.Format);

                var input = File.ReadAllBytes(first.FilePath);
                var output = File.ReadAllBytes(outPath);
                if (input.Length != output.Length) throw new InvalidOperationException();
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] != output[i]) throw new InvalidOperationException();
                }
            }
            var elapsed = s.ElapsedMilliseconds;
            MessageBox.Show("Success!");
        }
#endif
    }
}
