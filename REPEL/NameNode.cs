using System;

namespace REPEL
{
    public class NameNode : ASTLeaf
    {
        public string Name => Token.Text;

        public bool IsAnonymous => Name == "_";

        public NameNode(Token token) : base(token) { }

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
