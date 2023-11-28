﻿using Coco.AI.PathFinding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorldG.Patrol
{
    [RequireComponent(typeof(SphereCollider))]
    public class PatrolController : MonoBehaviour
    {
        #region Variables
        [SerializeField] Spline splinePrefab;
        [SerializeField] CyclicType cyclicType = CyclicType.None;
        //[SerializeField] PathFinder<AStar> pathFinder;
        INodeListSupplier nodeList;
        [Space(20)]
        [SerializeField] bool shouldRepeat = false;
        [SerializeField] List<Task> tasks = new List<Task>();
        Movement mover;
        PathFinder finder;
        Spline spline;

        public event Action OnFinished;
        public event Action OnPatrolFinished;

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

        int currentAction = -1;
        Transform currentPoint = default;
        Transform nextPoint = default;
        object taskValue = default;
        IEnumerator<ISplineNode> enumerator = null;
        PatrolController innerController = null;

        #endregion
        public Transform NextPoint
        {
            get
            {
                if (!spline && spline.path == null) return default;

                enumerator??= spline.path.GetEnumerator();

                //if (innerController != null)
                //{
                //    var next = innerController.NextPoint;
                //    if (next != null)
                //        return next;
                //    else
                //        innerController = null;
                //}

                if (enumerator.MoveNext())
                {
                    //if (enumerator.Current.PatrolController)
                    //{
                    //    innerController = enumerator.Current.PatrolController;
                    //    return innerController.NextPoint;
                    //}
                    //else

                    return currentPoint = enumerator.Current.Transform;
                }
                else
                    return default;
            }
        } 
        public Movement Mover { get => mover; set => mover = value; }
        public bool CancelRequested { get; private set; }

        #region private methods
        private void Awake()
        {
            mover = GetComponent<Movement>();

            InitializeTasks();
        }

        private void OnEnable()
        {
            if (mover == null) return;

            mover.OnFinished += Execute_Tasks;
        }

        private void OnDisable()
        {
            if (mover != null)
                mover.OnFinished -= Execute_Tasks;
        }

        #region public methods
        public void SetNodeList(INodeListSupplier nodeList, CyclicType cyclicType)
        {
            this.cyclicType = cyclicType;

            finder = new PathFinder(nodeList);
        }

        public void FindNodes<U>(CyclicType cyclicType, INodeListSupplier nodeList)
            where U : MonoBehaviour, IPathNode, ISplineNode
        {
            this.cyclicType = cyclicType;
            List<IPathNode> nodes = new();

            for (int i = 0; i < transform.childCount; i++)
            {
                var node = transform.GetChild(i).GetComponent<U>();
                if (node)
                    nodes.Add(node);
            }

            nodeList.SetNodes(nodes.ToArray());
            this.finder = new PathFinder(nodeList);
        }

        public void CreatePatrolWithSpline<T>(Vector3 start, Vector3 end, CyclicType cyclicType) where T : IPathFinder, new()
        {
            this.cyclicType = cyclicType;
            finder.OnPathCalculated += () =>
            {
                var route = finder.BestRoute;
                //print("Total nodes!! =>  " + route?.Count);
                enumerator?.Dispose();
                enumerator = null;
                CreateSpline();
            };
            finder.Find_BestRoute<T>((start, end));
        }

        private void CreateSpline()
        {
            var route = finder.BestRoute.ToArray();

            for (int i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).GetComponent<Spline>())
                {
                    Destroy(transform.GetChild(i).gameObject);
                    break;
                }

            var splineGO = Instantiate(splinePrefab, transform.parent.transform);
            var spline = splineGO.GetComponent<Spline>();

            for (int i = 0; i < route.Length; i++)
            {
                var go = new GameObject("Node " + i, typeof(MyNode));
                go.transform.parent = splineGO.transform;
                go.transform.position = route[i].Position;
            }

            if (this.spline) Destroy(this.spline.gameObject);
            this.spline = spline;
            spline.cyclicType = cyclicType;
            Initialize();

            OnFinished?.Invoke();
            //Execute_Tasks();
        }

        public void Initialize()
        {
            if (!spline) spline = transform.GetComponentInChildren<Spline>();
            spline.Initialize();

            tasksList = new();
            if (actionsList == null && !InitializeTasks())
                return;

            foreach (var task in tasks)
            {
                tasksList.Add(actionsList[task.type]);
            }
        }

        public void Execute_Tasks()
        {
            if (CancelRequested) { RestartTasks(); return; }

            currentAction++;
            if (tasksList != null && currentAction < tasksList.Count)
            {
                taskValue = tasks[currentAction].value;
                tasksList[currentAction].Invoke();
                return;
            }
            else if (shouldRepeat && tasksList != null)
            {
                currentAction = -1;
                Execute_Tasks();
                return;
            }

            RestartTasks();
        }  

        public void AbortPatrol () => CancelRequested = true;
        #endregion

        private void RestartTasks()
        {
            currentAction = -1;
            enumerator?.Reset();
            OnPatrolFinished?.Invoke();
            CancelRequested = false;
        }

        private bool InitializeTasks()
        {
            if (!mover) return false;

            actionsList = new Dictionary<TaskType, Action>()
                {
                    { TaskType.Turn, () => mover.TurnTo((float)taskValue) },
                    { TaskType.Move, () => {
                        Transform p = NextPoint;
                        if (p == null) { RestartTasks(); return;}
                        mover.MoveTo(p); } },
                    { TaskType.Wait, () => Invoke("Execute_Tasks", (float)taskValue)}
                };

            return true;
        }
        #endregion
    }
}
