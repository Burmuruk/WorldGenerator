using Coco.AI.PathFinding;
using System;
using UnityEngine;
using WorldG.level;
using WorldG.Patrol;
using static UnityEngine.GraphicsBuffer;

namespace WorldG.Control
{
    public class Minion : MonoBehaviour, ISelectable
{
        [SerializeField] private bool _isSelected;
        [SerializeField] protected bool _isWorking = false;
        [SerializeField] Spline spline;
        protected PatrolController _patrolController;
        protected Movement movement;
        protected LevelGenerator level;
        Action onDeselection;

        [SerializeField] protected bool isMoving = false;

        public bool IsSelected => _isSelected;
        public bool IsWorking
        {
            get
            {
                if (isMoving || _isWorking)
                    return true;

                return false;
            }
        }
        public Action OnDeselection { get => onDeselection; set => onDeselection += value; }

        protected virtual void Awake()
        {
            _patrolController = gameObject.GetComponent<PatrolController>();
            _patrolController.OnFinished += _patrolController.Execute_Tasks;
            
            movement = GetComponent<Movement>();
            level = FindObjectOfType<LevelGenerator>();
        }

        private void Update()
        {
            if (IsSelected)
                Debug.DrawRay(transform.position, Vector3.up * 8, Color.red);
        }

        private void OnDrawGizmos()
        {
            if (_isSelected)
                Debug.DrawRay(transform.position, Vector3.up * 10);
        }

        public void Select()
        {
            if (IsSelected) return;

            _isSelected = true;
            Debug.DrawRay(transform.position, Vector3.up * 10, Color.red, 100);
        }

        public void Deselect()
        {
            _isSelected = false;
        }

        public void SetTask(Action<object> task, object args) => task?.Invoke(args);

        public virtual void SetWork(object args) { }

        public void MoveInRoad(object destiny)
        {
            if (IsWorking) return;
            Vector3 target = (Vector3)destiny;
            Debug.DrawRay(target, Vector3.up * 10, Color.white, 10);

            //StopActions();
            MoveTo(target);
        }

        protected void MoveTo(Vector3 target)
        {
            _patrolController.OnPatrolFinished += StopPath;
            _patrolController.CreatePatrolWithSpline<AStar>(transform.position, target, CyclicType.None);
            isMoving = true;
        }

        public void SetConnections(INodeListSupplier nodeList)
        {
            _patrolController.SetNodeList(nodeList, CyclicType.None);
        }

        protected virtual void MoveToTarget() { }

        private void StopPath()
        {
            isMoving = false;
            _patrolController.OnPatrolFinished -= StopPath;
        }

        protected void StopActions()
        {
            if (!IsWorking) return;

            _patrolController.AbortPatrol();

        }
    }

    public interface ISelectable
    {
        bool IsSelected { get; }
        Action OnDeselection { get; set; }

        void Select();
        void Deselect();
    }
}
