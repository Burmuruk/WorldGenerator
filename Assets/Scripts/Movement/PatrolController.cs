using Coco.AI.PathFinding;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldG.Patrol
{
    [RequireComponent(typeof(SphereCollider), typeof(Movement))]
    public class PatrolController : MonoBehaviour
    {
        #region Variables
        [SerializeField] Spline spline;
        [SerializeField] CyclicType cyclicType = CyclicType.None;
        [Space(20)]
        [SerializeField] bool shouldRepeat = false;
        [SerializeField] List<Task> tasks = new List<Task>();
        Movement mover;
        IPathFinder finder;

        public enum TaskType
        {
            None,
            Turn,
            Move,
            Wait
        }
        [Serializable]
        public struct Task
        {
            public TaskType type;
            public float value;
        }

        Dictionary<TaskType, Action> actionsList = null;
        List<Action> tasksList = new List<Action>();

        int currentAction = 0;
        Transform currentPoint = default;
        Transform nextPoint = default;
        object taskValue = default;
        IEnumerator<PatrolPoint> enumerator = null;

        #endregion

        #region public methods
        public Transform NextPoint
        {
            get
            {
                if (enumerator == null)
                    enumerator = spline.path.GetEnumerator();

                if (enumerator.MoveNext())
                {
                    return currentPoint = enumerator.Current.gameObject.transform;
                }
                else
                    return default;
            }
        } 
        #endregion

        #region private methods
        private void Awake()
        {
            mover = GetComponent<Movement>();

            if (mover)
                actionsList = new Dictionary<TaskType, Action>()
                {
                    { TaskType.Turn, () => mover.TurnTo((float)taskValue) },
                    { TaskType.Move, () => mover.MoveTo(NextPoint) },
                    { TaskType.Wait, () => Invoke("Execute_Tasks", (float)taskValue)}
                };
        }

        private void Start()
        {
            if (spline)
            {
                spline.cyclicType = cyclicType;
                spline.Initialize();
            }

            foreach (var task in tasks)
            {
                tasksList.Add(actionsList[task.type]);
            }

            Execute_Tasks();
        }

        private void OnEnable()
        {
            if (!mover) return;

            mover.OnFinished += Execute_Tasks;
        }

        private void OnDisable()
        {
            if (mover)
                mover.OnFinished -= Execute_Tasks;
        }

        public void SetTarget(Piece target, List<Piece> roads)
        {

        }

        public void SetSpline(Spline spline) => this.spline = spline;

        private void Execute_Tasks()
        {
            if (currentAction < tasksList.Count && tasksList != null)
            {
                taskValue = tasks[currentAction].value;
                //print(currentAction);
                tasksList[currentAction++].Invoke();
            }
            else if (shouldRepeat && tasksList != null && currentAction >= tasksList.Count)
            {
                currentAction = 0;
                Execute_Tasks();
            }
        } 
        #endregion
    }
}
