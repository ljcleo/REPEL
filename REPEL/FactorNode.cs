using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace REPEL
{
    public class FactorNode : ASTBranch
    {
        private static readonly Dictionary<string, Func<object, object>> _normalPrefix = new Dictionary<string, Func<object, object>>()
        {
            { "+", Positive },
            { "-", Negative },
            { "!", Not },
            { "~", Reverse }
        };

        private static readonly Dictionary<string, Func<object, object>> _selfPrefix = new Dictionary<string, Func<object, object>>()
        {
            { "++", Increase },
            { "--", Decrease }
        };

        private static readonly Dictionary<string, int> _variablePrefix = new Dictionary<string, int>()
        {
            { "@", 1 },
            { "^", 2 }
        };

        public IASTNode Operand => this[Count - 1];

        public bool HasVariablePrefix => Count > 1 && _variablePrefix.ContainsKey((Prefix(0) as ASTLeaf).Token.Text);

        public bool IsAssignable => (Operand as PrimaryNode).IsAssignable && (Count == 1 || (Count == 2 && HasVariablePrefix));

        public FactorNode(Collection<IASTNode> children) : base(children) { }

        public IASTNode Prefix(int index) => this[Count - index - 2];

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var node in this) builder.Append(node is ASTLeaf ? (node as ASTLeaf).Token.Text : node.ToString());
            return builder.ToString();
        }

        public override void Lookup(Symbols sym)
        {
            for (int i = 0; i < Count - 2; i++) if (_variablePrefix.ContainsKey((Prefix(i) as ASTLeaf).Token.Text)) throw new InterpretException("can only use variable prefix before name");
            
            if (HasVariablePrefix)
            {
                if (Operand is PrimaryNode primary) primary.Lookup(sym, _variablePrefix[(Prefix(0) as ASTLeaf).Token.Text]);
                else throw new InternalException("factors must follow primary");
            }
            else Operand.Lookup(sym);
        }

        public void AssignLookup(Symbols sym)
        {
            if (!IsAssignable) throw new InterpretException("factor not assignable");

            if (Operand is PrimaryNode primary) primary.AssignLookup(sym, HasVariablePrefix ? _variablePrefix[(Prefix(0) as ASTLeaf).Token.Text] : 0);
            else throw new InternalException("factors must follow primary");
        }

        public override object Evaluate(Environment env)
        {
            if (env == null) throw new ArgumentNullException(nameof(env));

            object result = Operand.Evaluate(env);
            int type = 0;

            for (int i = 0; i < Count - 1; i++)
            {
                string current = (Prefix(i) as ASTLeaf).Token.Text;

                if (type == 0)
                {
                    if (_variablePrefix.ContainsKey(current)) continue;
                    else type = 1;
                }

                if (type == 1)
                {
                    if (_selfPrefix.ContainsKey(current))
                    {
                        if (!(Operand as PrimaryNode).IsAssignable) throw new InterpretException("can only use self-changing prefix before assignable factor or primary");
                        (Operand as PrimaryNode).AssignEvaluate(env, result = _selfPrefix[current](result));
                        continue;
                    }
                    else type = 2;
                }

                if (_normalPrefix.ContainsKey(current)) result = _normalPrefix[current](result);
                else throw new InternalException("prefix '" + current + "' is used illegally");
            }

            return result;
        }

        public void AssignEvaluate(Environment env, object value)
        {
            if (!IsAssignable) throw new InterpretException("factor not assignable");

            if (Operand is PrimaryNode primary) primary.AssignEvaluate(env, value);
            else throw new InternalException("factors must follow primary");
        }

        private static object Positive(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            if (obj is long) return +(obj as long?).Value;
            else if (obj is double) return +(obj as double?).Value;
            else throw new InterpretException("bad type for '+'");
        }

        private static object Negative(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            if (obj is long) return -(obj as long?).Value;
            else if (obj is double) return -(obj as double?).Value;
            else throw new InterpretException("bad type for '+'");
        }

        private static object Not(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return !Atom.GetBoolAtom(ExpressionNode.GetBoolValue(obj));
        }

        private static object Reverse(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            if (obj is long) return ~(obj as long?).Value;
            else throw new InterpretException("bad type for '~'");
        }

        private static object Increase(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            if (obj is long) return (obj as long?).Value + 1;
            else throw new InterpretException("bad type for '++'");
        }

        private static object Decrease(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            if (obj is long) return (obj as long?).Value - 1;
            else throw new InterpretException("bad type for '--'");
        }
    }
}
