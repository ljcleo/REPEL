using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace REPEL
{
    public static class LexicalAutomaton
    {
        private static readonly Dictionary<char, int> _baseChars = new Dictionary<char, int>() { { 'b', 2 }, { 'o', 8 }, { 'd', 10 }, { 'x', 16 } };

        private static readonly Dictionary<char, char> _escapeChars = new Dictionary<char, char>()
        {
            { '"', '\"' },
            { '\\', '\\' },
            { '0', '\0' },
            { 'a', '\a' },
            { 'b', '\b' },
            { 'f', '\f' },
            { 'n', '\n' },
            { 'r', '\r' },
            { 't', '\t' },
            { 'v', '\v' }
        };

        private static readonly List<char> _rawEscapeChars = new List<char>() { '"', '$' };

        private static readonly List<string> _longOperators = new List<string>()
        {
            "*...",
            "**=", "<<=", ">>=", "&&=", "||=",
            "...",
            "..",
            "++", "--",
            "**",
            "<<", ">>",
            "==", "!=", "<=", ">=",
            "&&", "||",
            "*=", "/=", "\\=", "%=", "+=", "-=", "&=", "|=", "^=",
            "[*", "*]", "<*", "*>", "/\\", "!is", "!in"
        };
        
        public static Collection<Token> Match(int currentLine, string parseString, out string leftString)
        {
            StringBuilder builder = new StringBuilder(parseString);
            builder.Append("\0\0\0\0\0");

            Collection<Token> tokens = new Collection<Token>();
            leftString = string.Empty;

            try
            {
                while (builder[0] != '\0')
                {
                    while (char.IsWhiteSpace(builder[0])) builder.Remove(0, 1);
                    if (builder[0] == '\0') break;

                    if (builder[0] == '/')
                    {
                        if (builder[1] == '/')
						{
						    leftString = "";
						    tokens.Add(new IdentifierToken(currentLine, Token.EOL));
							return tokens;
						}
                        else if (builder[1] == '*')
                        {
                            MatchComment(builder, out leftString);
                            continue;
                        }
                    }

                    if (builder[0] == '0' && _baseChars.Keys.Contains(char.ToLower(builder[1], CultureInfo.CurrentCulture)))
                    {
                        int integerBase = _baseChars[char.ToLower(builder[1], CultureInfo.CurrentCulture)];
                        builder.Remove(0, 2);
                        tokens.Add(MatchInteger(builder, currentLine, integerBase));
                    }
                    else if (char.IsDigit(builder[0]))
                    {
                        if (char.ToLower(builder[1], CultureInfo.CurrentCulture) == 'k')
                        {
                            int integerBase = builder[0] - '0';
                            builder.Remove(0, 2);
                            tokens.Add(MatchInteger(builder, currentLine, integerBase));
                        }
                        else if (char.IsDigit(builder[1]) && char.ToLower(builder[2], CultureInfo.CurrentCulture) == 'k')
                        {
                            int integerBase = (builder[0] - '0') * 10 + builder[1] - '0';
                            builder.Remove(0, 3);
                            tokens.Add(MatchInteger(builder, currentLine, integerBase));
                        }
                        else tokens.Add(MatchDecimal(builder, currentLine));
                    }
                    else if (builder[0] == '.' && char.IsDigit(builder[1]))
                    {
                        builder.Remove(0, 1);
                        tokens.Add(MatchFloat(builder, currentLine, 0));
                    }
                    else if (builder[0] == '"') tokens.Add(MatchString(builder, currentLine));
                    else if (builder[0] == '@' && builder[1] == '"')
                    {
                        Token rawString = MatchRawString(builder, currentLine, out leftString);
                        if (rawString != null) tokens.Add(rawString);
                    }
                    else if (builder[0] == '\'') tokens.Add(MatchComplexName(builder, currentLine));
                    else if (char.IsLetter(builder[0]) || builder[0] == '_') tokens.Add(MatchWord(builder, currentLine));
                    else tokens.Add(MatchOperator(builder, currentLine));
                }

                if (leftString == null) leftString = string.Empty;
                if (leftString.Length == 0) tokens.Add(new IdentifierToken(currentLine, Token.EOL));
                return tokens;
            }
            catch (ParseException) { throw; }
        }

        private static void MatchComment(StringBuilder builder, out string leftString)
        {
            leftString = builder.ToString().Substring(0, builder.Length - 5);
            builder.Remove(0, 2);

            while (builder[0] != '*' || builder[1] != '/')
            {
                if (builder[0] == '\0') return;
                builder.Remove(0, 1);
            }

            builder.Remove(0, 2);
            leftString = "";
        }

        private static Token MatchInteger(StringBuilder builder, int currentLine, int integerBase)
        {
            if (integerBase < 2 || integerBase > 36) throw new ParseException("bad number");

            long value = 0;
            try
            {
                for (long bit = 0; builder.Length > 2 && char.IsLetterOrDigit(builder[0]); builder.Remove(0, 1))
                {
                    if ((bit = ConvertChar(builder[0])) >= integerBase) throw new ParseException("bad number");
                    value = value * integerBase + bit;
                }
            }
            catch (ParseException) { throw; }

            return new IntegerToken(currentLine, value);
        }

        private static Token MatchDecimal(StringBuilder builder, int currentLine)
        {
            long value = 0;
            for (; char.IsDigit(builder[0]); builder.Remove(0, 1)) value = value * 10 + builder[0] - '0';
            
            if (builder[0] == '.')
            {
                if (!char.IsDigit(builder[1])) throw new ParseException("bad number");
                builder.Remove(0, 1);
                return MatchFloat(builder, currentLine, value);
            }
            if (char.ToLower(builder[0], CultureInfo.CurrentCulture) == 'e')
            {
                if (!char.IsDigit(builder[1]) && !((builder[1] == '-' || builder[1] == '+') && char.IsDigit(builder[2]))) throw new ParseException("bad number");
                builder.Remove(0, 1);
                return MatchExponent(builder, currentLine, value);
            }

            return new IntegerToken(currentLine, value);
        }

        private static Token MatchFloat(StringBuilder builder, int currentLine, long integerPart)
        {
            double value = integerPart;
            for (double bit = 0.1; char.IsDigit(builder[0]); bit *= 0.1, builder.Remove(0, 1)) value = value + (builder[0] - '0') * bit;

            if (char.ToLower(builder[0], CultureInfo.CurrentCulture) == 'e')
            {
                if (!char.IsDigit(builder[1]) && !((builder[1] == '-' || builder[1] == '+') && char.IsDigit(builder[2]))) throw new ParseException("bad number");
                builder.Remove(0, 1);
                return MatchExponent(builder, currentLine, value);
            }

            return new FloatToken(currentLine, value);
        }

        private static Token MatchExponent(StringBuilder builder, int currentLine, double original)
        {
            long exponent = 0, sign = 1;
            if (!char.IsDigit(builder[0]))
            {
                if (builder[0] == '-') sign = -1;
                builder.Remove(0, 1);
            }
            
            for (; char.IsDigit(builder[0]); builder.Remove(0, 1)) exponent = exponent * 10 + builder[0] - '0';
            return new FloatToken(currentLine, original * Math.Pow(10, exponent * sign));
        }

        private static Token MatchString(StringBuilder builder, int currentLine)
        {
            builder.Remove(0, 1);
            StringBuilder convertString = new StringBuilder();

            for (bool escape = false; escape || builder[0] != '"';)
            {
                if (!escape && builder[0] == '\\')
                {
                    escape = true;
                    builder.Remove(0, 1);
                    continue;
                }

                if (builder[0] == '\0') throw new ParseException("bad string");

                if (escape)
                {
                    if (_escapeChars.Keys.Contains(builder[0]))
                    {
                        convertString.Append(_escapeChars[builder[0]]);
                        builder.Remove(0, 1);
                    }
                    else if (builder[0] == 'u')
                    {
                        builder.Remove(0, 1);
                        int unicode = 0;

                        for (int i = 0; i < 4; i++)
                        {
                            try
                            {
                                int k = ConvertChar(builder[i]);
                                if (k >= 16) throw new ParseException();
                                unicode = unicode * 16 + k;
                            }
                            catch (ParseException) { throw new ParseException("bad unicode char"); }
                        }

                        convertString.Append((char)unicode);
                        builder.Remove(0, 4);
                    }
                    else throw new ParseException("bad escape char '" + builder[0] + "'");

                    escape = false;
                }
                else
                {
                    convertString.Append(builder[0]);
                    builder.Remove(0, 1);
                }
            }

            builder.Remove(0, 1);
            return new StringToken(currentLine, convertString.ToString());
        }

        private static Token MatchRawString(StringBuilder builder, int currentLine, out string leftString)
        {
            leftString = builder.ToString().Substring(0, builder.Length - 5);
            builder.Remove(0, 2);

            StringBuilder convertString = new StringBuilder();

            while (builder[0] != '"')
            {
                if (builder[0] == '\0') return null;

                if (builder[0] == '$')
                {
                    if (_rawEscapeChars.Contains(builder[1]))
                    {
                        convertString.Append(builder[1]);
                        builder.Remove(0, 2);
                    }
                    else throw new ParseException("bad escape char");
                }
                else
                {
                    convertString.Append(builder[0]);
                    builder.Remove(0, 1);
                }
            }

            leftString = "";
            builder.Remove(0, 1);
            return new StringToken(currentLine, convertString.ToString());
        }

        private static Token MatchComplexName(StringBuilder builder, int currentLine)
        {
            builder.Remove(0, 1);
            StringBuilder name = new StringBuilder();

            while (builder[0] != '\'')
            {
                if (builder[0] == '\0') throw new ParseException("bad complex name");
                name.Append(builder[0]);
                builder.Remove(0, 1);
            }

            builder.Remove(0, 1);
            return new NameToken(currentLine, name.ToString());
        }

        private static Token MatchWord(StringBuilder builder, int currentLine)
        {
            StringBuilder word = new StringBuilder().Append(builder[0]);
            builder.Remove(0, 1);

            while (char.IsLetterOrDigit(builder[0]) || builder[0] == '_')
            {
                word.Append(builder[0]);
                builder.Remove(0, 1);
            }

            return new NameToken(currentLine, word.ToString());
        }

        private static Token MatchOperator(StringBuilder builder, int currentLine)
        {
            foreach (string t in _longOperators)
            {
                if (builder.ToString().StartsWith(t, StringComparison.CurrentCultureIgnoreCase))
                {
                    builder.Remove(0, t.Length);
                    return new IdentifierToken(currentLine, t);
                }
            }

            string nextOperator = builder[0].ToString();
            builder.Remove(0, 1);
            return new IdentifierToken(currentLine, nextOperator);
        }

        private static int ConvertChar(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'A' && c <= 'Z') return c - 'A' + 10;
            if (c >= 'a' && c <= 'z') return c - 'a' + 10;
            throw new ParseException("bad number");
        }

        private sealed class IntegerToken : Token
        {
            public override bool IsInteger => true;

            public override long IntegerValue { get; protected set; }

            public override string Text => IntegerValue.ToString(CultureInfo.CurrentCulture);

            public IntegerToken(int line, long value) : base(line) => IntegerValue = value;
        }

        private sealed class FloatToken : Token
        {
            public override bool IsFloat => true;

            public override double FloatValue { get; protected set; }

            public override string Text => FloatValue.ToString(CultureInfo.CurrentCulture);

            public FloatToken(int line, double value) : base(line) => FloatValue = value;
        }

        private sealed class StringToken : Token
        {
            public override bool IsString => true;

            public override string Text { get; protected set; }

            public StringToken(int line, string text) : base(line) => Text = text;
        }

        private sealed class NameToken : Token
        {
            public override bool IsName => true;

            public override bool IsIdentifier => true;

            public override string Text { get; protected set; }

            public NameToken(int line, string identifier) : base(line) => Text = identifier;
        }

        private sealed class IdentifierToken : Token
        {
            public override bool IsIdentifier => true;

            public override string Text { get; protected set; }

            public IdentifierToken(int line, string identifier) : base(line) => Text = identifier;
        }
    }
}
