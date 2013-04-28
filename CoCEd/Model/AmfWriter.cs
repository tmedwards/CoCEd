using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoCEd.Model
{
    public sealed class AmfWriter : IDisposable
    {
        readonly BinaryWriter _writer;
        readonly Dictionary<String, int> _stringLookup = new Dictionary<String, int>();
        readonly Dictionary<AmfTrait, int> _traitLookup = new Dictionary<AmfTrait, int>();
        readonly Dictionary<AmfObject, int> _objectLookup = new Dictionary<AmfObject, int>();

        public AmfWriter(Stream stream)
        {
            _writer = new BinaryWriter(stream);
        }

        public void Run(AmfFile file, string newName)
        {
            // Endianness
            _writer.Write((byte)0x00);  
            _writer.Write((byte)0xBF);

            // Placeholder for size
            _writer.Write((int)0);      

            // Magic signature
            _writer.Write('T');
            _writer.Write('C');
            _writer.Write('S');
            _writer.Write('O');
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x04);
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x00);

            // Name
            var countBytes = BitConverter.GetBytes((UInt16)newName.Length);
            WriteBytesAfterSwap(countBytes, i => 1 - i);
            _writer.Write(newName.ToArray());

            // AMF version number
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x03);

            // Key-value pairs
            foreach(var pair in file)
            {
                WriteString(pair.Key);
                WriteValue(pair.Value);
                _writer.Write((byte)0);
            }

            // Replace size
            int dataSize = (int)_writer.BaseStream.Length - 6;
            var sizeBytes = BitConverter.GetBytes(dataSize);
            _writer.BaseStream.Seek(2, SeekOrigin.Begin);
            WriteBytesAfterSwap(sizeBytes, i => 3 - i);

            // Flush
            _writer.Flush();
        }

        void WriteValue(Object obj)
        {
            if (obj == null)
            {
                _writer.Write((byte)AmfTypes.Null);
            }
            else if (obj is Boolean)
            {
                if ((bool)obj) _writer.Write((byte)AmfTypes.True);
                else _writer.Write((byte)AmfTypes.False);
            }
            else if (obj is Undefined)
            {
                _writer.Write((byte)AmfTypes.Undefined);
            }
            else if (obj is Int32)
            {
                _writer.Write((byte)AmfTypes.Integer);
                WriteI29((int)obj);
            }
            else if (obj is Double)
            {
                _writer.Write((byte)AmfTypes.Double);
                WriteDouble((double)obj);
            }
            else if (obj is String)
            {
                _writer.Write((byte)AmfTypes.String);
                WriteString((string)obj);
            }
            else if (obj is AmfArray)
            {
                _writer.Write((byte)AmfTypes.Array);
                WriteArray((AmfArray)obj);
            }
            else if (obj is AmfObject)
            {
                _writer.Write((byte)AmfTypes.Object);
                WriteObject((AmfObject)obj);
            }
            else if (obj is byte[])
            {
                _writer.Write((byte)AmfTypes.ByteArray);
                WriteByteArray((byte[])obj);
            }
            else if (obj is DateTime)
            {
                _writer.Write((byte)AmfTypes.Date);
                WriteDate((DateTime)obj);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        void WriteBytesAfterSwap(byte[] srcBytes, Func<int, int> indexTransform)
        {
            byte[] destBytes = new byte[srcBytes.Length];
            for (int i = 0; i < srcBytes.Length; i++)
            {
                destBytes[indexTransform(i)] = srcBytes[i];
            }
            _writer.Write(destBytes);
        }

        void WriteI29(int value)
        {
            const int upperExclusiveBound = 1 << 29;
            if (value < 0) WriteU29(value + upperExclusiveBound);
            else WriteU29(value);
        }

        void WriteU29(int value, bool lowBitFlag)
        {
            value <<= 1;
            if (lowBitFlag) value |= 1;
            WriteU29(value);
        }

        void WriteU29(int value)
        {
            // Two combinations possible
            // 7-7-7-8 (22-15-8-0)
            // 0-7-7-7 (   14-7-0)

            bool fourBytes = (value >> 21) != 0;
            int shift = fourBytes ? 22 : 14;
            int numBytes = 0;
            while (shift >= 0)
            {
                int mask = (numBytes == 3 ? 0xFF : 0x7F);
                byte b = (byte)((value >> shift) & mask);

                if (shift == 8) shift = 0;
                else shift -= 7;

                if (b == 0 && numBytes == 0 && shift >= 0) continue;
                ++numBytes;

                if (shift >= 0) b |= 0x80;
                _writer.Write(b);
            }
        }

        void WriteDouble(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            WriteBytesAfterSwap(bytes, i => 7 - i);
        }

        void WriteString(string value)
        {
            int index;
            if (value == "")
            {
                WriteU29(0, true);
            }
            else if (_stringLookup.TryGetValue(value, out index))
            {
                WriteU29(index, false);
            }
            else
            {
                WriteU29(value.Length, true);
                _writer.Write(value.ToArray());
                _stringLookup.Add(value, _stringLookup.Count);
            }
        }

        void WriteArray(AmfArray obj)
        {
            // TODO: Optimize this : o(n²) to o(n)
            int countDense = obj.DenseCount;
            WriteU29(countDense, true);

            // Associative part (key-value pairs)
            foreach(var pair in obj)
            {
                int index;
                if (Int32.TryParse(pair.Key, out index) && index < countDense) continue;

                WriteString(pair.Key);
                WriteValue(pair.Value);
            }
            WriteString("");

            // Dense part (consecutive indices >=0 and <count)
            for (int i = 0; i < countDense; i++)
            {
                var value = obj[i.ToString()];
                WriteValue(value);
            }
        }

        void WriteObject(AmfObject obj)
        {
            // By reference or by instance?
            int index;
            if (_objectLookup.TryGetValue(obj, out index))
            {
                WriteU29(index, false);
                return;
            }
            _objectLookup.Add(obj, _objectLookup.Count);

            // Trait
            WriteTrait(obj.Trait);

            // Trait's properties
            foreach (var name in obj.Trait.Properties)
            {
                var value = obj[name];
                WriteValue(value);
            }

            // Dynamic properties
            if (obj.Trait.IsDynamic)
            {
                foreach(var pair in obj)
                {
                    // Is prop from trait or dynamic?
                    if (obj.Trait.Properties.Contains(pair.Key)) continue;

                    WriteString(pair.Key);
                    WriteValue(pair.Value);
                }
                WriteString("");
            }
        }

        void WriteTrait(AmfTrait trait)
        {
            // By reference or by instance?
            int index;
            if (_traitLookup.TryGetValue(trait, out index))
            {
                WriteU29((index << 2) | 1);
                return;
            }
            _traitLookup.Add(trait, _traitLookup.Count);

            // Index and flags
            index = 3;                                      // 0b011. From left to right: trait not externalizable, trait by instance, obj by instance
            if (trait.IsDynamic) index |= 8;                // 0b1000
            index |= (trait.Properties.Length << 4);        
            WriteU29(index);

            // Name and properties
            WriteString(trait.Name);
            foreach(var name in trait.Properties)
            {
                WriteString(name);
            }
        }

        void WriteByteArray(byte[] value)
        {
            WriteU29(value.Length, true);
            _writer.Write(value);
        }

        void WriteDate(DateTime date)
        {
            WriteU29(0, true);

            var elapsed = date - new DateTime(1970, 1, 1);
            WriteDouble(elapsed.TotalMilliseconds);
        }

        void IDisposable.Dispose()
        {
            _writer.Dispose();
        }
    }
}
