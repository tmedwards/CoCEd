using System;
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
        VectorObject = 0x10,
        Dictionary = 0x11,
    }

    public sealed class Undefined
    {
        public static readonly Undefined Instance = new Undefined();

        private Undefined()
        {
        }
    }

    public sealed class AmfPair
    {
        public String Key { get; set; }
        public dynamic Value { get; set; }

        public AmfPair()
        {
        }

        public AmfPair(string name, object value)
        {
            Key = name;
            Value = value;
        }

        public AmfPair(AmfPair clone)
        {
            Key = clone.Key;
            Value = clone.Value;
        }

        public override string ToString()
        {
            return Key;
        }
    }

    public abstract class AmfNode : IEnumerable<AmfPair>
    {
        protected readonly List<AmfPair> _pairs = new List<AmfPair>();

        public AmfNode()
        {
        }

        public AmfNode(AmfNode clone)
        {
            foreach (var node in clone._pairs) _pairs.Add(new AmfPair(node));
        }

        public int Count
        {
            get { return _pairs.Count; }
        }

        public int DenseCount
        {
            get
            {
                bool[] flags = new bool[Count];
                foreach (var pair in _pairs)
                {
                    int index;
                    if (Int32.TryParse(pair.Key, out index)) flags[index] = true;
                }
                for (int i = 0; i < flags.Length; i++)
                {
                    if (!flags[i]) return i;
                }
                return flags.Length;
            }
        }

        public dynamic this[int key]
        {
            get { return this[key.ToString()]; }
            set { this[key.ToString()] = value; }
        }

        public dynamic this[string key]
        {
            get
            {
                var pair = _pairs.FirstOrDefault(x => x.Key == key);
                if (pair == null) return Undefined.Instance;
                return pair.Value;
            }
            set
            {
                var pair = _pairs.Where(x => x.Key == key).FirstOrDefault();
                if (pair == null) _pairs.Add(new AmfPair(key, value));
                else pair.Value = value;
            }
        }

        public bool Contains(string key)
        {
            return _pairs.Any(x => x.Key == key);
        }

        public void Add(AmfNode node)
        {
            int count = DenseCount;
            Add(count.ToString(), node);
        }

        public void Add(string key, object value)
        {
            if (_pairs.Any(x => x.Key == key)) throw new ArgumentException();
            _pairs.Add(new AmfPair(key, value));
        }

        public bool Remove(string key, bool decrementIndicesGreaterThanKey, out object removedValue)
        {
            removedValue = Undefined.Instance;

            // Remove the item
            bool removed = false;
            for(int index = 0; index < _pairs.Count; index++)
            {
                if (_pairs[index].Key != key) continue;

                // Remove it
                removedValue = _pairs[index].Value;
                _pairs.RemoveAt(index);
                removed = true;
                break;
            }

            int keyIndex;
            if (!removed) return false;
            if (!decrementIndicesGreaterThanKey) return true;
            if (!Int32.TryParse(key, out keyIndex)) return true;

            // Decrements indices greater than the key
            foreach(var pair in _pairs)
            {
                int pairIndex;
                if (Int32.TryParse(pair.Key, out pairIndex) && pairIndex > keyIndex)
                {
                    --pairIndex;
                    pair.Key = pairIndex.ToString();
                }
            }
            return true;
        }

        public void Insert(object value, int index)
        {
            int insertionIndex = _pairs.Count;

            // Increment indices greater than the key and get the best insertion
            int i = 0;
            int pairIndex;
            foreach (var pair in _pairs)
            {
                if (Int32.TryParse(pair.Key, out pairIndex) && pairIndex >= index)
                {
                    if (pairIndex == index) insertionIndex = i;
                    ++pairIndex;
                    pair.Key = pairIndex.ToString();
                }
                ++i;
            }

            _pairs.Insert(insertionIndex, new AmfPair(index.ToString(), value));
        }

        IEnumerator<AmfPair> IEnumerable<AmfPair>.GetEnumerator()
        {
            return _pairs.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _pairs.GetEnumerator();
        }
    }

    public sealed class AmfArray : AmfNode
    {
    }

    public sealed class AmfObject : AmfNode
    {
        public AmfTrait Trait { get; set; }
    }

    public sealed class AmfTrait
    {
        public AmfTrait()
        {
        }

        public String[] Properties { get; set; }
        public bool IsDynamic { get; set; }
        public string Name { get; set; }
    }
}
