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

        public bool EvaluateCondition(Environment env, object left)
        {
            bool judge = true;

            switch (Type)
            {
                case 0:
                    judge = ExpressionNode.GetBoolValue(Condition.Evaluate(env));
                    break;
                case 1:
                    if (left == null) throw new InterpretException("cannot use 'eq' without left value");
                    judge = left.Equals(Condition.Evaluate(env));
                    break;
                case 2:
                    if (left == null) throw new InterpretException("cannot use 'in' without left value");
                    throw new InternalException("'in' not implemented currently");
            }

            return judge;
        }

        public override string ToString() => "(" + GetTypeString() + (Condition is NullNode ? "" : Condition.ToString()) + " " + Body.ToString() + ")";

        public override object Evaluate(Environment env) => Body.Evaluate(env);
    }
}
