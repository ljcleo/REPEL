using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class ParameterNode : ASTBranch
    {
        public int Type { get; protected set; }

        public IASTNode Name => this[0];

        public IASTNode DefaultValue => this[1];

        public ParameterNode(Collection<IASTNode> children) : this(children, 0) { }

        public ParameterNode(Collection<IASTNode> children, int type) : base(children) => Type = type;

        public override string ToString()
        {
            switch (Type)
            {
                case 0: return Name.ToString() + (DefaultValue is NullNode ? "" : " = " + DefaultValue.ToString());
                case 1: return Name.ToString() + "...";
                case 2: return Name.ToString() + (DefaultValue is NullNode ? "" : " = " + DefaultValue.ToString());
                case 3: return Name.ToString() + "*...";
                default: throw new InternalException("bad parameter type");
            }
        }

        public override object Evaluate(Environment env) => throw new NotImplementedException();
    }
}
