using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace REPEL
{
    public class CaseNode : BlockNode
    {
        public IASTNode TestValue => this[0];

        public CaseNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(case ").Append(TestValue.ToString()).Append(" ");
            foreach (var node in this.Skip(1)) builder.Append(node.ToString());
            return builder.Append(")").ToString();
        }

        public override void Lookup(Symbols sym) => base.Lookup(sym, true);

        public override object Evaluate(Environment env)
        {
            Environment inner = new Environment(InnerSymbol.Count, env);

            object left = TestValue.Evaluate(inner);
            if (InnerSymbol.Contains("_"))
            {
                int index = InnerSymbol["_"];
                inner.SetValue(0, index, left);
            }

            foreach (var node in this)
            {
                if (node == TestValue) continue;

                GuardNode guard = node as GuardNode;
                if (guard == null) throw new InternalException("bad guard");

                if (guard.EvaluateCondition(inner, left))
                {
                    guard.Evaluate(inner);
                    break;
                }
            }

            return Atom.AtomNull;
        }
    }
}
