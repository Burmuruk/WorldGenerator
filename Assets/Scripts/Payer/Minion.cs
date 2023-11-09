using System;
using UnityEngine;
using WorldG.Patrol;

namespace WorldG.Control
{
    public class Minion : MonoBehaviour, ISelectable
    {
        [SerializeField]private bool _isSelected;
        [SerializeField] private bool _isWorking = false;
        [SerializeField] Spline spline;
        protected PatrolController _patrolController;
        Action onDeselection;
        protected Movement movement;

        public bool IsSelected => _isSelected;
        public bool IsWorking => _isWorking;
        public Action OnDeselection { get => onDeselection; set => onDeselection += value; }

        private void Awake()
        {
            _patrolController = gameObject.AddComponent<PatrolController>();
            movement = GetComponent<Movement>();
        }

        private void Update()
        {
            if (IsSelected)
                Debug.DrawRay(transform.position, Vector3.up * 8, Color.red);
        }

        public void Select()
        {
            if (IsSelected) return;

            _isSelected = true;
        }

        public void Deselect()
        {
            _isSelected = false;
        }

        public void SetTask(Action<object> task, object args) => task?.Invoke(args);

        public void SetDestiny(Vector3 target)
        {
            //Set target and start      A*      Connections
        }

        public virtual void SetWork(object args) { }

        public void Move(object destiny)
        {
            Vector3 target = (Vector3)destiny;
            Debug.DrawRay(target, target + Vector3.up * 10);

            

            //_patrolController.SetTarget()
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
