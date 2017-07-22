using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace REPEL
{
    public class CaseNode : ASTBranch
    {
        public IASTNode TestValue => this[0];

        public CaseNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(case ").Append(TestValue.ToString()).Append(" ");
            foreach (var node in this.Skip(1)) builder.Append(node.ToString());
            return builder.Append(")").ToString();
        }

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
