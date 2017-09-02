using System;
using System.Collections.Generic;

namespace REPEL
{
    public class GlobalEnvironment : Environment
    {
        private Symbols _symbols;

        public Symbols Names => _symbols;

        public GlobalEnvironment() : base(1 << 4, null) => _symbols = new Symbols();

        public new object GetValue(string name)
        {
            try { return Values[_symbols[name]]; }
            catch (KeyNotFoundException) { return Outer == null ? null : Outer.GetValue(name); }
        }

        public new void SetValue(string name, object value) => (GetEnvironment(name) ?? this).SetNewValue(name, value);

        public new void SetValue(int nest, int index, object value)
        {
            if (nest == 0) Assign(index, value);
            else base.SetValue(nest, index, value);
        }
        
        public new void SetNewValue(string name, object value) => Assign(_symbols.AddSymbol(name), value);

        public new Environment GetEnvironment(string name) => _symbols.Contains(name) ? this : (Outer == null ? null : Outer.GetEnvironment(name));

        private void Assign(int index, object value)
        {
            if (index >= Values.Length) Resize(Math.Max(Values.Length << 1, index + 1));
            Values[index] = value;
        }
    }
}
