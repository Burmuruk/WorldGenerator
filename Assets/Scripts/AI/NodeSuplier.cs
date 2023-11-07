using SotomaYorch.Agent.Dijkstra;
using System.Collections.Generic;
using UnityEngine;

namespace Coco.AI.PathFinding
{
    public class NodeSuplier : INodeListSupplier
    {
        private Vector3 startNode = default;
        private Vector3 endNode = default;
        ScrNode[] nodes = null;
        private float maxDistance = 2;
        private float maxAngle = 5;
        float pRadious = .5f;

        public pState connectionsState = pState.None;

        public Vector3 StartNode => startNode;

        public Vector3 EndNode => endNode;

        public ScrNode[] Nodes => nodes;

        public void CalculateNodesConnections()
        {
            if (connectionsState == pState.running || connectionsState == pState.deleting) return;

            connectionsState = pState.running;
            ClearNodeConnections();
            InitializeNodeLists();
        }

        private void InitializeNodeLists()
        {
            if (nodes == null || nodes.Length <= 0) return;

            var maxDis = maxDistance / Mathf.Sin(maxAngle * Mathf.PI / 180);

            for (int i = 0; i < nodes.Length; i++)
            {
                var cur = nodes[i];

                for (int j = i + 1; j < nodes.Length; j++)
                {
                    float dif = Get_VerticalDifference(nodes[j], cur);
                    var m = Get_Magnitud(cur, nodes[j]);

                    if ((dif < .001f && m <= maxDistance) || (dif > .001f && m <= maxDis))
                    {
                        float normal1, normal2;
                        //Vector3 hitPos1, hitPos2;

                        bool hitted1 = Detect_OjbstaclesBetween(cur, nodes[j], out normal1);
                        bool hitted2 = Detect_OjbstaclesBetween(nodes[j], cur, out normal2);

                        (ConnectionType a, ConnectionType b) types = Get_Types(hitted1, hitted2);

                        if (!hitted1)
                            cur.nodeConnections.Add(
                                new NodeConnection(cur, nodes[j], m, types.a));
                        if (!hitted2)
                            nodes[j].nodeConnections.Add(
                                new NodeConnection(nodes[j], cur, m, types.b));
                    }
                }
            }

            connectionsState = pState.finished;

            (ConnectionType a, ConnectionType b) Get_Types(bool hitted1, bool hitted2)
            {
                return (hitted1, hitted2) switch
                {
                    (false, false) => (ConnectionType.BIDIMENSIONAL, ConnectionType.BIDIMENSIONAL),
                    (false, true) => (ConnectionType.A_TO_B, ConnectionType.None),
                    (true, false) => (ConnectionType.None, ConnectionType.B_TO_A),
                    _ => (ConnectionType.None, ConnectionType.None),
                };
            }
            float Get_VerticalDifference(ScrNode node, ScrNode cur)
            {
                float dif = 0;

                if (node.transform.position.y > cur.transform.position.y)
                {
                    dif = node.transform.position.y - cur.transform.position.y;
                }
                else
                    dif = cur.transform.position.y - node.transform.position.y;

                return dif;
            }
        }

        public void Clear() => nodes = null;

        public void ClearNodeConnections()
        {
            if (connectionsState == pState.running || connectionsState == pState.deleting || connectionsState != pState.finished) 
                return;

            if (nodes == null || nodes.Length <= 0) return;

            connectionsState = pState.deleting;

            foreach (var node in nodes)
                node.Clear_Connections();

            connectionsState = pState.None;
        }

        public ScrNode FindNearestNode(Vector3 start)
        {
            if (nodes == null || nodes.Length <= 0) return default;

            float minDistance = float.MaxValue;
            int? index = -1;

            for (int i = 0; i < nodes.Length; i++)
            {
                if (Vector3.Distance(nodes[i].transform.position, start) is var d && d < minDistance)
                {
                    minDistance = d;
                    index = i;
                }
            }

            return index.HasValue ? nodes[index.Value] : null;
        }

        public void SetTarget(Vector3 start, Vector3 end, Vector3[] nodes, float pRadious = .2f)
        {
            this.startNode = start;
            this.endNode = end;
            this.nodes = nodes;
            this.pRadious = pRadious;
        }

        private float Get_Magnitud(ScrNode nodeA, ScrNode nodeB) =>
            Vector3.Distance(nodeA.transform.position, nodeB.transform.position);

        bool Detect_OjbstaclesBetween(ScrNode nodeA, ScrNode nodeB, out float groundNormal)
        {
            groundNormal = 0;
            RaycastHit[] hit;
            var pointA = nodeA.transform.position + new Vector3(0, pRadious, 0);
            var pointB = nodeA.transform.position + new Vector3(0, 2 * pRadious + 1, 0);

            var dir = (nodeB.transform.position - nodeA.transform.position);

            hit = Physics.CapsuleCastAll(pointA, pointB, pRadious, dir.normalized, Vector3.Distance(nodeA.transform.position, nodeB.transform.position));
            Debug.DrawLine(pointA, pointB);
            Debug.DrawRay(pointA, dir.normalized * Vector3.Distance(nodeA.transform.position, nodeB.transform.position));

            bool hitted = false;
            for (int k = 0; k < hit.Length; k++)
            {
                if (Vector3.Angle(new Vector3(0, 1, 0), hit[k].normal) is var a && (a < (10) || (a > 89 && a < 90.5)) && a != 0)
                {
                    hitted = true;
                }
                else if (Vector3.Angle(new Vector3(0, 0, hit[k].normal.z), new Vector3(0, 0, 1)) is var a2 && (a2 < maxAngle || a2 == 0))
                {
                    groundNormal = a2;
                }
            }

            return hitted;
        }
    }
}
