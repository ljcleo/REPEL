using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class Symbols
    {
        private Dictionary<string, int> _table = new Dictionary<string, int>();
        private Symbols _outer;

        public int this[string name] => _table[name];

        public int Count => _table.Count;

        public Symbols Outer => _outer;

        public Symbols() : this(null) { }

        public Symbols(Symbols outer) =>_outer = outer;

        public bool Contains(string name) => _table.ContainsKey(name);

        public Location GetLocation(int type, string name) => type == 0 ? GetLocation(name) : type == 1 ? GetGlobalLocation(name) : type == 2 ? GetLocalLocation(name) : throw new InternalException("bad variable type");

        public int AddSymbol(string name) => Contains(name) ? Insert(name) : this[name];

        public Location SetSymbol(int type, string name) => type == 0 ? SetSymbol(name) : type == 1 ? SetGlobalSymbol(name) : SetLocalSymbol(name);

        private Location GetLocation(string name) => GetLocation(name, 0);

        private Location GetLocation(string name, int nest) => Contains(name) ? new Location(nest, this[name]) : (Outer == null ? null : Outer.GetLocation(name, nest + 1));

        private Location GetGlobalLocation(string name) => GetGlobalLocation(name, 0);

        private Location GetGlobalLocation(string name, int nest) => Outer == null ? (Contains(name) ? new Location(nest, this[name]) : null) : Outer.GetGlobalLocation(name, nest + 1);

        private Location GetLocalLocation(string name) => Contains(name) ? new Location(0, this[name]) : null;

        private Location SetSymbol(string name) => GetLocation(name) ?? new Location(0, Insert(name));

        private Location SetGlobalSymbol(string name) => Outer == null ? SetSymbol(name) : Outer.SetGlobalSymbol(name);

        private Location SetLocalSymbol(string name) => GetLocalLocation(name) ?? new Location(0, Insert(name));

        private int Insert(string name)
        {
            int p = Count;
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
