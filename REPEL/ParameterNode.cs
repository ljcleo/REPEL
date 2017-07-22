using System;
using System.Collections.ObjectModel;
using System.Text;

namespace REPEL
{
    public class ParameterNode : ASTBranch
    {
        private bool _hasListParam, _hasDictParam;

        public bool HasListParam => _hasListParam;

        public bool HasDictParam => _hasDictParam;

        public ParameterNode(Collection<IASTNode> children) : this(children, false, false) { }

        public ParameterNode(Collection<IASTNode> children, bool hasListParam, bool hasDictParam) : base(children)
        {
            _hasListParam = hasListParam;
            _hasDictParam = hasDictParam;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("{");

            string sep = "";
            for (int i = 0; i < Count; i++)
            {
                builder.Append(sep);
                sep = " ";

                builder.Append(this[i].ToString());
                if (i == Count - 2 && _hasListParam && _hasDictParam) builder.Append("...");
                else if (i == Count - 1)
                {
                    if (_hasDictParam) builder.Append("*...");
                    else if (_hasListParam) builder.Append("...");
                }
            }

            return builder.Append("}").ToString();
        }

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
