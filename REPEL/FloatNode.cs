namespace REPEL
{
    public class FloatNode : ASTLeaf
    {
        public double Float => Token.FloatValue;

        public FloatNode(Token token) : base(token) { }

        public override object Evaluate(Environment env) => Float;
    }
}
