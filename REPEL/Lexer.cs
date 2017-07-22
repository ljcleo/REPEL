using System.Collections.Generic;
using System.IO;

namespace REPEL
{
    public class Lexer
    {
        private List<Token> _tokens = new List<Token>();
        private TextReader _reader;
        private string _parseString = string.Empty;
        private string _leftString = string.Empty;
        private int _currentLine = 0;
        private bool _eof = false;

        public Lexer(TextReader reader) => _reader = reader;

        public Token Read()
        {
            if (Fill(0))
            {
                Token next = _tokens[0];
                _tokens.RemoveAt(0);
                return next;
            }
            else return Token.EOF;
        }

        public Token Peek(int index) => Fill(index) ? _tokens[index] : Token.EOF;

        private bool Fill(int count)
        {
            for (; _tokens.Count <= count; ReadLine()) if (_eof) return false;
            return true;
        }

        private void ReadLine()
        {
            try { _parseString = _reader.ReadLine(); }
            catch (IOException e) { throw new ParseException(e); }

            if (_parseString == null || _parseString == "\u001a")
            {
                if (_leftString.Length != 0) throw new ParseException("bad EOF");

                _eof = true;
                return;
            }

            ++_currentLine;
            try { _tokens.AddRange(LexicalAutomaton.Match(_currentLine, _leftString + "\n" + _parseString, out _leftString)); }
            catch (ParseException e) { throw new ParseException(e.Message.Length == 0 ? "bad token" : e.Message + " at line " + _currentLine); }
        }
    }
}
