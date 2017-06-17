using System;
using System.IO;
using System.Runtime.Serialization;

namespace REPEL
{
    [Serializable]
    public class ParseException : Exception
    {
        public ParseException() { }

        public ParseException(Token token) : this(token, "") { }

        public ParseException(Token token, string message) : base("syntax error around " + (token == null ? "" : token.Location) + ". " + message) { }

        public ParseException(IOException exception) : base("", exception) { }

        public ParseException(string message) : base(message) { }

        public ParseException(string message, Exception innerException) : base(message, innerException) { }

        protected ParseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}