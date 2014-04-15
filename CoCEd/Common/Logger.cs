using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;

namespace CoCEd.Common
{
    public static class Logger
    {
        public static void Error(Exception e)
        {
            Error(e.ToString());
        }

        public static void Error(string msg)
        {
            try
            {
                string dataVersion = CoCEd.ViewModel.VM.Instance != null ? CoCEd.ViewModel.VM.Instance.FileVersion : "";
                if (!String.IsNullOrEmpty(dataVersion))
                {
                    dataVersion = String.Format(", CoC Data: {0}", dataVersion);
                }

                // if possible, make CoCEd's and CoC's versions an integral part of the exception message,
                // so we don't have to rely on users' claims of being up to date anymore
                msg = String.Format("[{0}: {1}{2}]\n{3}",
                    Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    dataVersion,
                    msg);

                if (File.Exists("CoCEd.log")) File.Delete("CoCEd.log");
                File.WriteAllText("CoCEd.log", msg);
            }
            catch(IOException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (NotSupportedException)
            {
            }
        }
    }
}
