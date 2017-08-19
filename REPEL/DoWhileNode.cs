using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class DoWhileNode : ASTBranch
    {
        public IASTNode Body => this[0];

        public IASTNode Condition => this[1];

        public DoWhileNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString() => "(do " + Body.ToString() + " while " + Condition.ToString() + ")";

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
