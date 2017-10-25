using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class WhileNode : BlockNode
    {
        public IASTNode Condition => this[0];

        public IASTNode Body => this[1];

        public WhileNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString() => "(while " + Condition.ToString() + " " + Body.ToString() + ")";

        public override object Evaluate(Environment env)
        {
            Environment inner = new Environment(InnerSymbol.Count, env);
            while (ExpressionNode.GetBoolValue(Condition.Evaluate(inner))) Body.Evaluate(inner);
            return Atom.AtomNull;
        }
    }
}
