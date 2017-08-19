using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class ClassNode : ASTBranch
    {
        public IASTNode Name => this[0];

        public IASTNode BaseClasses => this[1];

        public IASTNode Body => this[2];

        public ClassNode(Collection<IASTNode> children) : base(children) { }

        public override string ToString() => "(class " + Name.ToString() + " : " + BaseClasses.ToString() + " " + Body.ToString();

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
