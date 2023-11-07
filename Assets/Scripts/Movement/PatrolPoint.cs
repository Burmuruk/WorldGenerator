using System;
using UnityEngine;

namespace WorldG.Patrol
{
    //[ExecuteInEditMode]
    public class PatrolPoint : MonoBehaviour
    {
        #region variables
        public NodeData nodeData = null;
        public struct CopyData
        {
            public bool wasSelected;
            public PatrolPoint point;

            public CopyData(bool wasSelected)
            {
                this.wasSelected = wasSelected;
                point = null;
            }
        }
        [NonSerialized] public static CopyData copyData;

        public Action<PatrolPoint, PatrolPoint> OnNodeAdded;
        public event Action<PatrolPoint> OnNodeRemoved;
        private float selectionTime = 1f;
        private float currentTime = 0;
        private bool isTiming = false;
        #endregion

        #region properties
        private bool isSelected = false;
        public Vector3 Position { get => transform.position; }
        public Color NodeColor { get => nodeData != null ? nodeData.NodeColor : Color.blue; }
        public float Radius { get => nodeData != null ? nodeData.Radius : .5f; }
        public bool ShouldDraw { get => nodeData != null ? nodeData.ShouldDraw : true; }
        public float VerticalOffset { get => nodeData != null ? nodeData.VerticalOffset : 0; }
        #endregion

        #region overrides
        public static bool operator true(PatrolPoint p) => p != null;

        public static bool operator false(PatrolPoint p) => p == null;
        #endregion

        #region unity methods
        private void Awake()
        {
            if (copyData.wasSelected)
            {
                copyData.point.OnNodeAdded?.Invoke(copyData.point, this);
            }
        }

        private void OnDisable()
        {
            OnNodeRemoved?.Invoke(this);
        }

        private void Update()
        {
            //if (isTiming)
            //{
            //    if (currentTime < selectionTime)
            //        currentTime += Time.deltaTime;
            //    else
            //        Deselect(); 
            //}
        }

        private void LateUpdate()
        {
            //if (isSelected && !isTiming)
            //{
            //    copyData.point = this;
            //    copyData.wasSelected = true;

            //    currentTime = 0;
            //    print("selected " + transform.name);
            //    isTiming = true;
            //}

            //isSelected = false;
        }

        //[DrawGizmo(GizmoType.Pickable & GizmoType.Selected)]
        private void OnDrawGizmos()
        {
            Gizmos.color = NodeColor;
            Gizmos.DrawSphere(transform.position + Vector3.up * (float)VerticalOffset, (float)Radius);
        }

        private void OnDrawGizmosSelected()
        {
            Select();
            isSelected = true;
        }
        #endregion

        #region private methods
        private void Select()
        {
            copyData.point = this;
            copyData.wasSelected = true;
        }

        private void Deselect()
        {
            if (copyData.point != this) return;

            copyData.point = null;
            copyData.wasSelected = false;
            isTiming = false;
        } 
        #endregion
    } 
}


