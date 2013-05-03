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
                        reader.Run(this, out name);
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

        public void Save(string path)
        {
            EnsureBackupExists(path);

            // Delete existing file
            var name = Path.GetFileNameWithoutExtension(path);
            if (File.Exists(path))
            {
                var attribs = File.GetAttributes(path) & ~FileAttributes.ReadOnly;
                File.SetAttributes(path, attribs);   
                File.Delete(path);
            }

            // Create it
            using (var stream = File.Create(path))
            {
                using (var writer = new AmfWriter(stream))
                {
                    writer.Run(this, name);
                    stream.Flush();
                    stream.Close();
                }
            }
        }

        void EnsureBackupExists(string path)
        {
            try
            {
                // Backups are only done once per file throughout the lifetime of this process.
                var lowerPath = path.ToLowerInvariant();
                if (_backedUpFiles.Contains(lowerPath)) return;

                // Does not backup files created by us during the liftime of this process.
                if (!File.Exists(path))
                {
                    _backedUpFiles.Add(lowerPath);
                    return;
                }

                // Create backup
                var backUpPath = lowerPath.Replace(".sol", ".bak");
                File.Copy(path, backUpPath, true);
                _backedUpFiles.Add(lowerPath);
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (SecurityException)
            {
            }
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
}
