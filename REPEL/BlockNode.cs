using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class BlockNode : ASTBranch
    {
        public BlockNode(Collection<IASTNode> children) : base(children) { }

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
