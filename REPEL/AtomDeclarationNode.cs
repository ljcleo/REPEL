using System.Collections.ObjectModel;
using System.Text;

namespace REPEL
{
    public class AtomDeclarationNode : ASTBranch
    {
        public AtomDeclarationNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(");

            string sep = "";
            foreach (var node in this)
            {
                builder.Append(sep);
                sep = ",";
                builder.Append(node.ToString());
            }

            return builder.Append(")").ToString();
        }

        public override void Lookup(Symbols sym)
        {
            foreach (var node in this)
            {
                NameNode name = node as NameNode;
                if (name == null) throw new InternalException("bad atom assignment");

                if (sym.Contains(name.Name)) throw new InterpretException("atom name used");
                else name.AssignLookup(sym, 1);
            }
        }

        public override object Evaluate(Environment env)
        {
            foreach (var node in this)
            {
                NameNode name = node as NameNode;
                if (name == null) throw new InternalException("bad atom assignment");

                name.AssignEvaluate(env, new Atom(name.Name));
            }

            return Atom.AtomNull;
        }
    }
}