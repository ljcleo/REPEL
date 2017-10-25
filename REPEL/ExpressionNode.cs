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

        public override void Lookup(Symbols sym)
        {
            string current = (Operator as ASTLeaf).Token.Text;
            if (current[0] == '=')
            {
                if (current != "=" && (current == string.Empty || _normalOperator.ContainsKey(current.Substring(0)))) throw new InternalException("bad assign operator parsed");

                FactorNode leftFactor = Left as FactorNode;
                if (leftFactor == null || !leftFactor.IsAssignable) throw new InterpretException("left part not assignable");

                leftFactor.AssignLookup(sym);
                Right.Lookup(sym);
            }
            else
            {
                Left.Lookup(sym);
                Right.Lookup(sym);
            }
        }

        public override object Evaluate(Environment env)
        {
            if (env == null) throw new ArgumentNullException(nameof(env));

            string current = (Operator as ASTLeaf).Token.Text;
            if (_normalOperator.ContainsKey(current)) return _normalOperator[current](Left.Evaluate(env), Right.Evaluate(env));
            else if (current[current.Length - 1] == '=')
            {
                FactorNode leftFactor = Left as FactorNode;
                if (leftFactor == null || !leftFactor.IsAssignable) throw new InterpretException("left part not assignable");

                object rightValue = null;
                if (current == "=") rightValue = Right.Evaluate(env);
                else if (current.Length > 2 && _normalOperator.ContainsKey(current.Substring(0, current.Length - 1))) rightValue = _normalOperator[current.Substring(0, current.Length - 1)](Left.Evaluate(env), Right.Evaluate(env));

                if (rightValue == null) throw new InternalException("bad assign operator parsed");
                leftFactor.AssignEvaluate(env, rightValue);

                return rightValue;
            }
            else throw new InternalException("operator '" + current + "' has not been implemented");
        }

        private static object Power(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            long? ll = left as long?, rl = right as long?;
            double? lf = left as double?, rf = right as double?;

            if ((ll == null && lf == null) || (rl == null && rf == null)) throw new InterpretException("bad type for '**'");

            if (ll != null && rl != null) return (long)Math.Pow(ll.Value, rl.Value);
            else return Math.Pow((ll ?? lf).Value, (rl ?? rf).Value);
        }

        private static object Multiply(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (right is double)
            {
                if (left is long) return ((left as long?) * (right as double?)).Value;
                else if (left is double) return ((left as double?) * (right as double?)).Value;
                else throw new InterpretException("bad type for '*'");
            }
            else if (right is long)
            {
                long rl = (right as long?).Value;

                if (left is long) return ((left as long?) * rl).Value;
                else if (left is double) return ((left as double?) * rl).Value;
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

            long? ll = left as long?, rl = right as long?;
            double? lf = left as double?, rf = right as double?;
            if ((ll == null && lf == null) || (rl == null && rf == null)) throw new InterpretException("bad type for /");
            
            try { return (ll ?? lf).Value * 1.0 / (rl ?? rf).Value; }
            catch (DivideByZeroException) { throw new InterpretException("cannot divide by 0"); }
        }

        private static object ExactDivide(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            long? ll = left as long?, rl = right as long?;
            double? lf = left as double?, rf = right as double?;
            if ((ll == null && lf == null) || (rl == null && rf == null)) throw new InterpretException("bad type for /");
            
            double modres = Math.IEEERemainder((ll ?? lf).Value, (rl ?? rf).Value);
            if (double.IsNaN(modres)) throw new InterpretException("cannot divide by zero");

            if (ll != null && rl != null) return (ll.Value - (long)modres) / rl.Value;
            else return ((ll ?? lf).Value - modres) / (rl ?? rf).Value;
        }

        private static object Modulo(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            long? ll = left as long?, rl = right as long?;
            double? lf = left as double?, rf = right as double?;
            if ((ll == null && lf == null) || (rl == null && rf == null)) throw new InterpretException("bad type for /");
            
            double modres = Math.IEEERemainder((ll ?? lf).Value, (rl ?? rf).Value);
            if (double.IsNaN(modres)) throw new InterpretException("cannot modulo by zero");

            if (ll != null && rl != null) return (long)modres;
            else return modres;
        }

        private static object Add(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (left is long || left is double)
            {
                long? ll = left as long?, rl = right as long?;
                double? lf = left as double?, rf = right as double?;
                if ((ll == null && lf == null) || (rl == null && rf == null)) throw new InterpretException("bad type for +");
                
                if (ll != null && rl != null) return ll.Value + rl.Value;
                else return (ll ?? lf).Value + (rl ?? rf).Value;
            }
            else if (left is string && right is string) return (left as string) + (right as string);
            else throw new InterpretException("bad type for '+'");
        }

        private static object Subtract(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (left is long || left is double)
            {
                long? ll = left as long?, rl = right as long?;
                double? lf = left as double?, rf = right as double?;
                if ((ll == null && lf == null) || (rl == null && rf == null)) throw new InterpretException("bad type for -");
                
                if (ll != null && rl != null) return ll.Value - rl.Value;
                else return (ll ?? lf).Value - (rl ?? rf).Value;
            }
            else throw new InterpretException("bad type for '-'");
        }

        private static object LeftShift(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (!(left is long) || !(right is long)) throw new InterpretException("bad type for '<<'");

            long rightValue = (right as long?).Value;
            if (rightValue < 0 || rightValue > int.MaxValue) throw new InterpretException("large shift not supported in this version");
            return (left as long?).Value << (int)rightValue;
        }

        private static object RightShift(object left, object right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            if (!(left is long) || !(right is long)) throw new InterpretException("bad type for '<<'");

            long rightValue = (right as long?).Value;
            if (rightValue < 0 || rightValue > int.MaxValue) throw new InterpretException("large shift not supported in this version");
            return (left as long?).Value >> (int)rightValue;
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

            if (left is long || left is double)
            {
                long? ll = left as long?, rl = right as long?;
                double? lf = left as double?, rf = right as double?;
                if ((ll == null && lf == null) || (rl == null && rf == null)) throw new InterpretException("bad type for -");
                
                result = (ll ?? lf).Value == (rl ?? rf).Value;
            }
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

            

            if (left is long || left is double)
            {
                long? ll = left as long?, rl = right as long?;
                double? lf = left as double?, rf = right as double?;
                if ((ll == null && lf == null) || (rl == null && rf == null)) throw new InterpretException("bad type for -");
                
                result = (ll ?? lf).Value < (rl ?? rf).Value;
            }
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
