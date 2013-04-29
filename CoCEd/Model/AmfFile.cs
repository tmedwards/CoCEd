using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoCEd.Model
{
    public sealed class AmfFile : AmfNode
    {
        public AmfFile(AmfFile clone)
            : base(clone)
        {
            FilePath = clone.FilePath;
            Error = clone.Error;
            Date = clone.Date;
            Name = clone.Name;
        }

        public AmfFile(string path)
        {
            FilePath = path;
            Date = File.GetLastWriteTime(path);
            try
            {
                using (var stream = File.OpenRead(path))
                {
                    using (var reader = new AmfReader(stream))
                    {
                        string name;
                        reader.Run(this, out name);
                        Name = name;
                    }
                }
                throw new NotImplementedException();
            }
            catch (IOException e)
            {
                Error = e.ToString();
            }
#if !DEBUG
            catch (InvalidOperationException e)
            {
                Error = e.ToString();
            }
            catch (ArgumentException e)
            {
                Error = e.ToString();
            }
            catch (NotImplementedException e)
            {
                Error = e.ToString();
            }
#endif
        }

        public string FilePath
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Error
        {
            get;
            private set;
        }

        public DateTime Date
        {
            get;
            private set;
        }

        public AmfFile Save(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
            }

            using (var stream = File.OpenWrite(path))
            {
                using (var writer = new AmfWriter(stream))
                {
                    writer.Run(this, name);
                    stream.Flush();
                    stream.Close();
                }
            }

            var clone = new AmfFile(this);
            clone.FilePath = path;
            clone.Name = name;
            return clone;
        }

#if DEBUG
        public void Test()
        {
            using (var stream = new ComparisonStream(FilePath))
            {
                using (var writer = new AmfWriter(stream))
                {
                    writer.Run(this, Name);
                }
                stream.AssertSameLength();
            }
        }
#endif
    }

    public class AmfScanResult
    {
        public String MoreThanOneFolderPath { get; set; }
        public String MissingPermissionPath { get; set; }
    }
}
