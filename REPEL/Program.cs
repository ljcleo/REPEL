using System;

namespace REPEL
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Write your scirpt here and get syntactic analyze results");
            Console.WriteLine("-----------------------------");
            DateTime start = DateTime.Now;

            Lexer lexer = new Lexer(Console.In);
            Parser parser = new Parser(lexer);
            SyntaticAnalyze(lexer, parser);

            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine("Hello world! Elapsed Time: {0:F2}ms", (DateTime.Now - start).TotalMilliseconds);
            Console.ReadLine();
        }

        private static void LexicalAnalyze(Lexer lexer) { for (Token t; (t = lexer.Read()) != Token.EOF;) Console.WriteLine(t); }

        private static void SyntaticAnalyze(Lexer lexer, Parser parser)
        {
            for (IASTNode t; lexer.Peek(0) != Token.EOF;)
            {
                try
                {
                    t = parser.Parse();
                    Console.WriteLine("=> " + t.ToString());
                }
                catch (Exception e) { Console.WriteLine(e.Message); }
            }
        }
    }
}
