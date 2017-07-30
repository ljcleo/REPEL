using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class MemberNode : PostfixNode
    {
        public IASTNode Name => this[0];

        public MemberNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString() => (Name as ASTLeaf).Token.Text;

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
