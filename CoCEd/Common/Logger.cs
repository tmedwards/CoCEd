using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
