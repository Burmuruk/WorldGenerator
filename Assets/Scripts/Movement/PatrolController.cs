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
        //[SerializeField] PathFinder<AStar> pathFinder;
        INodeListSupplier nodeList;
        [Space(20)]
        [SerializeField] bool shouldRepeat = false;
        [SerializeField] List<Task> tasks = new List<Task>();
        Movement mover;
        PathFinder finder;

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
        IEnumerator<ISplineNode> enumerator = null;
        PatrolController innerController = null;

        #endregion

        #region public methods
        public Transform NextPoint
        {
            get
            {
                enumerator??= (IEnumerator<ISplineNode>)spline.path.GetEnumerator();

                if (innerController != null)
                {
                    var next = innerController.NextPoint;
                    if (next != null)
                        return next;
                    else
                        innerController = null;
                }

                if (enumerator.MoveNext())
                {
                    if (enumerator.Current.PatrolController)
                    {
                        innerController = enumerator.Current.PatrolController;
                        return innerController.NextPoint;
                    }
                    else

                    return currentPoint = enumerator.Current.Transform;
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

        private void OnEnable()
        {
            if (!mover) return;

            mover.OnFinished += () => Execute_Tasks(shouldRepeat);
        }

        private void OnDisable()
        {
            if (mover)
                mover.OnFinished -= () => Execute_Tasks(shouldRepeat);
        }

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

        public void CreatePatrolWithSpline<T>(Vector3 start, Vector3 end) where T : IPathFinder, new()
        {
            finder.Find_BestRoute<T>((start, end));
            var route = finder.BestRoute;
            print("Total nodes!! =>  " + route?.Count);
            //gameObject.AddComponent<Spline>();
        }

        public void Initialize()
        {
            if (spline)
            {
                spline.cyclicType = cyclicType;
                spline.Initialize();
            }
            else
                return;

            foreach (var task in tasks)
            {
                tasksList.Add(actionsList[task.type]);
            }
        }

        public void Execute_Tasks(bool repeat)
        {
            if (currentAction < tasksList.Count && tasksList != null)
            {
                taskValue = tasks[currentAction].value;
                //print(currentAction);
                tasksList[currentAction++].Invoke();
            }
            else if (repeat && tasksList != null && currentAction >= tasksList.Count)
            {
                currentAction = 0;
                Execute_Tasks(repeat);
            }
        } 
        #endregion
    }
}
