namespace REPEL
{
    public class NameNode : ASTLeaf
    {
        private int _nest, _index = -1;

        public string Name => Token.Text;

        public bool IsAnonymous => Name == "_";

        public NameNode(Token token) : base(token) { }

        public override void Lookup(Symbols sym)
        {
            var location = sym.GetLocation(Name);
            if (location == null) throw new InterpretException("undefined name: " + Name, this);
            else
            {
                _nest = location.Nest;
                _index = location.Index;
            }
        }

        public void AssignLookup(Symbols sym)
        {
            var location = sym.SetSymbol(Name);
            _nest = location.Nest;
            _index = location.Index;
        }

        public override object Evaluate(Environment env) => _index == -1 ? env.GetValue(Name) : env.GetValue(_nest, _index);

        public void AssignEvaluate(Environment env, object value)
        {
            if (_index == -1) env.SetValue(Name, value);
            else env.SetValue(_nest, _index, value);
        }
    }
}
