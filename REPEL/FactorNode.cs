using System;
using System.Collections.ObjectModel;
using System.Text;

namespace REPEL
{
    public class FactorNode : ASTBranch
    {
        public IASTNode Operand => this[Count - 1];

        public FactorNode(Collection<IASTNode> children) : base(children) { }

        public IASTNode Prefix(int index) => this[index];

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var node in this) builder.Append(node is ASTLeaf ? (node as ASTLeaf).Token.Text : node.ToString());
            return builder.ToString();
        }

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
