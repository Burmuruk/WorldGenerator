using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldG.Patrol;

namespace Coco.AI
{
    #region Enums

    public enum ConnectionType
    {
        None,
        BIDIMENSIONAL,
        A_TO_B,
        B_TO_A
    }

    #endregion

    #region Structs

    [System.Serializable]
    public struct NodeConnection
    {
        public ScrNode node;
        public ConnectionType connectionType;
        public float magnitude;

        public NodeConnection(ScrNode current, ScrNode node)
        {
            this.node = node;
            this.connectionType = ConnectionType.None;
            this.magnitude = 0;
            magnitude = DistanceBewtweenNodes(current, node);
        }

        public NodeConnection(ScrNode current, ScrNode node, float magnitude, ConnectionType type = ConnectionType.None) : this(current, node)
        {
            this.connectionType = type;
        }

        private float DistanceBewtweenNodes(ScrNode a, ScrNode b)
        {
            return Vector3.Distance(a.transform.position, b.transform.position);
        }
    }

    #endregion

    public class ScrNode : MonoBehaviour
    {
        #region LocalVariables

        [SerializeField]
        public List<NodeConnection> nodeConnections = new List<NodeConnection>();
        [HideInInspector]
        public uint idx;
        #endregion

        #region IntializationMethods

        public float DistanceBewtweenNodes(in NodeConnection node)
        {
            Vector3 value = node.node.transform.position - transform.position;
            return value.magnitude;
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            foreach (var item in nodeConnections)
            {
                if (item.connectionType == ConnectionType.BIDIMENSIONAL)
                Debug.DrawRay(transform.position, item.node.transform.position - transform.position, Color.blue);
            }
        }

        public void Clear_Connections()
        {
            nodeConnections.Clear();
        }

        public void SetIndex(uint idx) => this.idx = idx;
    }
}