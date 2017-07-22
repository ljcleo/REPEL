using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class ForNode : ASTBranch
    {
        public IASTNode Variable => this[0];

        public IASTNode Range => this[1];

        public IASTNode Body => this[2];

        public ForNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString() => "for " + Variable.ToString() + " in " + Range.ToString() + " -> " + Body.ToString();

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
