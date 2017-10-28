using System.Collections.ObjectModel;

namespace REPEL
{
    public class BlockNode : ASTBranch
    {
        private Symbols _inner;

        public Symbols InnerSymbol => _inner;

        public BlockNode(Collection<IASTNode> children) : base(children) { }

        public override void Lookup(Symbols sym) => Lookup(sym, false);

        public void Lookup(Symbols sym, bool withAnonymous)
        {
            _inner = new Symbols(sym);
            if (withAnonymous) _inner.SetSymbol(2, "_");
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
