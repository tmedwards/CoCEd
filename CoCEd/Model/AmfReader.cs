using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoCEd.Model
{
    internal sealed class AmfReader : IDisposable
    {
        readonly BinaryReader _reader;
        readonly List<String> _stringLookup = new List<String>();
        readonly List<AmfTrait> _traitLookup = new List<AmfTrait>();
        readonly List<AmfObject> _objectLookup = new List<AmfObject>();

        public AmfReader(Stream stream)
        {
            _reader = new BinaryReader(stream);
        }

        public void Run(AmfFile file, out string name)
        {
            // Endianness
            if (_reader.ReadByte() != 0) throw new NotImplementedException("Unknown endianness");
            if (_reader.ReadByte() != 0xBF) throw new NotImplementedException("Unknown endianness");

            // Size
            int size = (int)ReadU32();
            if (size + 6 != _reader.BaseStream.Length) throw new InvalidOperationException("Wrong file size");

            // Magic signature
            if (ReadString(4) != "TCSO") throw new InvalidOperationException();
            _reader.BaseStream.Seek(6, SeekOrigin.Current);

            // Read name
            size = ReadU16();
            name = ReadString(size);

            // Version
            int version = (int)ReadU32();
            if (version < 3) throw new NotImplementedException("Wrong version");

            // Read content
            while (true)
            {
                var key = ReadString();
                var value = ReadValue();
                file.AddNoCheck(key, value);

                if (_reader.ReadByte() != 0) throw new InvalidOperationException();
                if (_reader.BaseStream.Position == _reader.BaseStream.Length) break;
            }
        }

        Object ReadValue()
        {
            var type = (AmfTypes)_reader.ReadByte();
            switch (type)
            {
                case AmfTypes.Undefined:
                    return Undefined.Instance;

                case AmfTypes.Null:
                    return null;

                case AmfTypes.True:
                    return true;

                case AmfTypes.False:
                    return false;

                case AmfTypes.Integer:
                    return ReadI29();

                case AmfTypes.Double:
                    return ReadDouble();

                case AmfTypes.String:
                    return ReadString();

                case AmfTypes.Array:
                    return ReadArray();

                case AmfTypes.Object:
                    return ReadObject();

                case AmfTypes.ByteArray:
                    return ReadByteArray();

                case AmfTypes.Date:
                    return ReadDate();

                default:
                    throw new NotImplementedException();
            }
        }

        byte[] ReadBytesAndSwap(int count, Func<int, int> indexTransform)
        {
            byte[] srcBytes = _reader.ReadBytes(count);
            byte[] destBytes = new byte[count];
            for (int i = 0; i < srcBytes.Length; i++)
            {
                destBytes[indexTransform(i)] = srcBytes[i];
            }
            return destBytes;
        }

        double ReadDouble()
        {
            var bytes = ReadBytesAndSwap(8, i => 7 - i);
            return BitConverter.ToDouble(bytes, 0);
        }

        string ReadString(int numChars)
        {
            var chars = _reader.ReadChars(numChars);
            return new string(chars);
        }

        string ReadString()
        {
            bool isValue;
            var lengthOrIndex = ReadU29(out isValue);
            if (!isValue) return _stringLookup[lengthOrIndex];
            if (lengthOrIndex == 0) return "";

            var str = ReadString(lengthOrIndex);
            _stringLookup.Add(str);
            return str;
        }

        ushort ReadU16()
        {
            var bytes = ReadBytesAndSwap(2, i => 1 - i);
            return BitConverter.ToUInt16(bytes, 0);
        }

        uint ReadU32()
        {
            var bytes = ReadBytesAndSwap(4, i => 3 - i);
            return BitConverter.ToUInt32(bytes, 0);
        }

        int ReadI29()
        {
            int result = ReadU29();
            int maxPositiveInclusive = (1 << 28) - 1;
            if (result <= maxPositiveInclusive) return result;

            const int upperExclusiveBound = 1 << 29;
            return result - upperExclusiveBound;
        }

        int ReadU29(out bool lowBitFlag)
        {
            int result = ReadU29();
            lowBitFlag = (result & 1) == 1;
            return result >> 1;
        }

        int ReadU29()
        {
            int numBytes = 0;
            int result = 0;
            while (true)
            {
                byte b = _reader.ReadByte();
                if (numBytes == 3) return (result << 8) | b;

                result = (result << 7) | (b & 0x7F);
                if ((b & 0x7F) == b) return result;

                ++numBytes;
            }
        }

        AmfArray ReadArray()
        {
            var result = new AmfArray();
            bool isInstance;
            var count = ReadU29(out isInstance);
            if (!isInstance) throw new NotImplementedException("Violation of AMF3 spec. Array by reference in v4?");

            // Associative part (key-value pairs)
            while (true)
            {
                var key = ReadString();
                if (key == "") break;

                var value = ReadValue();
                result.AddNoCheck(key, value);
            }

            // Dense part (consecutive indices >=0 and <count)
            for (int i = 0; i < count; i++)
            {
                var value = ReadValue();
                result.AddNoCheck(i.ToString(), value);
            }

            return result;
        }

        AmfObject ReadObject()
        {
            bool isInstance;
            int refIndex = ReadU29(out isInstance);
            if (!isInstance) return _objectLookup[refIndex];

            var result = new AmfObject();
            _objectLookup.Add(result);

            result.Trait = ReadTrait(refIndex);

            foreach (var name in result.Trait.Properties)
            {
                var value = ReadValue();
                result.AddNoCheck(name, value);
            }

            if (result.Trait.IsDynamic)
            {
                while(true)
                {
                    var name = ReadString();
                    if (name == "") break;

                    var value = ReadValue();
                    result.AddNoCheck(name, value);
                }
            } 

            return result;
        }

        AmfTrait ReadTrait(int refIndex)
        {
            bool isInstance = PopFlag(ref refIndex);
            if (!isInstance) return _traitLookup[refIndex];

            bool isExternalizable = PopFlag(ref refIndex);
            if (isExternalizable) throw new NotImplementedException("Unsupported externalized traits");

            var result = new AmfTrait();
            _traitLookup.Add(result);

            result.IsDynamic = PopFlag(ref refIndex);
            result.Name = ReadString();

            result.Properties = new string[refIndex];
            for (var i = 0; i < result.Properties.Length; i++)
            {
                result.Properties[i] = ReadString();
            }

            return result;
        }

        static bool PopFlag(ref int value)
        {
            bool result = (value & 1) == 1;
            value >>= 1;
            return result;
        }

        byte[] ReadByteArray()
        {
            bool mustBeOne;
            int length = ReadU29(out mustBeOne);
            if (!mustBeOne) throw new NotImplementedException("Violation of the AMF spec: did Adobe introduced byte array by ref?");

            var result = _reader.ReadBytes(length);
            return result;
        }

        DateTime ReadDate()
        {
            bool mustBeOne;
            ReadU29(out mustBeOne);
            if (!mustBeOne) throw new NotImplementedException("Violation of the AMF spec: did Adobe introduced date by ref?");

            var elapsed = ReadDouble();
            var result = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(elapsed);
            return result;
        }

        void IDisposable.Dispose()
        {
            _reader.Dispose();
        }
    }
}
