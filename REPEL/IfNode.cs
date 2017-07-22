using System;
using System.Collections.ObjectModel;
using System.Text;

namespace REPEL
{
    public class IfNode : ASTBranch
    {
        public IfNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(if ");
            foreach (var node in this) builder.Append(node.ToString());
            return builder.Append(")").ToString();
        }

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
