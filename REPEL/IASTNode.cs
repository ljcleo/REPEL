using System.Collections.Generic;

namespace REPEL
{
    public interface IASTNode : IEnumerable<IASTNode>
    {
        IASTNode this[int index] { get; }

        IEnumerator<IASTNode> Children { get; }

        int Count { get; }

        string Location { get; }

        object Evaluate(Environment env);
    }
}