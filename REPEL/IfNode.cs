using System.Collections.ObjectModel;
using System.Text;

namespace REPEL
{
    public class IfNode : BlockNode
    {
        public IfNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(if ");
            foreach (var node in this) builder.Append(node.ToString());
            return builder.Append(")").ToString();
        }

        public override object Evaluate(Environment env)
        {
            Environment inner = new Environment(InnerSymbol.Count, env);

            foreach (var node in this)
            {
                GuardNode guard = node as GuardNode;
                if (guard == null) throw new InternalException("bad guard");

                if (guard.EvaluateCondition(env, null))
                {
                    guard.Evaluate(inner);
                    break;
                }
            }

            return Atom.AtomNull;
        }
    }
}
