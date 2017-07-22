using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class ControlNode : ASTBranch
    {
        public IASTNode Type => this[0];

        public IASTNode Value => this[1];

        public ControlNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString() => "(" + Type.ToString() + (Value is NullNode ? "" : " " + Value.ToString()) + ")";

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
