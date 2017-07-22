using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class SliceNode : ASTBranch
    {
        public IASTNode Start => this[0];

        public IASTNode End => this[1];

        public IASTNode Step => this[2];

        public SliceNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString() => Start.ToString() + ":" + End.ToString() + ":" + Step.ToString();

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
