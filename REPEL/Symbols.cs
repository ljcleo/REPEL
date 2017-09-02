using System.Collections.Generic;

namespace REPEL
{
    public class Symbols
    {
        private Dictionary<string, int> _table = new Dictionary<string, int>();
        private Symbols _outer;

        public int this[string name] => _table[name];

        public Symbols() : this(null) { }

        public Symbols(Symbols outer) =>_outer = outer;

        public bool Contains(string name) => _table.ContainsKey(name);

        public Location GetLocation(string name) => GetLocation(name, 0);

        public Location GetLocation(string name, int nest) => _table.ContainsKey(name) ? new Location(nest, _table[name]) : (_outer == null ? null : _outer.GetLocation(name, nest + 1));

        public int AddSymbol(string name) => _table.ContainsKey(name) ? Insert(name) : _table[name];

        public Location SetSymbol(string name) => GetLocation(name) ?? new Location(0, Insert(name));

        private int Insert(string name)
        {
            int p = _table.Count;
            return _table[name] = p;
        }

        public class Location
        {
            public int Nest { get; protected set; }

            public int Index { get; protected set; }

            public Location(int nest, int index)
            {
                Nest = nest;
                Index = index;
            }
        }
    }
}
