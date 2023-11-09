using System.Collections.Generic;
using UnityEngine;

namespace WorldG.Patrol
{
    public interface INodeListSupplier
    {
        public Vector3 StartNode { get; }
        public Vector3 EndNode { get; }
        public IPathNode[] Nodes { get; }

        public void SetTarget(IPathNode[] nodes, float pRadious = .2f, float maxDistance = 2, float maxAngle = 45, float height = 1);

        public IPathNode FindNearestNode(Vector3 start);

        public void CalculateNodesConnections();

        public void Clear();

        public void ClearNodeConnections();
    }
}
