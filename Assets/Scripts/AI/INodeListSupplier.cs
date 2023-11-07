using System.Collections.Generic;
using UnityEngine;

namespace WorldG.Patrol
{
    public interface INodeListSupplier
    {
        public Vector3 StartNode { get; }
        public Vector3 EndNode { get; }
        public IPathNode[] Nodes { get; }

        public void SetTarget(Vector3 start, Vector3 end, Vector3[] nodes, float pRadious = .2f);

        public IPathNode FindNearestNode(Vector3 start);

        public void CalculateNodesConnections();

        public void Clear();

        public void ClearNodeConnections();
    }
}
