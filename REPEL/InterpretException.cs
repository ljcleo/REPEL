using System;
using System.Runtime.Serialization;

namespace REPEL
{
    [Serializable]
    public class InterpretException : InvalidOperationException
    {
        public InterpretException() { }

        public InterpretException(string message) : base(message) { }

        public InterpretException(string message, IASTNode node) : base(message + " " + (node == null ? "" : node.Location)) { }

        public InterpretException(string message, Exception innerException) : base(message, innerException) { }

        protected InterpretException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
