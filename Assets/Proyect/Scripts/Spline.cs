using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace WorldG.Patrol
{
    //[ExecuteInEditMode]
    public class Spline : MonoBehaviour
    {
        #region variables
        [Header("Nodes")]
        [SerializeField] public float verticalOffset = 1;
        [SerializeField] public float nodeRadius = .5f;
        [SerializeField] public Color nodeColor = Color.blue;
        [Header("Spline")]
        public bool shouldDraw = true;
        [SerializeField] public CyclicType cyclicType = CyclicType.None;
        [SerializeField, ReadOnly] int nodesCount = 0;
        [SerializeField] Color lineColor = Color.yellow;

        public PatrolPath path { get; private set; }
        NodeData nodeData;
        private bool isDisable = false;
        #endregion

        #region Unity methods
        private void Awake()
        {
            nodeData = new NodeData(this);
        }

        private void OnEnable()
        {
            if (!isDisable) return;
            
            Initialize();
        }

        private void OnDisable()
        {
            if (path == null || path.Count <= 0) return;

            var point = path.FirstNode;
            while (point.Next != null)
            {
                UnsubscribeTo_Node(point.Value);
                print(point.Value.transform.name);
            }
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        private void OnDrawGizmos()
        {
            if (path != null)
                nodesCount = path.Count;
            else
                nodesCount = 0;

            if (path == null || path.Count <= 1)
                return;
            List<NodeData> nodes = new List<NodeData>();
            //List<int> hello;
            if (!shouldDraw) return;

            LinkedListNode<PatrolPoint> node = path.FirstNode;

            for (int i = 0; i < path.Count; i++)
            {
                var cur = node.Value.Position + Vector3.up * verticalOffset;

                if (cyclicType == CyclicType.Circle && node.Value == path.Last)
                    Debug.DrawLine(cur, path.First.Position + Vector3.up * verticalOffset, lineColor);
                else if (node.Value == path.Last)
                    break;
                else
                    Debug.DrawLine(cur, node.Next.Value.Position + Vector3.up * verticalOffset, lineColor);

                node = node.Next;
            }
        }
        #endregion

        #region public methods
        public void Initialize()
        {
            if (path != null && path.Count < 0)
            {
                Set_NodeSettings(transform.GetComponentsInChildren<PatrolPoint>());
                return;
            }

            Get_StartPoints();
        }
        #endregion

        #region private methods
        private void Get_StartPoints()
        {
            var points = transform.GetComponentsInChildren<PatrolPoint>();

            SubscribeTo_Node(points);
            Set_NodeSettings(points);

            path = new PatrolPath(cyclicType, points: points);
            nodesCount = path.Count;
        }

        private void SubscribeTo_Node(params PatrolPoint[] points)
        {
            foreach (var point in points)
            {
                point.OnNodeAdded += AddNode;
                point.OnNodeRemoved += (rPoint) => path.Remove(rPoint);
            }
        }

        private void UnsubscribeTo_Node(PatrolPoint point)
        {
            if (!point) return;

            point.OnNodeAdded -= AddNode;
        }

        private void AddNode(PatrolPoint current, PatrolPoint newPoint)
        {
            if (current == path.Last)
            {
                path.Add(newPoint);
            }
            else
            {
                var prev = path.Prev(current);
                var next = path.Next(current);

                if (!prev)
                    path.AddAfter(current, newPoint);

                var prevDirection = prev.Position - current.Position;
                var nextDirection = next.Position - current.Position;
                var newDirection = newPoint.Position - current.Position;

                var prevAngle = Vector3.Angle(prevDirection, newDirection);
                var nextAngle = Vector3.Angle(nextDirection, newDirection);
                if (prevAngle > nextAngle)
                    path.AddAfter(current, newPoint);
                else /*if (nextAngle > prevAngle)*/
                    path.AddBefore(current, newPoint);
            }

            Set_NodeSettings(newPoint);
            SubscribeTo_Node(newPoint);
        }

        private void Set_NodeSettings(params PatrolPoint[] points)
        {
            foreach (var point in points)
            {
                point.nodeData = nodeData;
            }
        } 
        #endregion
    }

    public class NodeData
    {
        private Spline spline;

        public NodeData(Spline spline)
        {
            this.spline = spline;
        }

        private Color nodeColor = Color.blue;
        private float radius = .5f;
        private bool shouldDraw = true;
        private float verticalOffset = 1;

        public Color NodeColor { get => spline.nodeColor; }
        public float Radius { get => spline.nodeRadius; }
        public bool ShouldDraw { get => spline.shouldDraw; }
        public float VerticalOffset { get => spline.verticalOffset; }
    }
}
