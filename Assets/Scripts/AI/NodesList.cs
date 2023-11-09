
using System.Collections.Generic;
using UnityEngine;
using Coco.AI.PathFinding;
using WorldG.Patrol;

public enum pState
{
    None,
    running,
    finished,
    deleting
}

namespace Coco.AI
{
    public class NodesList : MonoBehaviour
    {
        #region Variables
        [Header("Nodes Settings")]
        [Space]
        [SerializeField]
        float maxDistance = 3;
        [SerializeField]
        float maxAngle = 45;
        [SerializeField]
        bool showChanges = false;
        [SerializeField]

        [Header("Dijkstra Settings")]
        [Space]
        public GameObject startNode;
        bool nearestStart = false;
        [SerializeField]
        public GameObject endNode;
        bool nearestEnd = false;
        [SerializeField]
        bool drawPath = false;

        [Header("Mesh Settings")]
        [Space]
        [SerializeField]
        GameObject Node;
        [SerializeField]
        GameObject x1;
        [SerializeField]
        GameObject x2;
        [SerializeField]
        float minDistance = 3;
        [SerializeField]
        bool createMesh = false;
        [SerializeField]
        bool showMeshZone = false;
        [SerializeField]
        float pRadious = .5f;

        Dijkstra dijkstra;
        uint nodeCount = 0;
        List<(IPathNode node, IPathNode hitPos)> edgesToFix;

        public pState dijkstraState = pState.None;
        public pState meshState = pState.None;
        public pState connectionsState = pState.None;

        private List<IPathNode> nodes = new List<IPathNode>();
        #endregion

        #region Properties
        public float MinDistance { get => minDistance; }
        public bool CreateMesh { get => createMesh; set => createMesh = value; }
        public bool HasDijkstra
        {
            get
            {
                if (dijkstra != null && dijkstra.Calculated)
                    return true;
                else
                    return false;
            }
        }
        public pState DijkstraState { get => dijkstraState; }
        public pState MeshState { get => meshState; set => meshState = value; }
        public pState ConnectionsState { get => connectionsState; }
        public bool AreProcessRunning
        {
            get
            {
                if (dijkstraState == pState.running ||
                    meshState == pState.running ||
                    connectionsState == pState.running)
                    return true;
                else
                    return false;
            }
        }
        public bool AreProcessDeleting
        {
            get
            {
                if (meshState == pState.deleting ||
                    connectionsState == pState.deleting ||
                    dijkstraState == pState.deleting)
                    return true;

                else
                    return false;
            }
        }
        public List<IPathNode> Nodes
        {
            get
            {
                if (connectionsState == pState.finished)
                {
                    if (nodes.Count == 0)
                    {
                        for (int i = 0; i < transform.childCount; i++)
                        {
                            var node = transform.GetChild(i).GetComponent<IPathNode>();

                            if (node != null)
                                nodes.Add(node);
                        }
                    }

                    return nodes;
                }
                else
                    return null;
            }
        }
        #endregion

        #region Unity methods
        private void Start()
        {
            if (!x1 || !x2)
            {
                Debug.LogError("All start nodes are not settled.");
                return;
            }

            //if (createMesh)
            //    Create_PathMesh();
            //InitializeNodeLists();
        }

        private void OnDrawGizmos()
        {
            if (showChanges)
                Draw_Mesh();

            if (showMeshZone)
                Draw_MeshZone();

            if (drawPath && dijkstra != null && dijkstra.Calculated)
                Draw_Dijkstra();
        }
        #endregion

        #region Public methods
        public void Calculate_PathMesh()
        {
            if (!createMesh || AreProcessRunning || AreProcessDeleting) return;

            meshState = pState.running;
            Destroy_Nodes();
            Create_PathMesh();
        }

        public void Calculate_NodesConections()
        {
            if (AreProcessRunning || AreProcessDeleting) return;

            connectionsState = pState.running;
            Clear_NodeConections();
            InitializeNodeLists();
        }

        public void Calculate_Dijkstra()
        {
            if (!startNode || !endNode) return;
            if (AreProcessRunning || AreProcessDeleting) return;

            nearestStart = startNode.GetComponent<IPathNode>() == null ? true : false;
            nearestEnd = endNode.GetComponent<IPathNode>() == null ? true : false;

            (IPathNode start, IPathNode end) = (nearestStart, nearestEnd) switch
            {
                (true, true) => (Find_NearestNode(startNode.transform.position), Find_NearestNode(endNode.transform.position)),
                (true, false) => (Find_NearestNode(startNode.transform.position), endNode.GetComponent<IPathNode>()),
                (false, true) => (startNode.GetComponent<IPathNode>(), Find_NearestNode(endNode.transform.position)),
                _ => (startNode.GetComponent<IPathNode>(), endNode.GetComponent<IPathNode>())
            };

            if (dijkstra != null)
                Clear_Dijkstra();

            dijkstra = new Dijkstra(start, end);
            dijkstraState = pState.running;
            dijkstra.Start_Algorithm(out _);

            Debug.DrawLine(start.Position, start.Position + Vector3.up * 25, Color.red, 10);
            Debug.DrawLine(end.Position, end.Position + Vector3.up * 25, Color.red, 10);

            if (dijkstra.Calculated)
                dijkstraState = pState.finished;
            else
                dijkstraState = pState.None;
        }

        public void Destroy_Nodes()
        {
            if (AreProcessRunning || AreProcessDeleting || meshState != pState.finished) return;

            meshState = pState.deleting;
            var nodes = transform.GetComponentsInChildren<IPathNode>();
            nodeCount = 0;
            this.nodes.Clear();

//            foreach (var node in nodes)
//            {
//#if UNITY_EDITOR
//                DestroyImmediate(node.gameObject);
//                continue;
//#endif

//                Destroy(node.gameObject);
//            }

//            CreateMesh = true;
//            meshState = pState.None;
        }

        public void Clear_NodeConections()
        {
            if (AreProcessRunning || AreProcessDeleting || connectionsState != pState.finished) return;

            connectionsState = pState.deleting;
            var nodes = transform.GetComponentsInChildren<IPathNode>();

            foreach (var node in nodes)
                node.ClearConnections();

            if (HasDijkstra)
                Clear_Dijkstra();

            connectionsState = pState.None;
        }

        public void Clear_Dijkstra()
        {
            if (AreProcessDeleting || AreProcessRunning) return;

            dijkstraState = pState.deleting;
            var nodes = transform.GetComponentsInChildren<IPathNode>();

            if (dijkstra != null)
                dijkstra.Clear();

            dijkstraState = pState.None;
        }

        public IPathFinder Get_PathTo(Vector3 destiny)
        {
            return null;
        }

        public List<IPathNode> Get_Nodes() => nodes;

        #endregion

        #region Connections
        private void InitializeNodeLists()
        {
            IPathNode[] nodes;
            if (this.nodes.Count <= 0)
                nodes = transform.GetComponentsInChildren<IPathNode>();
            else
                nodes = this.nodes.ToArray();
            
            var maxDis = maxDistance / Mathf.Sin(maxAngle * Mathf.PI / 180);
            edgesToFix = new List<(IPathNode node, IPathNode hitPos)>();

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

                        //if (!hitted1 && (normal1 < 45 || normal1 == 0))
                        //{
                        //    //hitted2 = true;
                        //    edgesToFix.Add((cur, hitPos1));
                        //}
                        //if (!hitted2 && (normal2 < 45 || normal2 == 0))
                        //{
                        //    //hitted1 = true;
                        //}

                        (ConnectionType a, ConnectionType b) types = Get_Types(hitted1, hitted2);

                        //if (!hitted1)
                        //    cur.NodeConnections.Add(
                        //        new NodeConnection(cur, nodes[j], m, types.a));
                        //if (!hitted2)
                        //    nodes[j].NodeConnections.Add(
                        //        new NodeConnection(nodes[j], cur, m, types.b));
                    }
                    //else
                    //{
                    //    float dis;
                    //    if (cur.transform.position.z > nodes[j].transform.position.z)
                    //        dis = cur.transform.position.z - nodes[j].transform.position.z;
                    //    else
                    //        dis = nodes[j].transform.position.z - cur.transform.position.z;

                    //    if (dis > maxDis)
                    //        edgesToFix.Add((cur, nodes[j]));
                    //}
                }
            }

            //for (int i = 0; i < edgesToFix.Count; i++)
            //{
            //    Set_OffsetOnEdge(edgesToFix[i].node, edgesToFix[i].hitPos);
            //}

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

        private void Set_OffsetOnEdge(IPathNode a, IPathNode hitPos)
        {
            float distBetween = Vector3.Distance(a.Position, hitPos.Position);
            //float disToHit;
            var direction = hitPos.Position - a.Position;

            RaycastHit hit;
            if (!Physics.Raycast(a.Position, direction, out hit, distBetween) &&
                !Physics.Raycast(hitPos.Position, direction * -1, out hit, distBetween))
                return;

            if (MinDistance < pRadious / 2)
            {

            }
            else if (distBetween < pRadious)
            {

            }
            else
            {
                //var finalPos = direction + direction.normalized * (disToHit - pRadious);
                //transform.position = direction + direction.normalized * (disToHit - pRadious);
                Debug.DrawLine(a.Position, hit.point + Vector3.up * 10, Color.red, 5);
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
                if (Vector3.Angle(new Vector3(0, 1, 0), hit[k].normal) is var a && (a < (10) || (a > 89 && a < 90.5) ) && a != 0)
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
        #endregion

        #region Mesh
        private void Create_PathMesh()
        {
            Fix_InvertedPositions();

            float xIndex = (x2.transform.position - x1.transform.position).x / minDistance;
            float zIndex = (x2.transform.position - x1.transform.position).z / minDistance;
            float height = (x2.transform.position - x1.transform.position).y;



            for (float i = 0; i < Mathf.Abs(xIndex); i += minDistance)
            {
                for (float j = 0; j < Mathf.Abs(zIndex); j += minDistance)
                {
                    var curPosA = new Vector3()
                    {
                        x = x1.transform.position.x + minDistance * i,
                        y = x2.transform.position.y,
                        z = x1.transform.position.z - minDistance * j
                    };

                    //Debug.DrawRay(curPosA, Vector3.down * height, Color.green, 9);
                    Ray hi = new Ray(curPosA, Vector3.down * height);
                    var positions = Detect_Ground(height, hi);

                    if (positions != null)
                        Verify_CapsuleArea(positions);
                }
            }

            CreateMesh = false;
            meshState = pState.finished;
        }

        private void Fix_InvertedPositions()
        {
            if (x2.transform.position.x < x1.transform.position.x)
            {
                var newPos = x1.transform.position;
                x1.transform.position = x2.transform.position;
                x2.transform.position = newPos;
            }
            //if (x2.transform.position.z < x1.transform.position.z)
            //{
            //    var newPos = x1.transform.position;
            //    x1.transform.position = x2.transform.position;
            //    x2.transform.position = newPos;
            //}
        }

        private List<Vector3> Detect_Ground(float height, Ray hi, Vector3 offset = default)
        {
            List<Vector3> nodes = null;
            var offsetRay = hi;
            offsetRay.origin = hi.origin + offset;

            var hits = Physics.RaycastAll(offsetRay, height);

            if (hits != null && hits.Length > 0)
            {
                for (int k = 0; k < hits.Length; k++)
                {
                    var angle = Vector3.Angle(hits[k].normal, Vector3.up);

                    if (angle <= maxAngle)
                    {
                        (nodes ??= new List<Vector3>()).Add(offsetRay.origin + Vector3.down * (hits[k].distance - .1f)/* + Vector3.up * 1.5f*/);

                        offset += Vector3.down * (hits[k].distance + 3);
                        //hi.origin = hi.origin + Vector3.down * (hits[k].distance + 3);


                        var positions = Detect_Ground(height - hits[k].distance - 3, hi, offset);

                        if (positions != null)
                            foreach (var pos in positions)
                            {
                                nodes.Add(pos);
                            }
                    }
                }
            }

            return nodes;
        }

        private void Verify_CapsuleArea(List<Vector3> positions)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                var start = positions[i] + new Vector3(0, .7f, 0);

                if (!Physics.CapsuleCast(start, (start + new Vector3(0, 1, 0)), .5f, new Vector3(0, 1, 0), .1f))
                    Create_Node(positions[i]);
            }
        }

        private void Create_Node(in Vector3 position)
        {
            var newNode = Instantiate(Node, transform);
            newNode.transform.position = position;
            newNode.transform.name = "Node " + nodeCount.ToString();
            var nodeCs = newNode.GetComponent<IPathNode>();
            //nodeCs.SetIndex(nodeCount++);
            nodes.Add(nodeCs);
        }

        private void Draw_MeshZone()
        {
            Vector3 dis = x2.transform.position - x1.transform.position;
            Debug.DrawLine(x1.transform.position, x1.transform.position + Vector3.right * dis.x, Color.red, 2);
            Debug.DrawLine(x1.transform.position, x1.transform.position + Vector3.forward * dis.z, Color.red, 2);
            Debug.DrawLine(x1.transform.position, x1.transform.position + Vector3.up * dis.y, Color.red, 2);

            Debug.DrawLine(x2.transform.position, x2.transform.position + Vector3.left * dis.x, Color.red, 2);
            Debug.DrawLine(x2.transform.position, x2.transform.position + Vector3.back * dis.z, Color.red, 2);
            Debug.DrawLine(x2.transform.position, x2.transform.position + Vector3.down * dis.y, Color.red, 2);

            var rd = x2.transform.position + Vector3.down * dis.y + Vector3.left * dis.x;
            Debug.DrawLine(rd, rd + Vector3.up * dis.y, Color.red, 2);
            Debug.DrawLine(rd, rd + Vector3.right * dis.x, Color.red, 2);
            Debug.DrawLine(rd + Vector3.up * dis.y, rd + Vector3.up * dis.y + Vector3.back * dis.z, Color.red, 2);

            var ld = x1.transform.position + Vector3.up * dis.y + Vector3.right * dis.x;
            Debug.DrawLine(ld, ld + Vector3.down * dis.y, Color.red, 2);
            Debug.DrawLine(ld, ld + Vector3.left * dis.x, Color.red, 2);
            Debug.DrawLine(ld + Vector3.down * dis.y, ld + Vector3.down * dis.y + Vector3.forward * dis.z, Color.red, 2);
        }

        private void Draw_Mesh()
        {
            var nodes = transform.GetComponentsInChildren<IPathNode>();

            for (int i = 0; i < nodes.Length; i++)
            {
                var cur = nodes[i];

                for (int j = i + 1; j < nodes.Length; j++)
                {
                    if (Get_Magnitud(cur, nodes[j]) is var m && m <= maxDistance)
                    {
                        bool hitted1 = Detect_OjbstaclesBetween(cur, nodes[j], out _);
                        bool hitted2 = Detect_OjbstaclesBetween(nodes[j], cur, out _);

                        if (!hitted1 && !hitted2)
                            Debug.DrawLine(cur.Position + new Vector3(0, 1.5f, 0), (nodes[j].Position) + new Vector3(0, 1.5f, 0), Color.red);
                    }
                }
            }
        }
        #endregion

        #region Dijkstra
        public IPathNode Find_NearestNode(Vector3 start)
        {
            IPathNode[] nodes;
            if (this.nodes.Count <= 0)
                nodes = transform.GetComponentsInChildren<IPathNode>();
            else
                nodes = this.nodes.ToArray();

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

        private void Draw_Dijkstra()
        {
            var path = dijkstra.ShortestPath;
            var node = path.First;

            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(node.Value.Position + Vector3.up, node.Next.Value.Position + Vector3.up, Color.black);

                node = node.Next;
            }
        }
        #endregion
    } 
}