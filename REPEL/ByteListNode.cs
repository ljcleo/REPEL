using System;
using System.Collections.ObjectModel;
using System.Text;

namespace REPEL
{
    public class ByteListNode : ASTBranch
    {
        public ByteListNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("<*");

            string sep = "";
            foreach (var node in this)
            {
                builder.Append(sep);
                sep = ",";
                builder.Append(node.ToString());
            }

            return builder.Append("*>").ToString();
        }

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
