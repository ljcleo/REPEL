using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class ControlNode : ASTBranch
    {
        public int Type { get; protected set; }

        public IASTNode Value => this[0];

        public ControlNode(Collection<IASTNode> children) : this(children, 0) { }

        public ControlNode(Collection<IASTNode> children, int type) : base(children) => Type = type;

        private string GetTypeString()
        {
            switch (Type)
            {
                case 0: return "break";
                case 1: return "continue";
                case 2: return "return";
                default: throw new InternalException("bad control type");
            }
        }

        public override string ToString() => "(" + GetTypeString() + (Value is NullNode ? "" : " " + Value.ToString()) + ")";

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
