using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class PairNode : ASTBranch
    {
        public IASTNode Key => this[0];

        public IASTNode Value => this[1];

        public PairNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString() => Key.ToString() + ":" + Value.ToString();
    }
}
