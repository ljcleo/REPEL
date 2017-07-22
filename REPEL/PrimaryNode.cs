using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class PrimaryNode : ASTBranch
    {
        public PrimaryNode(Collection<IASTNode> children) : base(children) { }

        public IASTNode Suffix(int layer) { return this[Count - layer - 1]; }

        public bool HasSuffix(int layer) { return Count - layer > 1; }

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
