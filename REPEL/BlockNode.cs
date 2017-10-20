using System.Collections.ObjectModel;

namespace REPEL
{
    public class BlockNode : ASTBranch
    {
        private Symbols _inner;

        public Symbols InnerSymbol => _inner;

        public BlockNode(Collection<IASTNode> children) : base(children) { }

        public override void Lookup(Symbols sym)
        {
            _inner = new Symbols(sym);
            foreach (var node in this) node.Lookup(_inner);
        }

        public override object Evaluate(Environment env)
        {
            Environment inner = new Environment(InnerSymbol.Count, env);

            object result = Atom.AtomNull;
            foreach (var node in this) if ((!(node is NullNode))) result = node.Evaluate(inner);
            return result;
        }
    }
}
