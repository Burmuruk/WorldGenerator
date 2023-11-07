using System.Collections.Generic;
using UnityEngine;

namespace WorldG.Patrol
{
    public interface IPathNode
    {
        public uint ID { get; }
        public Vector3 Position { get; }

        public List<NodeConnection> NodeConnections { get; }

        public float GetDistanceBetweenNodes(in NodeConnection connection);
        
        public void ClearConnections();
    }
}
