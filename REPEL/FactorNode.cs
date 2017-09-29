using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace REPEL
{
    public class FactorNode : ASTBranch
    {
        private static readonly Dictionary<string, Func<object, object>> _normalFactors = new Dictionary<string, Func<object, object>>()
        {
            { "+", Positive },
            { "-", Negative },
            { "!", Not },
            { "~", Reverse }
        };

        public IASTNode Operand => this[Count - 1];

        public FactorNode(Collection<IASTNode> children) : base(children) { }

        public IASTNode Prefix(int index) => this[Count - index - 2];

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var node in this) builder.Append(node is ASTLeaf ? (node as ASTLeaf).Token.Text : node.ToString());
            return builder.ToString();
        }

        public override object Evaluate(Environment env)
        {
            if (env == null) throw new ArgumentNullException(nameof(env));

            object result = Operand.Evaluate(env);

            for (int i = 0; i < Count - 1; i++)
            {
                string current = (Prefix(i) as ASTLeaf).Token.Text;
                if (_normalFactors.ContainsKey(current)) result = _normalFactors[current](result);
                else throw new InternalException("prefix '" + current + "' has not been implemented");
            }

            return result;
        }

        private static object Positive(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            else if (obj is long) return +(obj as long?);
            else if (obj is double) return +(obj as double?);
            else throw new InterpretException("bad type for '+'");
        }

        private static object Negative(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            else if (obj is long) return -(obj as long?);
            else if (obj is double) return -(obj as double?);
            else throw new InterpretException("bad type for '+'");
        }

        private static object Not(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            else if (obj is long) return (obj as long?) == 0;
            else if (obj is double) return (obj as double?) == 0.0;
            else if (obj is string) return (obj as string) == string.Empty;
            else if (obj is ICollection<object>) return (obj as ICollection<object>).Count == 0;
            else if (obj is Atom) return !(obj as Atom).BoolValue;
            else return true;
        }

        private static object Reverse(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            else if (obj is long) return ~(obj as long?);
            else throw new InterpretException("bad type for '~'");
        }
    }
}
