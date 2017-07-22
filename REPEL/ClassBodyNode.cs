using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class ClassBodyNode : ASTBranch
    {
        public ClassBodyNode(Collection<IASTNode> children) : base(children) { }

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
