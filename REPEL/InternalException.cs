using System;
using System.Runtime.Serialization;

namespace REPEL
{
    [Serializable]
    public class InternalException : InvalidOperationException
    {
        public InternalException() { }

        public InternalException(string message) : base(message) { }

        public InternalException(string message, IASTNode node) : base(message + " " + (node == null ? "" : node.Location)) { }

        public InternalException(string message, Exception innerException) : base(message, innerException) { }

        protected InternalException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
