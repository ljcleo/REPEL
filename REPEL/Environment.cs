namespace REPEL
{
    public class Environment
    {
        private object[] _values;
        private Environment _outer;

        protected object[] Values => _values;

        protected Environment Outer => _outer;

        public Environment(int size, Environment outer)
        {
            _values = new object[size];
            _outer = outer;
        }

        public object GetValue(int nest, int index) => nest == 0 ? _values[index] : (_outer?.GetValue(nest - 1, index));

        public object GetValue(string name) => throw new InternalException("cannot access by name");

        public void SetValue(int nest, int index, object value)
        {
            if (nest == 0) _values[index] = value;
            else if (_outer == null) throw new InternalException("no outer environment");
            else _outer.SetValue(nest - 1, index, value);
        }

        public object SetValue(string name, object value) => throw new InternalException("cannot access by name");

        public object SetNewValue(string name, object value) => throw new InternalException("cannot access by name");

        public Environment GetEnvironment(string name) => throw new InternalException("cannot access by name");

        protected void Resize(int length)
        {
            object[] temp = new object[length];
            _values.CopyTo(temp, 0);
            _values = temp;
        }
    }
}