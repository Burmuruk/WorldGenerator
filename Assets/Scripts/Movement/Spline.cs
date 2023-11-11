using System;
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
        [SerializeField] GameObject prefab;
        [SerializeField] NodeData nodeData;
        //[SerializeField] public float verticalOffset = 1;
        //[SerializeField] public float nodeRadius = .5f;
        //[SerializeField] public Color nodeColor = Color.blue;
        [Header("Spline")]
        public bool shouldDraw = true;
        [SerializeField] public CyclicType cyclicType = CyclicType.None;
        [SerializeField, ReadOnly] int nodesCount = 0;
        [SerializeField] Color lineColor = Color.yellow;

        public PatrolPath<MyNode> path { get; private set; }
        private bool isDisable = false;
        #endregion

        #region Unity methods
        private void Awake()
        {
            nodeData = new NodeData(nodeData);
        }

        private void OnEnable()
        {
            //if (!isDisable) return;

            //Initialize();
        }

        private void OnDisable()
        {
            if (path == null || path.Count <= 0) return;

            var point = path.FirstNode;
            while (point.Next != null)
            {
                UnsubscribeTo_Node(point.Value);
                print(point.Value.Transform.name);
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
            //List<NodeData> nodes = new List<NodeData>();
            //List<int> hello;
            if (!shouldDraw) return;

            LinkedListNode<MyNode> node = path.FirstNode;

            for (int i = 0; i < path.Count; i++)
            {
                var cur = node.Value.Position + Vector3.up * nodeData.VerticalOffset;

                if (cyclicType == CyclicType.Circle && node.Value == path.Last)
                    Debug.DrawLine(cur, path.First.Position + Vector3.up * nodeData.VerticalOffset, lineColor);
                else if (node.Value == path.Last)
                    break;
                else
                    Debug.DrawLine(cur, node.Next.Value.Position + Vector3.up * nodeData.VerticalOffset, lineColor);

                node = node.Next;
            }
        }
        #endregion

        #region public methods
        public void Initialize()
        {
            if (path != null && path.Count < 0)
            {
                Set_NodeSettings(transform.GetComponentsInChildren<MyNode>());
                return;
            }

            Get_StartPoints();
        }

        public void Initialize(List<ISplineNode> points)
        {

        }
        #endregion

        #region private methods
        private void Get_StartPoints()
        {
            var points = transform.GetComponentsInChildren<MyNode>();

            SubscribeTo_Node(points);
            Set_NodeSettings(points);

            path = new PatrolPath<MyNode>(cyclicType, points: points);
            nodesCount = path.Count;
        }

        private void SubscribeTo_Node(params MyNode[] points)
        {
            foreach (var point in points)
            {
                point.OnNodeAdded += AddNode;
                point.OnNodeRemoved += (rPoint) => path.Remove(rPoint);
            }
        }

        private void UnsubscribeTo_Node(MyNode point)
        {
            if (point == null) return;

            point.OnNodeAdded -= (a, b) => AddNode((MyNode)a, (MyNode)b);
        }

        private void AddNode(MyNode current, MyNode newPoint)
        {
            if (current == path.Last)
            {
                path.Add(newPoint);
            }
            else
            {
                var prev = path.Prev(current);
                var next = path.Next(current);

                if (prev == null)
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

        private void Set_NodeSettings(params MyNode[] points)
        {
            foreach (var point in points)
            {
                point.SetNodeData(nodeData);
            }
        }
        #endregion
    }

    [Serializable]
    public class NodeData
    {
        [SerializeField] private Color nodeColor = Color.blue;
        [SerializeField] private float radious = .5f;
        [SerializeField] private bool shouldDraw = true;
        [SerializeField] private float verticalOffset = 1;

        public Color NodeColor { get => nodeColor; }
        public float Radius { get => radious; }
        public bool ShouldDraw { get => shouldDraw; }
        public float VerticalOffset { get => verticalOffset; }

        public NodeData(NodeData data)
        {
            (nodeColor, radious, shouldDraw, verticalOffset) = data;
        }

        public void Deconstruct(out Color color, out float radious, out bool shouldDraw, out float vOffset)
        {
            color = nodeColor;
            radious = this.radious;
            shouldDraw = this.shouldDraw;
            vOffset = this.verticalOffset;
        }
    }
}
