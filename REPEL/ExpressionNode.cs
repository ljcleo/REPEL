using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace REPEL
{
    public class ExpressionNode : ASTBranch
    {
        private static readonly Dictionary<string, Func<object, object, object>> _normalOperator = new Dictionary<string, Func<object, object, object>>()
        {
            { "**", Power },
            { "*", Multiply },
            { "/", Divide },
            { "\\", ExactDivide },
            { "%", Modulo },
            { "+", Add },
            { "-", Subtract },
            { "<<", LeftShift },
            { ">>", RightShift },
            { "&", BitAnd },
            { "^", BitXor },
            { "|", BitOr },
            { "==", Equal },
            { "!=", Inequal },
            { "<", Lesser },
            { "<=", LesserEqual },
            { ">", Greater },
            { ">=", GreaterEqual },
            { "&&", LogicalAnd },
            { "||", LogicalOr },
            { "and", ObjectAnd },
            { "or", ObjectOr }
        };

        public IASTNode Left => this[0];

        public IASTNode Operator => this[1];

        public IASTNode Right => this[2];

        public ExpressionNode(Collection<IASTNode> children) : base(children) { }

        public static bool GetBoolValue(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            else if (obj is long) return (obj as long?) != 0;
            else if (obj is double) return Math.Abs((obj as double?).Value) >= double.Epsilon;
            else if (obj is string) return (obj as string) != string.Empty;
            else if (obj is Atom) return (obj as Atom).BoolValue;
            else return true;
        }

        public override object Evaluate(Environment env)
        {
            if (env == null) throw new ArgumentNullException(nameof(env));

            string current = (Operator as ASTLeaf).Token.Text;
            if (_normalOperator.ContainsKey(current)) return _normalOperator[current](Left.Evaluate(env), Right.Evaluate(env));
            else if (current[0] == '=')
            {
                if (!(Left as FactorNode).IsAssignable) throw new InterpretException("left factor not assignable");

                object rvalue = null;
                if (current == "=") rvalue = Right.Evaluate(env);
                else if (current.Length > 2 && _normalOperator.ContainsKey(current.Substring(0))) rvalue = _normalOperator[current.Substring(1)](Left.Evaluate(env), Right.Evaluate(env));

                if (rvalue == null) throw new InternalException("bas assign operator parsed");

                return rvalue;
            }
            else throw new InternalException("operator '" + current + "' has not been implemented");
        }

        private static object Power(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (left is long && right is long) return (long)Math.Pow((left as long?).Value, (right as long?).Value);
            else
            {
                double? lf = left as double?, rf = right as double?;
                if (lf == null || rf == null) throw new InterpretException("bad type for '**'");
                return Math.Pow(lf.Value, rf.Value);
            }
        }

        private static object Multiply(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (right is double)
            {
                if (left is long || left is double) return ((left as double?) * (right as double?)).Value;
                else throw new InterpretException("bad type for '*'");
            }
            else if (right is long)
            {
                long rl = (right as long?).Value;

                if (left is long) return ((left as long?) * rl).Value;
                else if (left is double) return ((left as double?) * (right as double?)).Value;
                else if (left is string)
                {
                    if (rl == 0) return String.Empty;

                    StringBuilder builder = new StringBuilder(left as string);
                    for (long i = 1; i < rl; i++) builder.Append(left as string);
                    return builder.ToString();
                }
                else throw new InterpretException("bad type for '*'");
            }
            else throw new InterpretException("bad type for '*'");
        }

        private static object Divide(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (!(left is long || left is double) || !(right is long || right is double)) throw new InterpretException("bad type for /");
            try { return ((left as double?) / (right as double?)).Value; }
            catch (DivideByZeroException) { throw new InterpretException("cannot divide by 0"); }
        }

        private static object ExactDivide(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (!(left is long || left is double) || !(right is long || right is double)) throw new InterpretException("bad type for /");
            try
            {
                if (left is long && right is long) return (((left as long?) - (long)Math.IEEERemainder((left as long?).Value, (right as long?).Value)) / (right as long?)).Value;
                else return (((left as double?) - Math.IEEERemainder((left as double?).Value, (right as double?).Value)) / (right as double?)).Value;
            }
            catch (DivideByZeroException) { throw new InterpretException("cannot divide by 0"); }
        }

        private static object Modulo(object left, object right)
        {
            if (!(left is long || left is double) || !(right is long || right is double)) throw new InterpretException("bad type for /");
            try
            {
                if (left is long && right is long) return (long)Math.IEEERemainder((left as long?).Value, (right as long?).Value);
                else return Math.IEEERemainder((left as double?).Value, (right as double?).Value);
            }
            catch (DivideByZeroException) { throw new InterpretException("cannot divide by 0"); }
        }

        private static object Add(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (left is long)
            {
                if (right is long) return ((left as long?) + (right as long?)).Value;
                else if (right is double) return ((left as double?) + (right as double?)).Value;
                else throw new InterpretException("bad type for '+'");
            }
            else if (left is float)
            {
                if (right is long || right is double) return ((left as double?) + (right as double?)).Value;
                else throw new InterpretException("bad type for '+'");
            }
            else if (left is string && right is string) return (left as string) + (right as string);
            else throw new InterpretException("bad type for '+'");
        }

        private static object Subtract(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (left is long)
            {
                if (right is long) return ((left as long?) - (right as long?)).Value;
                else if (right is double) return ((left as double?) - (right as double?)).Value;
                else throw new InterpretException("bad type for '-'");
            }
            else if (left is float)
            {
                if (right is long || right is double) return ((left as double?) - (right as double?)).Value;
                else throw new InterpretException("bad type for '-'");
            }
            else throw new InterpretException("bad type for '-'");
        }

        private static object LeftShift(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (!(left is long) || !(right is long)) throw new InterpretException("bad type for '<<'");
            return (left as long?).Value << (right as int?).Value;
        }

        private static object RightShift(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (!(left is long) || !(right is long)) throw new InterpretException("bad type for '<<'");
            return (left as long?).Value >> (right as int?).Value;
        }

        private static object BitAnd(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (left is long && right is long) return ((left as long?) & (right as long?)).Value;
            else if (Atom.IsBoolAtom(left) && Atom.IsBoolAtom(right)) return (left as Atom) & (right as Atom);
            else throw new InterpretException("bad type for '&'");
        }

        private static object BitXor(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (left is long && right is long) return ((left as long?) ^ (right as long?)).Value;
            else if (Atom.IsBoolAtom(left) && Atom.IsBoolAtom(right)) return (left as Atom) ^ (right as Atom);
            else throw new InterpretException("bad type for '^'");
        }

        private static object BitOr(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (left is long && right is long) return ((left as long?) | (right as long?)).Value;
            else if (Atom.IsBoolAtom(left) && Atom.IsBoolAtom(right)) return (left as Atom) | (right as Atom);
            else throw new InterpretException("bad type for '|'");
        }

        private static object Equal(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            bool result = false;

            if (left is long || left is double) result = (left as double?) == (right as double?);
            else if (left is string) result = (left as string) == (right as string);
            else result = left.Equals(right);

            return Atom.GetBoolAtom(result);
        }

        private static object Inequal(object left, object right) => !(Equal(left, right) as Atom);

        private static object Lesser(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            bool result = false;

            if ((left is long || left is double) && (right is long || right is double)) result = (left as double?) < (right as double?);
            else if (left is string && right is string) result = (left as string).CompareTo((right as string)) < 0;
            else throw new InterpretException("bad type for <");

            return Atom.GetBoolAtom(result);
        }

        private static object LesserEqual(object left, object right) => !(Lesser(right, left) as Atom);

        private static object Greater(object left, object right) => Lesser(right, left);

        private static object GreaterEqual(object left, object right) => !(Lesser(left, right) as Atom);

        private static object LogicalAnd(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return Atom.GetBoolAtom(GetBoolValue(left) && GetBoolValue(right));
        }

        private static object LogicalOr(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return Atom.GetBoolAtom(GetBoolValue(left) || GetBoolValue(right));
        }

        private static object ObjectAnd(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return GetBoolValue(left) ? right : left;
        }

        private static object ObjectOr(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return GetBoolValue(left) ? left : right;
        }
    }
}
