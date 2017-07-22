using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class PostfixNode : ASTBranch
    {
        public PostfixNode(Collection<IASTNode> children) : base(children) { }

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
