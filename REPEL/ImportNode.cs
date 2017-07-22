using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class ImportNode : ASTBranch
    {
        public IASTNode Path => this[0];

        public ImportNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString() => Path.ToString();

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
