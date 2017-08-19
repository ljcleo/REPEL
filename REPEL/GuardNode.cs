using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class GuardNode : ASTBranch
    {
        public int Type { get; protected set; }

        public IASTNode Condition => this[0];

        public IASTNode Body => this[1];

        public GuardNode(Collection<IASTNode> children) : this(children, 0) { }

        public GuardNode(Collection<IASTNode> children, int type) : base(children) => Type = type;

        private string GetTypeString()
        {
            switch (Type)
            {
                case 0: return "st";
                case 1: return "eq";
                case 2: return "in";
                case 3: return "else";
                default: throw new InternalException("bad guard type");
            }
        }

        public override string ToString() => "(" + GetTypeString() + (Condition is NullNode ? "" : Condition.ToString()) + " " + Body.ToString() + ")";

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
