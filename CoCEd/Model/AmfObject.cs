using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoCEd.Model
{
    public enum AmfTypes
    {
        Undefined = 0,
        Null = 1,
        False = 2,
        True = 3,
        Integer = 4,
        Double = 5,
        String = 6,
        XmlDoc = 7,
        Date = 8,
        Array = 9,
        Object = 0xA,
        XmlMarker = 0xB,
        ByteArray = 0xC,
        VectorInt = 0xD,
        VectorUInt = 0xE,
        VectorDouble = 0xF,
        VectorGeneric = 0x10,
        Dictionary = 0x11,
    }

    public sealed class AmfNull
    {
        public static readonly AmfNull Instance = new AmfNull();

        private AmfNull()
        {
        }

        public override string ToString()
        {
            return "<Null>";
        }
    }

    public sealed class AmfTrait
    {
        public AmfTrait()
        {
        }

        public String[] Properties { get; set; }
        public bool IsExternalizable { get; set; }
        public bool IsDynamic { get; set; }
        public bool IsEnum { get; set; }
        public string Name { get; set; }
    }

    public sealed class AmfPair
    {
        public AmfPair()
        {
        }

        public AmfPair(Object name, Object value)
        {
            Key = name;
            Value = value;
        }

        public Object Key { get; set; }
        public Object Value { get; set; }
        public AmfObject ValueAsObject { get { return Value as AmfObject; } }

        public override string ToString()
        {
            return Key == null ? "<Undefined>" : Key.ToString();
        }
    }

    public class AmfObject : IEnumerable<AmfPair>
    {
        readonly List<AmfPair> _associativePairs;
        readonly List<Object> _denseValues;

        public AmfObject(AmfTypes type, int count = 0)
        {
            Type = type;
            switch (type)
            {
                case AmfTypes.Dictionary:
                    _associativePairs = new List<AmfPair>(count);
                    _denseValues = new List<object>();
                    break;

                case AmfTypes.Array:
                case AmfTypes.VectorInt:
                case AmfTypes.VectorUInt:
                case AmfTypes.VectorDouble:
                case AmfTypes.VectorGeneric:
                    _associativePairs = new List<AmfPair>();
                    _denseValues = new List<object>(count);
                    break;

                case AmfTypes.Object:
                    Trait = new AmfTrait { Name = "", IsDynamic = true, Properties = new string[0] };
                    _associativePairs = new List<AmfPair>(count);
                    _denseValues = new List<object>();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public AmfTrait Trait { get; set; }
        public AmfTypes Type { get; private set; }
        public string GenericElementType { get; set; }
        public bool IsFixedVector { get; set; }
        public bool HasWeakKeys { get; set; }

        public int DenseCount
        {
            get { return _denseValues.Count; }
        }

        public int AssociativeCount
        {
            get { return _associativePairs.Count; }
        }

        public int Count
        {
            get { return _associativePairs.Count + _denseValues.Count; }
        }

        public bool IsEnum
        {
            get { return Trait != null && Trait.IsEnum; }
        }

        public int EnumValue
        {
            get { return GetInt("value"); }
            set { this["value"] = value; }
        }

        public object this[object key]
        {
            get
            {
                int index;
                if (IsIndex(key, out index) && index >= 0 && index < _denseValues.Count) return _denseValues[index];

                var pair = GetAssociativePair(key);
                if (pair != null) return pair.Value;
                return null;
            }
            set
            {
                if (value == null)
                {
                    RemoveKey(key);
                    return;
                }

                int index;
                if (IsIndex(key, out index))
                {
                    if (index == _denseValues.Count)
                    {
                        Push(value);
                        return;
                    }
                    if (index >= 0 && index < _denseValues.Count)
                    {
                        _denseValues[index] = value;
                        return;
                    }
                }

                var pair = GetAssociativePair(key);
                if (pair != null) pair.Value = value;
                else _associativePairs.Add(new AmfPair(key, value));
            }
        }

        bool RemoveKey(object key)
        {
            int index;
            if (IsIndex(key, out index))
            {
                if (index >= 0 && index < _denseValues.Count)
                {
                    // We're going to remove 4, so we need to add 5 and higher to the associative part
                    for (int i = index + 1; i < _denseValues.Count; ++i)
                    {
                        _associativePairs.Add(new AmfPair(i, _denseValues[i]));
                    }

                    // Remove 4 and higher from the dense part
                    _denseValues.RemoveRange(index, _denseValues.Count - index);
                    return true;
                }
            }

            // Remove from associative index
            var pair = GetAssociativePair(key);
            if (pair == null) return false;

            _associativePairs.Remove(pair);
            return true;
        }

        public double GetDouble(Object key, double? defaultValue = null)
        {
            var value = this[key];
            if (value == null) return defaultValue.Value;
            if (value is string) return Double.Parse((string)value);
            if (value is double) return (double)value;
            if (value is bool) return (bool)value ? 1 : 0;
            if (value is uint) return (double)(uint)value;
            if (value is int) return (double)(int)value;
            throw new InvalidOperationException("No conversion available");
        }

        public int GetInt(Object key, int? defaultValue = null)
        {
            var value = this[key];
            if (value == null) return defaultValue.Value;
            if (value is string) return Int32.Parse((string)value);
            if (value is double) return (int)(double)value;
            if (value is bool) return (bool)value ? 1 : 0;
            if (value is uint) return (int)(uint)value;
            if (value is int) return (int)value;
            throw new InvalidOperationException("No conversion available");
        }

        public string GetString(Object key)
        {
            var value = this[key];
            if (value == null) return null;
            if (value is AmfNull) return null;
            if (value is string) return (string)value;
            if (value is AmfObject)
            {
                var obj = (AmfObject)value;
                if (obj.IsEnum) return obj.EnumValue.ToString();
            }
            return value.ToString();
        }

        public bool GetBool(Object key, bool? defaultValue = null)
        {
            var value = this[key];
            if (value == null) return defaultValue.Value;
            if (value is bool) return (bool)value;
            if (value is uint) return (uint)value == 0;
            if (value is int) return (int)value == 0;
            if (value as string == "false") return false;
            return true;
        }

        public AmfObject GetObj(Object key)
        {
            return this[key] as AmfObject;
        }

        public bool Contains(Object key)
        {
            int index;
            if (IsIndex(key, out index) && index >= 0 && index < _denseValues.Count) return true;

            return GetAssociativePair(key) != null;
        }

        public bool Pop(int index)
        {
            // Remove from dense part
            if (index >= 0 && index < _denseValues.Count)
            {
                _denseValues.RemoveAt(index);
                return true;
            }

            // Remove from associative part
            var pair = GetAssociativePair(index);
            if (pair == null) return false;
            _associativePairs.Remove(pair);

            // Shuffle following items
            while (true)
            {
                pair = GetAssociativePair(index + 1);
                if (pair == null) break;
                pair.Key = index;
                ++index;
            }
            return true;
        }

        public void Push(object value)
        {
            // Note: thanks to consistency, we know that there is no item in the associative part at index #DenseCount
            _denseValues.Add(value);
            if (_associativePairs.Count == 0) return;   // Optimization for deserialization

            // Before we had 0-4 and 6, we added 5, so we merge 6 and higher into the dense part
            while (true)
            {
                var pair = GetAssociativePair(_denseValues.Count);
                if (pair == null) return;

                _associativePairs.Remove(pair);
                _denseValues.Add(pair.Value);
            }
        }

        public void Move(int sourceIndex, int destIndex)
        {
            if (destIndex == sourceIndex) return;
            // No change on the index: the shift caused by the removal is compensated by the fact that we need to increment the index since we want to insert "after".

            if (sourceIndex < 0 || sourceIndex >= _denseValues.Count) throw new ArgumentOutOfRangeException();
            if (destIndex < 0 || destIndex >= _denseValues.Count) throw new ArgumentOutOfRangeException();

            var value = _denseValues[sourceIndex];
            _denseValues.RemoveAt(sourceIndex);
            _denseValues.Insert(destIndex, value);
        }

        public void Add(Object key, Object value)
        {
            int index;
            if (IsIndex(key, out index))
            {
                if (index == _denseValues.Count)
                {
                    _denseValues.Add(value);
                    return;
                }
                else if (index >= 0 && index < _denseValues.Count)
                {
                    throw new InvalidOperationException();
                }
            }

            if (GetAssociativePair(key) != null) throw new InvalidOperationException();
            _associativePairs.Add(new AmfPair(key, value));
        }

        public void SortDensePart(Comparison<Object> comparison)
        {
            _denseValues.Sort(comparison);
        }

        /*public dynamic D()
        {
            return this;
        }*/

        public Object[] GetDensePart()
        {
            return _denseValues.ToArray();
        }

        public IEnumerable<AmfPair> GetAssociativePart()
        {
            foreach(var pair in _associativePairs) yield return pair;
        }

        protected IEnumerable<AmfPair> Enumerate()
        {
            for (int i = 0; i < _denseValues.Count; i++) yield return new AmfPair(i, _denseValues[i]);
            foreach (var pair in _associativePairs) yield return pair;
        }

        IEnumerator<AmfPair> IEnumerable<AmfPair>.GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        bool IsIndex(object key, out int index)
        {
            if (key is int)
            {
                index = (int)key;
                return true;
            }
            if (key is string)
            {
                return Int32.TryParse((string)key, out index);
            }
            if (key == null)
            {
                index = 0;
                return false;
            }

            var str = key.ToString();
            return Int32.TryParse(str, out index);
        }

        AmfPair GetAssociativePair(object key)
        {
            int index = 0;
            foreach (var pair in _associativePairs)
            {
                if (AreSameKey(pair.Key, key)) return pair;
                ++index;
            }
            return null;
        }

        bool AreSameKey(object x, object y)
        {
            if (x == null) return (y == null);
            if (y == null) return false;
            if (Type == AmfTypes.Object) return x.ToString() == y.ToString();
            return x.Equals(y);
        }

        public static bool AreSame(object x, object y)
        {
            return object.Equals(x, y);
        }
    }

    public class AmfXmlType
    {
        public bool IsDocument { get; set; }
        public string Content { get; set; }
    }
}
