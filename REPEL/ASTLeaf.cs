using System.Collections;
using System.Collections.Generic;

namespace REPEL
{
    public class ASTLeaf : IASTNode
    {
        private static readonly List<IASTNode> Empty = new List<IASTNode>();

        private Token _token;

        public Token Token => _token;

        public ASTLeaf(Token token) => _token = token;

        public override string ToString() => _token.Text;

        public IASTNode this[int index] => Empty[index];

        public IEnumerator<IASTNode> Children => Empty.GetEnumerator();

        public int Count => 0;

        public string Location => "at line " + _token.Line;

        public virtual object Evaluate(Environment env) => throw new InternalException("cannot evaluate: " + ToString(), this);

        public IEnumerator<IASTNode> GetEnumerator() => Children;

        IEnumerator IEnumerable.GetEnumerator() => Children;
    }
}
