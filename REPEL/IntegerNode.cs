namespace REPEL
{
    public class IntegerNode : ASTLeaf
    {
        public long Integer => Token.IntegerValue;

        public IntegerNode(Token token) : base(token) { }

        public override object Evaluate(Environment env) => Integer;
    }
}
