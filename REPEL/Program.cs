using System;

namespace REPEL
{
    class Program
    {
        static void Main()
        {
            DateTime start = DateTime.Now;

            Lexer lexer = new Lexer(Console.In);
            for (Token t; (t = lexer.Read()) != Token.EOF;) Console.WriteLine(t);

            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine("Hello world! Elapsed Time: {0:F2}ms", (DateTime.Now - start).TotalMilliseconds);
            Console.ReadLine();
        }
    }
}
