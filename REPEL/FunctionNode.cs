using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class FunctionNode : ASTBranch
    {
        public IASTNode Name => this[0];

        public IASTNode Parameters => this[1];

        public IASTNode Body => this[2];

        public FunctionNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString() { return "(function " + Name + " " + Parameters + " " + Body + ")"; }

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
