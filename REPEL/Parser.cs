namespace REPEL
{
    public class Parser
    {
        private Lexer _lexer;

        public Parser(Lexer lexer) => _lexer = lexer;

        public IASTNode Parse() => SyntacticAutomaton.Program(_lexer);
    }
}
