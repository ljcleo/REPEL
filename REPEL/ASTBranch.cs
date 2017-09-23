using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace REPEL
{
    public class ASTBranch : IASTNode
    {
        private List<IASTNode> _children;

        public ASTBranch(Collection<IASTNode> children) { _children = new List<IASTNode>(children); }

        public void Add(IASTNode node) { _children.Add(node); }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(");

            string sep = "";
            foreach (var node in _children)
            {
                builder.Append(sep);
                sep = " ";
                builder.Append(node.ToString());
            }

            return builder.Append(")").ToString();
        }

        public IASTNode this[int index] => _children[index];

        public IEnumerator<IASTNode> Children => _children.GetEnumerator();

        public int Count => _children.Count;

        public string Location
        {
            get
            {
                foreach (IASTNode node in _children) if (node.Location != null) return node.Location;
                return null;
            }
        }

        public virtual void Lookup(Symbols sym) { foreach (var node in this) node.Lookup(sym); }

        public virtual object Evaluate(Environment env) => throw new InternalException("cannot evaluate: " + ToString(), this);

        public IEnumerator<IASTNode> GetEnumerator() => Children;

        IEnumerator IEnumerable.GetEnumerator() => Children;
    }
}
