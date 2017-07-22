using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class GuardNode : ASTBranch
    {
        public IASTNode Type => this[0];

        public IASTNode Condition => this[1];

        public IASTNode Body => this[2];

        public GuardNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString() => "(" + (Type as ASTLeaf).Token.Text + (Condition is NullNode ? "" : Condition.ToString()) + " -> " + Body.ToString() + ")";

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
