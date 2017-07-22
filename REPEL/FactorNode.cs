using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class FactorNode : ASTBranch
    {
        public IASTNode Prefix => this[0];

        public IASTNode Operand => this[1];

        public FactorNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString() => Prefix is NullNode ? Operand.ToString() : (Prefix as ASTLeaf).Token.Text + Operand.ToString();

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
