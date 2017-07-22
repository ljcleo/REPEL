using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class ExpressionNode : ASTBranch
    {
        public IASTNode Left => this[0];

        public IASTNode Operator => this[1];

        public IASTNode Right => this[2];

        public ExpressionNode(Collection<IASTNode> children) : base(children) { }

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
