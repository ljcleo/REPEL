using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace REPEL
{
    public class ExpressionNode : ASTBranch
    {
        public IASTNode Left => this[0];

        public IASTNode Operator => this[1];

        public IASTNode Right => this[2];

        public ExpressionNode(Collection<IASTNode> children) : base(children) { }

        public override object Evaluate(Environment env) => throw new NotImplementedException();

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
                if (left is long || left is double) return (left as double?) * (right as double?);
                else throw new InterpretException("bad type for '*'");
            }
            else if (right is long)
            {
                long rl = (right as long?).Value;

                if (left is long || left is double) return (left as double?) * (right as double?);
                else if (left is string)
                {
                    if (rl == 0) return String.Empty;

                    StringBuilder builder = new StringBuilder(left as string);
                    for (long i = 1; i < rl; i++) builder.Append(left as string);
                    return builder.ToString();
                }
                else if (left is ICollection<object>)
                {
                    ICollection<object> lc = left as ICollection<object>;

                    List<object> result = new List<object>();
                    for (long i = 0; i < rl; i++) foreach (var t in lc) result.Add(t);
                    return result;
                }
                else throw new InterpretException("bad type for '*'");
            }
            else throw new InterpretException("bad type for '*'");
        }
    }
}
