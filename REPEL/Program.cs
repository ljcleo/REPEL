using System;

namespace REPEL
{
    public class Program
    {
        public static void Main()
        {
            //Console.WriteLine("Write your scirpt here and get syntactic analyze results");
            Console.WriteLine("Write your script and get interpret result");
            Console.WriteLine("-----------------------------");
            DateTime start = DateTime.Now;

            Lexer lexer = new Lexer(Console.In);
            Parser parser = new Parser(lexer);

            GlobalEnvironment env = new GlobalEnvironment();
            //SyntaticAnalyze(lexer, parser);
            Interpret(lexer, parser, env);

            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine("Hello world! Elapsed Time: {0:F2}ms", (DateTime.Now - start).TotalMilliseconds);
            Console.ReadLine();
        }

        private static void LexicalAnalyze(Lexer lexer) { for (Token t; (t = lexer.Read()) != Token.EOF;) Console.WriteLine(t); }

        private static void SyntaticAnalyze(Lexer lexer, Parser parser)
        {
            while (true)
            {
                try
                {
                    for (IASTNode t; lexer.Peek(0) != Token.EOF;)
                    {
                        t = parser.Parse();
                        Console.WriteLine("=> " + t.ToString());
                    }

                    break;
                }
                catch (Exception e) { Console.WriteLine("E> " + e.Message); }
            }
        }

        private static void Interpret(Lexer lexer, Parser parser, GlobalEnvironment env)
        {
            while (true)
            {
                try
                {
                    for (IASTNode t; lexer.Peek(0) != Token.EOF;)
                    {
                        t = parser.Parse();
                        if (!(t is NullNode))
                        {
                            t.Lookup(env.Names);

                            object result = t.Evaluate(env);
                            if (result is string) Console.WriteLine("=> \"" + result + "\"");
                            else if (result is Atom) Console.WriteLine("=> '" + result + "'");
                            else Console.WriteLine("=> " + result);
                        }
                    }

                    break;
                }
                catch (Exception e) { Console.WriteLine("E> " + e.Message); }
            }
        }
    }
}
