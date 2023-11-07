using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldG.Patrol
{
    public enum ConnectionType
    {
        None,
        BIDIMENSIONAL,
        A_TO_B,
        B_TO_A
    }
    [System.Serializable]
    public struct NodeConnection
    {
        public IPathNode node;
        public ConnectionType connectionType;
        public float magnitude;

        public NodeConnection(IPathNode current, IPathNode node)
        {
            this.node = node;
            this.connectionType = ConnectionType.None;
            this.magnitude = 0;
            magnitude = DistanceBewtweenNodes(current, node);
        }

        public NodeConnection(IPathNode current, IPathNode node, float magnitude, ConnectionType type = ConnectionType.None) : this(current, node)
        {
            this.connectionType = type;
        }

        private float DistanceBewtweenNodes(IPathNode a, IPathNode b)
        {
            return Vector3.Distance(a.Position, b.Position);
        }
    }

    public class MyNode : MonoBehaviour, IPathNode, ISplineNode
    {
        [SerializeField]
        public List<NodeConnection> nodeConnections = new List<NodeConnection>();
        [HideInInspector]
        public uint idx;
        public NodeData nodeData = null;
        public static CopyData copyData;

        public ConnectionType ConnectionType => throw new NotImplementedException();

        public uint ID => idx;

        public Transform Transform { get => transform; }
        public List<NodeConnection> NodeConnections { get => nodeConnections; }

        public NodeData NodeData => throw new NotImplementedException();

        public bool IsSelected => throw new NotImplementedException();

        public Vector3 Position { get => transform.position; }

        public Action OnStart { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #region Unity methods
        private void OnDrawGizmosSelected()
        {
            foreach (var item in nodeConnections)
            {
                if (item.connectionType == ConnectionType.BIDIMENSIONAL)
                    Debug.DrawRay(transform.position, item.node.Position - transform.position, Color.blue);
            }
        }
        #endregion

        public void ClearConnections()
        {
            nodeConnections.Clear();
        }

        public float GetDistanceBetween(PatrolPoint a, PatrolPoint b)
        {
            throw new NotImplementedException();
        }

        public float GetDistanceBetweenNodes(in NodeConnection connection)
        {
            Vector3 value = connection.node.Position - transform.position;
            return value.magnitude;
        }

        public void Initilize()
        {
            throw new NotImplementedException();
        }

        public void SetIndex(uint idx) => this.idx = idx;
    }
}
