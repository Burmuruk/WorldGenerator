using SotomaYorch.Agent.Dijkstra;
using System.Collections.Generic;
using UnityEngine;

namespace Coco.AI.PathFinding
{
    public interface INodeListSupplier
    {
        public Vector3 StartNode { get; }
        public Vector3 EndNode { get; }
        public ScrNode[] Nodes { get; }

        public void SetTarget(Vector3 start, Vector3 end, Vector3[] nodes);

        public ScrNode FindNearestNode(Vector3 start);

        public void CalculateNodesConnections();

        public void Clear();

        public void ClearNodeConnections();
    }
}
