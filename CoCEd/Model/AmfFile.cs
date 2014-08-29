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
    public enum SerializationFormat
    {
        Slot = 0,
        Exported = 1,
    }


    public sealed class AmfFile : AmfObject
    {
        static readonly HashSet<String> _backedUpFiles = new HashSet<string>();


        public AmfFile(string path)
            : base(AmfTypes.Array)
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
                        SerializationFormat format;
                        reader.Run(this, out name, out format);
                        Format = format;
                        Name = name;
                    }
                }
            }
#if !DEBUG
            catch (IOException e)
            {
                Error = e.ToString();
            }
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
            catch (UnauthorizedAccessException e)
            {
                Error = e.ToString();
            }
#endif
            catch (SecurityException e)
            {
                Error = e.ToString();
            }
        }

        public SerializationFormat Format
        {
            get;
            private set;
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

        public bool CanBeSaved(SerializationFormat format)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = new AmfWriter(stream))
                    {
                        writer.Run(this, "Test", format);
                        stream.Flush();
                        stream.Close();
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void Save(string path, SerializationFormat format)
        {
            // Delete existing file
            var name = Path.GetFileNameWithoutExtension(path);

            // Write it to a temporary file, then move it.
            try
            {
                var tempPath = Path.GetTempFileName();
                Write(tempPath, format, name);
                EnsureDeleted(path);
                File.Move(tempPath, path);
            }
            // If this fails (no temporary folder access?), save directly
            catch (UnauthorizedAccessException)
            {
                Write(path, format, name);
            }
            catch (SecurityException)
            {
                Write(path, format, name);
            }
            catch (IOException)
            {
                Write(path, format, name);
            }
        }

        private void Write(string path, SerializationFormat format, string name)
        {
            EnsureDeleted(path);
            using (var stream = File.Create(path))
            {
                using (var writer = new AmfWriter(stream))
                {
                    writer.Run(this, name, format);
                    stream.Flush();
                    stream.Close();
                }
            }
        }

        static void EnsureDeleted(string path)
        {
            if (File.Exists(path))
            {
                var attribs = File.GetAttributes(path) & ~FileAttributes.ReadOnly;
                File.SetAttributes(path, attribs);
                File.Delete(path);
            }
        }

#if DEBUG
        public void TestSerialization()
        {
            using (var stream = new ComparisonStream(FilePath))
            {
                using (var writer = new AmfWriter(stream))
                {
                    writer.Run(this, Name, Format);
                }
                stream.AssertSameLength();
            }
        }
#endif
    }
}
