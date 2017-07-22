namespace REPEL
{
    public class StringNode : ASTLeaf
    {
        public string String => Token.Text;

        public StringNode(Token token) : base(token) { }

        public override object Evaluate(Environment env) => String;
    }
}
