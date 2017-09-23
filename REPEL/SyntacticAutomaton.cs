using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace REPEL
{
    public static class SyntacticAutomaton
    {
        private static readonly HashSet<string> _reserved = new HashSet<string>()
        {
            "as", "is", "in", "and", "or",
            "break", "continue", "return",
            "if", "else", "case", "of", "eq", "st", "while", "do", "for",
            "func", "class", "import"
        };

        private static readonly HashSet<string> _sliced = new HashSet<string>() { ",", "]" };
        private static readonly HashSet<string> _suffix = new HashSet<string>() { "(", "[", "." };
        private static readonly HashSet<string> _prefix = new HashSet<string>() { "+", "-", "!", "~", "++", "--", "@", "^", "&" };
        private static readonly HashSet<string> _assign = new HashSet<string>() { "=", "**=", "*=", "/=", "\\=", "%=", "+=", "-=", "<<=", ">>=", "&=", "^=", ",=", "&&=", ",,=" };
        private static readonly HashSet<string> _ending = new HashSet<string>() { ";", Token.EOL };

        private static readonly Dictionary<string, Precedence> _operators = new Dictionary<string, Precedence>()
        {
            { "**", new Precedence(1, false) },
            { "*", new Precedence(2, true) },
            { "/", new Precedence(2,true) },
            { "\\", new Precedence(2,true) },
            { "%", new Precedence(2,true) },
            { "+", new Precedence(3, true) },
            { "-", new Precedence(3, true) },
            { "<<", new Precedence(4, true) },
            { ">>", new Precedence(4, true) },
            { "&", new Precedence(5, true) },
            { "^", new Precedence(6, true) },
            { "|", new Precedence(7, true) },
            { "as", new Precedence(8, true) },
            { "==", new Precedence(9, true) },
            { "!=", new Precedence(9, true) },
            { "<", new Precedence(9, true) },
            { "<=", new Precedence(9, true) },
            { ">", new Precedence(9, true) },
            { ">=", new Precedence(9, true) },
            { "&&", new Precedence(10, true) },
            { "||", new Precedence(11, true) },
            { "is", new Precedence(12, true) },
            { "!is", new Precedence(12, true) },
            { "in", new Precedence(13, true) },
            { "!in", new Precedence(13, true) },
            { "and", new Precedence(14, true) },
            { "or", new Precedence(14, true) },
        };

        public static IASTNode Program(Lexer lexer)
        {
            if (IsNext(lexer, _ending, skip: true)) return new NullNode(new Collection<IASTNode>());

            IASTNode main = MainLine(lexer);
            Skip(lexer, _ending);
            return main;
        }

        private static IASTNode MainLine(Lexer lexer)
        {
            if (IsNext(lexer, "import", skip: true))
            {
                Collection<IASTNode> path = new Collection<IASTNode>() { Name(lexer, avoid: false) };

                while (!IsNext(lexer, _ending))
                {
                    Skip(lexer, ".");
                    path.Add(Name(lexer, avoid: false));
                }

                return new ImportNode(path);
            }
            else return Statement(lexer);
        }

        private static IASTNode Statement(Lexer lexer)
        {
            if (IsNext(lexer, "class")) return ClassDefinition(lexer);
            else if (IsNext(lexer, "func")) return FunctionDefinition(lexer);
            else if (IsNext(lexer, "for")) return ForStatement(lexer);
            else if (IsNext(lexer, "do")) return DoWhileStatement(lexer);
            else if (IsNext(lexer, "while")) return WhileStatement(lexer);
            else if (IsNext(lexer, "case")) return CaseStatement(lexer);
            else if (IsNext(lexer, "if")) return IfStatement(lexer);
            else if (IsNext(lexer, ":")) return BlockStatement(lexer);
            else return Simple(lexer);
        }

        private static IASTNode ClassDefinition(Lexer lexer)
        {
            Skip(lexer, "class");
            return new ClassNode(new Collection<IASTNode>() { Name(lexer), IsNext(lexer, "(") ? BaseClass(lexer) : new BaseClassNode(new Collection<IASTNode>()), ClassBody(lexer) });
        }

        private static IASTNode BaseClass(Lexer lexer)
        {
            Collection<IASTNode> bases = new Collection<IASTNode>();

            for (Skip(lexer, "("); !IsNext(lexer, ")", skip: true); )
            {
                bases.Add(Name(lexer));
                if (!IsNext(lexer, ")")) Skip(lexer, ",");
            }

            return new BaseClassNode(bases);
        }

        private static IASTNode ClassBody(Lexer lexer)
        {
            Skip(lexer, ":");
            Collection<IASTNode> statements = new Collection<IASTNode>();

            for (SkipEOL(lexer); !IsNext(lexer, "..", skip: true); SkipEOL(lexer))
            {
                if (IsNext(lexer, "class")) statements.Add(ClassDefinition(lexer));
                else if (IsNext(lexer, "func")) statements.Add(FunctionDefinition(lexer));
                else statements.Add(Expression(lexer));
            }

            return new ClassBodyNode(statements);
        }

        private static IASTNode FunctionDefinition(Lexer lexer)
        {
            Skip(lexer, "func");
            return new FunctionNode(new Collection<IASTNode>() { Name(lexer), Parameter(lexer), BlockStatement(lexer) });
        }

        private static IASTNode Parameter(Lexer lexer)
        {
            Collection<IASTNode> parameters = new Collection<IASTNode>();
            int state = 0;

            for (Skip(lexer, "("); !IsNext(lexer, ")", skip: true);)
            {
                IASTNode name = Name(lexer);

                if (IsNext(lexer, "..."))
                {
                    if (state == 2) throw new ParseException(lexer.Read());
                    else lexer.Read();

                    parameters.Add(new ParameterNode(new Collection<IASTNode>() { name, new NullNode(new Collection<IASTNode>()) }, 1));
                    state = 2;
                }
                else if (IsNext(lexer, "*...", skip: true))
                {
                    parameters.Add(new ParameterNode(new Collection<IASTNode>() { name, new NullNode(new Collection<IASTNode>()) }, 3));
                    Skip(lexer, ")");
                    break;
                }
                else
                {
                    IASTNode value = new NullNode(new Collection<IASTNode>());
                    if (IsNext(lexer, "=", skip: true)) value = Expression(lexer);
                    else if (state == 1) throw new ParseException((name as ASTLeaf).Token);

                    parameters.Add(new ParameterNode(new Collection<IASTNode>() { name, value }, state == 2 ? 2 : 0));
                    if (state == 0 && !(value is NullNode)) state = 1;
                }

                if (!IsNext(lexer, ")")) Skip(lexer, ",");
            }

            return new ParameterListNode(parameters);
        }

        private static IASTNode ForStatement(Lexer lexer)
        {
            Skip(lexer, "for");
            Collection<IASTNode> nodes = new Collection<IASTNode>() { Factor(lexer) };
            Skip(lexer, "in");
            nodes.Add(Expression(lexer));

            SkipEOL(lexer);
            nodes.Add(BlockStatement(lexer));
            return new ForNode(nodes);
        }

        private static IASTNode DoWhileStatement(Lexer lexer)
        {
            Skip(lexer, "do");
            SkipEOL(lexer);
            Collection<IASTNode> nodes = new Collection<IASTNode>() { BlockStatement(lexer) };

            SkipEOL(lexer);
            Skip(lexer, "while");
            nodes.Add(Expression(lexer));
            Skip(lexer, "..");

            return new DoWhileNode(nodes);
        }

        private static IASTNode WhileStatement(Lexer lexer)
        {
            Skip(lexer, "while");
            Collection<IASTNode> nodes = new Collection<IASTNode>() { Expression(lexer) };
            SkipEOL(lexer);
            nodes.Add(BlockStatement(lexer));

            return new WhileNode(nodes);
        }

        private static IASTNode CaseStatement(Lexer lexer)
        {
            Skip(lexer, "case");
            Collection<IASTNode> guards = new Collection<IASTNode>() { Expression(lexer) };
            Skip(lexer, "of");

            for (SkipEOL(lexer); !IsNext(lexer, "..", skip: true); SkipEOL(lexer))
            {
                if (IsNext(lexer, "st", skip: true)) guards.Add(new GuardNode(new Collection<IASTNode>() { Expression(lexer), BlockStatement(lexer) }, 0));
                else if (IsNext(lexer, "eq", skip: true)) guards.Add(new GuardNode(new Collection<IASTNode>() { Expression(lexer), BlockStatement(lexer) }, 1));
                else if (IsNext(lexer, "in", skip: true)) guards.Add(new GuardNode(new Collection<IASTNode>() { Expression(lexer), BlockStatement(lexer) }, 2));
                else if (IsNext(lexer, "else", skip: true)) guards.Add(new GuardNode(new Collection<IASTNode>() { new NullNode(new Collection<IASTNode>()), BlockStatement(lexer) }, 3));
                else throw new ParseException(lexer.Read());
            }
            
            return new CaseNode(guards);
        }

        private static IASTNode IfStatement(Lexer lexer)
        {
            Skip(lexer, "if");
            Collection<IASTNode> guards = new Collection<IASTNode>();

            for (SkipEOL(lexer); !IsNext(lexer, "..", skip: true); SkipEOL(lexer))
            {
                if (IsNext(lexer, "else", skip: true)) guards.Add(new GuardNode(new Collection<IASTNode>() { new NullNode(new Collection<IASTNode>()), BlockStatement(lexer) }, 3));
                else guards.Add(new GuardNode(new Collection<IASTNode>() { Expression(lexer), BlockStatement(lexer) }, 0));
            }

            return new IfNode(guards);
        }

        private static IASTNode BlockStatement(Lexer lexer)
        {
            Skip(lexer, ":");
            Collection<IASTNode> block = new Collection<IASTNode>();

            for (SkipEOL(lexer); !IsNext(lexer, "..", skip: true); SkipEOL(lexer))
            {
                if (IsNext(lexer, _ending, skip: true)) continue;

                block.Add(Statement(lexer));
                if (IsNext(lexer, "..", skip: true)) break;
                Skip(lexer, _ending);
            }

            return new BlockNode(block);
        }

        private static IASTNode Simple(Lexer lexer)
        {
            if (IsNext(lexer, "break", skip: true)) return new ControlNode(new Collection<IASTNode>() { new NullNode(new Collection<IASTNode>()) }, 0);
            else if (IsNext(lexer, "continue", skip: true)) return new ControlNode(new Collection<IASTNode>() { new NullNode(new Collection<IASTNode>()) }, 1);
            else if (IsNext(lexer, "return", skip: true)) return IsNext(lexer, _ending) || IsNext(lexer, "..") ? new ControlNode(new Collection<IASTNode>() { new NullNode(new Collection<IASTNode>()) }, 2) : new ControlNode(new Collection<IASTNode>() { Expression(lexer) }, 2);
            else return Expression(lexer);
        }

        private static IASTNode Expression(Lexer lexer)
        {
            if (IsNext(lexer, "/\\", skip: true)) return new LambdaNode(new Collection<IASTNode>() { Parameter(lexer), BlockStatement(lexer) });

            IASTNode expr = Factor(lexer);
            if (IsNext(lexer, _assign)) return new ExpressionNode(new Collection<IASTNode>() { expr, new ASTLeaf(lexer.Read()), Expression(lexer) });
            else
            {
                Precedence next;
                while ((next = NextOperator(lexer)) != null) expr = Shift(lexer, expr, next.Level);
                return expr;
            }
        }

        private static IASTNode Factor(Lexer lexer)
        {
            Collection<IASTNode> factor = new Collection<IASTNode>();
            while (IsNext(lexer, _prefix)) factor.Add(new ASTLeaf(lexer.Read()));

            factor.Add(Primary(lexer));
            return new FactorNode(factor);
        }

        private static IASTNode Primary(Lexer lexer)
        {
            Collection<IASTNode> primary = new Collection<IASTNode>();

            if (IsNext(lexer, "(", skip: true))
            {
                primary.Add(Expression(lexer));
                Skip(lexer, ")");
            }
            else if (IsNext(lexer, "[")) primary.Add(List(lexer));
            else if (IsNext(lexer, "[*")) primary.Add(Tuple(lexer));
            else if (IsNext(lexer, "<*")) primary.Add(ByteList(lexer));
            else if (IsNext(lexer, "{")) primary.Add(Dictionary(lexer));
            else
            {
                Token next = lexer.Read();
                if (next.IsInteger) primary.Add(new IntegerNode(next));
                else if (next.IsFloat) primary.Add(new FloatNode(next));
                else if (next.IsString) primary.Add(new StringNode(next));
                else if (next.IsName) primary.Add(new NameNode(next));
                else throw new InternalException("no such primary type");
            }

            while (IsNext(lexer, _suffix))
            {
                if (IsNext(lexer, "(")) primary.Add(Argument(lexer));
                else if (IsNext(lexer, "[")) primary.Add(Reference(lexer));
                else if (IsNext(lexer, ".")) primary.Add(Member(lexer));
                else throw new InternalException("no such suffix");
            }

            return new PrimaryNode(primary);
        }

        private static IASTNode List(Lexer lexer)
        {
            Collection<IASTNode> list = new Collection<IASTNode>();

            for (Skip(lexer, "["); !IsNext(lexer, "]", skip: true);)
            {
                list.Add(Range(lexer));
                if (!IsNext(lexer, "]")) Skip(lexer, ",");
            }
            
            return new ListNode(list);
        }

        private static IASTNode Tuple(Lexer lexer)
        {
            Collection<IASTNode> tuple = new Collection<IASTNode>();

            for (Skip(lexer, "[*"); !IsNext(lexer, "*]", skip: true);)
            {
                tuple.Add(Range(lexer));
                if (!IsNext(lexer, "*]")) Skip(lexer, ",");
            }

            return new TupleNode(tuple);
        }

        private static IASTNode ByteList(Lexer lexer)
        {
            Collection<IASTNode> bytes = new Collection<IASTNode>();

            for (Skip(lexer, "<*"); !IsNext(lexer, "*>", skip: true);)
            {
                bytes.Add(Range(lexer));
                if (!IsNext(lexer, "*>")) Skip(lexer, ",");
            }

            return new ByteListNode(bytes);
        }

        private static IASTNode Dictionary(Lexer lexer)
        {
            Collection<IASTNode> dictionary = new Collection<IASTNode>();

            for (Skip(lexer, "{"); !IsNext(lexer, "}", skip: true);)
            {
                dictionary.Add(Pair(lexer));
                if (!IsNext(lexer, "}")) Skip(lexer, ",");
            }

            return new DictionaryNode(dictionary);
        }

        private static IASTNode Range(Lexer lexer)
        {
            IASTNode start = Expression(lexer);
            return IsNext(lexer, ":", skip: true) ? new RangeNode(new Collection<IASTNode>() { start, Expression(lexer), IsNext(lexer, ":", skip: true) ? Expression(lexer) : new NullNode(new Collection<IASTNode>()) }) : start;
        }

        private static IASTNode Argument(Lexer lexer)
        {
            Collection<IASTNode> arguments = new Collection<IASTNode>();
            bool key = false;

            for (Skip(lexer, "("); !IsNext(lexer, ")", skip: true);)
            {
                if (key || IsNext(lexer, ":", count: 1))
                {
                    arguments.Add(Pair(lexer, arg: true));
                    key = true;
                }
                else arguments.Add(Expression(lexer));

                if (!IsNext(lexer, ")")) Skip(lexer, ",");
            }

            return new ArgumentNode(arguments);
        }

        private static IASTNode Pair(Lexer lexer, bool arg = false)
        {
            IASTNode key = arg ? Name(lexer) : Expression(lexer);
            Skip(lexer, ":");
            return new PairNode(new Collection<IASTNode>() { key, Expression(lexer) });
        }

        private static IASTNode Reference(Lexer lexer)
        {
            Collection<IASTNode> references = new Collection<IASTNode>();

            for (Skip(lexer, "["); ;)
            {
                references.Add(Slice(lexer));
                if (IsNext(lexer, "]", skip: true)) break;
                else Skip(lexer, ",");
            }

            return new ReferenceNode(references);
        }

        private static IASTNode Slice(Lexer lexer)
        {
            if (IsNext(lexer, ":", skip: true))
            {
                Collection<IASTNode> slice = new Collection<IASTNode>() { new NullNode(new Collection<IASTNode>()) };

                if (IsNext(lexer, _sliced))
                {
                    slice.Add(new NullNode(new Collection<IASTNode>()));
                    slice.Add(new NullNode(new Collection<IASTNode>()));
                    return new SliceNode(slice);
                }

                if (IsNext(lexer, ":", skip: true)) slice.Add(new NullNode(new Collection<IASTNode>()));
                else
                {
                    slice.Add(Expression(lexer));
                    if (!IsNext(lexer, ":", skip: true))
                    {
                        slice.Add(new NullNode(new Collection<IASTNode>()));
                        return new SliceNode(slice);
                    }
                }

                slice.Add(IsNext(lexer, _sliced) ? new NullNode(new Collection<IASTNode>()) : Expression(lexer));
                return new SliceNode(slice);
            }

            IASTNode start = Expression(lexer);
            if (!IsNext(lexer, ":", skip: true)) return start;

            Collection<IASTNode> slicee = new Collection<IASTNode>() { start };

            if (IsNext(lexer, _sliced))
            {
                slicee.Add(new NullNode(new Collection<IASTNode>()));
                slicee.Add(new NullNode(new Collection<IASTNode>()));
                return new SliceNode(slicee);
            }

            if (IsNext(lexer, ":", skip: true)) slicee.Add(new NullNode(new Collection<IASTNode>()));
            else
            {
                slicee.Add(Expression(lexer));
                if (!IsNext(lexer, ":", skip: true))
                {
                    slicee.Add(new NullNode(new Collection<IASTNode>()));
                    return new SliceNode(slicee);
                }
            }

            slicee.Add(IsNext(lexer, _sliced) ? new NullNode(new Collection<IASTNode>()) : Expression(lexer));
            return new SliceNode(slicee);
        }

        private static IASTNode Member(Lexer lexer)
        {
            Skip(lexer, ".");
            return new MemberNode(new Collection<IASTNode>() { Name(lexer) });
        }

        private static IASTNode Name(Lexer lexer, bool avoid = true)
        {
            Token name = lexer.Read();
            if (!name.IsName || (avoid && _reserved.Contains(name.Text))) throw new ParseException(name);
            return new NameNode(name);
        }

        private static void Skip(Lexer lexer, string name)
        {
            Token next = lexer.Read();
            if (!next.IsIdentifier || next.Text != name) throw new ParseException(next);
        }

        private static void Skip(Lexer lexer, HashSet<string> set)
        {
            Token next = lexer.Read();
            if (!next.IsIdentifier || !set.Contains(next.Text)) throw new ParseException(next);
        }

        private static void SkipEOL(Lexer lexer) { while (IsNext(lexer, Token.EOL, skip: true)) ; }

        private static bool IsNext(Lexer lexer, string name, bool skip = false, int count = 0)
        {
            Token next = lexer.Peek(count);
            if (next.IsIdentifier && next.Text == name)
            {
                if (skip) lexer.Read();
                return true;
            }
            else return false;
        }

        private static bool IsNext(Lexer lexer, HashSet<string> set, bool skip = false, int count = 0)
        {
            Token next = lexer.Peek(count);
            if (next.IsIdentifier && set.Contains(next.Text))
            {
                if (skip) lexer.Read();
                return true;
            }
            else return false;
        }

        private static IASTNode Shift(Lexer lexer, IASTNode left, int level)
        {
            ASTLeaf op = new ASTLeaf(lexer.Read());
            IASTNode right = Factor(lexer);
            
            for (Precedence next; (next = NextOperator(lexer)) != null && RightFirst(level, next); right = Shift(lexer, right, next.Level)) ;
            return new ExpressionNode(new Collection<IASTNode>() { left, op, right });
        }

        private static bool RightFirst(int level, Precedence next) => level > next.Level || (level == next.Level && !next.IsLeft);

        private static Precedence NextOperator(Lexer lexer)
        {
            Token next = lexer.Peek(0);
            return next.IsIdentifier && _operators.Keys.Contains(next.Text) ? _operators[next.Text] : null;
        }

        private class Precedence
        {
            public int Level { get; protected set; }

            public bool IsLeft { get; protected set; }

            public Precedence()
            {
                Level = 99;
                IsLeft = true;
            }

            public Precedence(int level, bool isLeft)
            {
                Level = level;
                IsLeft = isLeft;
            }
        }
    }
}
