using System;
using System.Collections.ObjectModel;

namespace REPEL
{
    public class PrimaryNode : ASTBranch
    {
        public IASTNode Operand => this[0];

        public bool IsAssignable => Count == 1 && Operand is NameNode;

        public PrimaryNode(Collection<IASTNode> children) : base(children) { }

        public IASTNode Suffix(int layer) => this[Count - layer - 1];

        public bool HasSuffix(int layer) => Count - layer > 1;

        public override void Lookup(Symbols sym) => Lookup(sym, 0);

        public void Lookup(Symbols sym, int type)
        {
            if (type < 0 || type > 2) throw new InternalException("bad variable type");

            if (type == 0) base.Lookup(sym);
            else
            {
                if (IsAssignable) (Operand as NameNode).Lookup(sym, type);
                else throw new InterpretException("can only use variable prefix before name");
            }
        }

        public void AssignLookup(Symbols sym) => AssignLookup(sym, 0);

        public void AssignLookup(Symbols sym, int type)
        {
            if (type < 0 || type > 2) throw new InternalException("bad variable type");

            if (IsAssignable) (Operand as NameNode).AssignLookup(sym, type);
            else throw new InterpretException("can only use variable prefix before name");
        }

        public override object Evaluate(Environment env) => Operand.Evaluate(env);

        public void AssignEvaluate(Environment env, object value)
        {
            if (IsAssignable) (Operand as NameNode).AssignEvaluate(env, value);
        }
    }
}
