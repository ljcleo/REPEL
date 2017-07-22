using System.Collections.ObjectModel;

namespace REPEL
{
    public class NullNode : ASTBranch
    {
        public NullNode(Collection<IASTNode> children) : base(children) { }
    }
}
