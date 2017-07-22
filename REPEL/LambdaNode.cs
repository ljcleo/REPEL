using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class LambdaNode : ASTBranch
    {
        public IASTNode Parameters => this[0];

        public IASTNode Body => this[1];

        public LambdaNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString() => "(/\\ " + Parameters.ToString() + " -> " + Body.ToString();

        public override object Evaluate(Environment env) => throw new NotImplementedException()
    }
}
