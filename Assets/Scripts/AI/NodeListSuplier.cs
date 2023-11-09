using UnityEngine;


namespace WorldG.Patrol
{
    public class NodeListSuplier : INodeListSupplier
    {
        private Vector3 startNode = default;
        private Vector3 endNode = default;
        IPathNode[] nodes = null;
        private float maxDistance = 2;
        private float maxAngle = 5;
        float pRadious = .5f;
        float height = 1;

        public pState connectionsState = pState.None;

        public Vector3 StartNode => startNode;

        public Vector3 EndNode => endNode;

        public IPathNode[] Nodes => nodes;

        #region Public methods
        public void CalculateNodesConnections()
        {
            if (connectionsState == pState.running || connectionsState == pState.deleting) return;

            connectionsState = pState.running;
            ClearNodeConnections();
            InitializeNodeLists();
        }

        public void Clear() => nodes = null;

        public void ClearNodeConnections()
        {
            if (connectionsState == pState.running || connectionsState == pState.deleting || connectionsState != pState.finished)
                return;

            if (nodes == null || nodes.Length <= 0) return;

            connectionsState = pState.deleting;

            foreach (var node in nodes)
                node.ClearConnections();

            connectionsState = pState.None;
        }

        public IPathNode FindNearestNode(Vector3 start)
        {
            if (nodes == null || nodes.Length <= 0) return default;

            float minDistance = float.MaxValue;
            int? index = -1;

            for (int i = 0; i < nodes.Length; i++)
            {
                if (Vector3.Distance(nodes[i].Position, start) is var d && d < minDistance)
                {
                    minDistance = d;
                    index = i;
                }
            }

            return index.HasValue ? nodes[index.Value] : null;
        }

        public void SetTarget(IPathNode[] nodes, float pRadious = .2f, float maxDistance = 2, float maxAngle = 45, float height = 1)
        {
            this.nodes = nodes;
            this.pRadious = pRadious;
            this.maxDistance = maxDistance;
            this.maxAngle = maxAngle;
            this.height = height;

            CalculateNodesConnections();
        } 
        #endregion

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
                            cur.NodeConnections.Add(
                                new NodeConnection(cur, nodes[j], m, types.a));
                        if (!hitted2)
                            nodes[j].NodeConnections.Add(
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
            float Get_VerticalDifference(IPathNode node, IPathNode cur)
            {
                float dif = 0;

                if (node.Position.y > cur.Position.y)
                {
                    dif = node.Position.y - cur.Position.y;
                }
                else
                    dif = cur.Position.y - node.Position.y;

                return dif;
            }
        }

        private float Get_Magnitud(IPathNode nodeA, IPathNode nodeB) =>
            Vector3.Distance(nodeA.Position, nodeB.Position);

        bool Detect_OjbstaclesBetween(IPathNode nodeA, IPathNode nodeB, out float groundNormal)
        {
            groundNormal = 0;
            RaycastHit[] hit;
            var pointA = nodeA.Position + new Vector3(0, pRadious, 0);
            var pointB = nodeA.Position + new Vector3(0, 2 * pRadious + 1, 0);

            var dir = (nodeB.Position - nodeA.Position);

            hit = Physics.CapsuleCastAll(pointA, pointB, pRadious, dir.normalized, Vector3.Distance(nodeA.Position, nodeB.Position));
            Debug.DrawLine(pointA, pointB);
            Debug.DrawRay(pointA, dir.normalized * Vector3.Distance(nodeA.Position, nodeB.Position));

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
