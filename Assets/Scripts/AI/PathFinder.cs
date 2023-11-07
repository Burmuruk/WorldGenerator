using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Coco.AI.PathFinding
{
    public enum FinderType
	{
		None,
		Dijkstra,
		AStar
	}

	public class PathFinder<T> where T: IPathFinder, new()
	{
		#region Varibles
		NodesList nodesList;
		T algorithem;
		public List<LinkedList<ScrNode>> paths;
		public List<(int idx, float distance)> routeSizes = null;
		LinkedList<ScrNode>.Enumerator enumerator;
		public int? shorstestNodeIdx = null;
		public int curPath = -1;

		//states
		(ScrNode start, ScrNode end)[] curNodes = null;
		public bool isCalculating = false;
		public event Action OnPathCalculated;

        #endregion

        public Vector3 Start { get => nodesList.startNode.transform.position; }
        public Vector3 End { get => nodesList.endNode.transform.position; }

        #region public
        public LinkedList<ScrNode> BestRoute { get => paths[routeSizes[curPath].idx]; }

        public int? ShorstestNodeIdx { get => shorstestNodeIdx; }

        public PathFinder(NodesList nodesList)
        {
            this.nodesList = nodesList;
            paths = new List<LinkedList<ScrNode>>();
            enumerator = default;
        }

        public LinkedList<ScrNode> Get_Route(ScrNode start, ScrNode end, out float distance)
        {
            return algorithem.Get_Route(start, end, out distance);
        }

        public void Find_BestRoute(params (Vector3 start, Vector3 end)[] pairs)
        {
            if (isCalculating) return;
            if (nodesList.Nodes == null) return;

            if (algorithem == null) algorithem = new();
            algorithem.SetNodeList(nodesList.Nodes);
            shorstestNodeIdx = null;
            curNodes = new (ScrNode start, ScrNode end)[pairs.Length];
            var distances = new List<float>();
            Task<List<float>> task;

            int idx = paths.Count;
            int idxDis = (routeSizes ??= new()).Count;

            for (int i = 0; i < pairs.Length; i++)
            {
                curNodes[i].start = nodesList.Find_NearestNode(pairs[i].start);
                curNodes[i].end = nodesList.Find_NearestNode(pairs[i].end);
            }

            try
            {
                isCalculating = true;
                task = Task.Run(() => GetAllRoutes(curNodes));

                task.Wait();
                var awaiter = task.GetAwaiter();
                awaiter.OnCompleted(() =>
                {
                    var result = awaiter.GetResult();
                    FindShortestPath(result, idx, idxDis);
                    OnPathCalculated?.Invoke();
                });
            }
            catch (AggregateException aex)
            {
                Debug.Log(aex.Message);
            }

            return;
        }

        public void GoToEnd(Vector3 start)
        {
            Find_BestRoute((start, nodesList.endNode.transform.position));

        }

        public Vector3? GetNextNode()
        {
            if (curPath < 0 || routeSizes.Count <= 0 || routeSizes[curPath].idx < 0) return null;

            if (enumerator.Current == null)
                enumerator = BestRoute.GetEnumerator();

            if (enumerator.MoveNext())
            {
                return enumerator.Current.transform.position;
            }

            else if (curPath + 1 < routeSizes.Count)
            {
                RemoveMainPath();
                //curPath++;
                enumerator = BestRoute.GetEnumerator();
                return GetNextNode();
            }

            return null;
        } 
        #endregion

        #region private
        private void FindShortestPath(List<float> distances, int idx, int idxDis)
        {
            if (distances.Count <= 0) return;

            SortElements(distances, idx, idxDis);
            RemoveLongPaths(idx, idxDis, distances.Count - 1);
            shorstestNodeIdx = null;

            for (int i = 0; i < curNodes.Length; i++)
            {
                if (curNodes[i].end.idx == paths[idx].Last.Value.idx || curNodes[i].end.idx == paths[idx].First.Value.idx)
                {
                    shorstestNodeIdx = i;
                }
            }

            routeSizes.RemoveAt(routeSizes.Count - 1);

            if (curPath < 0)
                curPath = idxDis;

            Debug.Log($"Sorteado index {idxDis} distance {routeSizes[idxDis].distance} to {paths[idx].Last.Value.idx}");

            isCalculating = false;

            void SortElements(List<float> distances, int idxPath, int idxDis)
            {
                routeSizes.Add((-1, float.MaxValue));

                for (int i = 0; i < distances.Count; i++)
                {
                    bool added = false;

                    for (int j = idxDis; j < routeSizes.Count; j++)
                    {
                        if (distances[i] < routeSizes[j].distance)
                        {
                            routeSizes.Insert(j, (idxPath + i, distances[i]));
                            added = true;
                            break;
                        }
                    }

                    if (!added)
                        routeSizes.Add((paths.Count - 1, distances[i]));
                }
            }
            void RemoveLongPaths(int idx, int idxDis, int length)
            {
                routeSizes.RemoveRange(idxDis + 1, length);

                paths[idx] = paths[routeSizes[idxDis].idx];
                routeSizes[idxDis] = (idx, routeSizes[idxDis].distance);

                paths.RemoveRange(idx + 1, length);
            }
        }

        private List<float> GetAllRoutes((ScrNode start, ScrNode end)[] nodes)
        {
            Task[] tasks = new Task[nodes.Length];
            List<float> distances = new List<float>();

            for (int i = 0; i < nodes.Length; i++)
            {
                int j = i;
                Task task = Task.Run(() => Get_Path(distances, nodes[j].start, nodes[j].end));
                tasks[i] = task;
            }

            try
            {
                //Debug.Log($"To start {tasks.Length} Threads");
                var notNull = tasks.Where(t => t != null).ToArray();
                Task.WaitAll(notNull);
            }
            catch (AggregateException aex)
            {
                Debug.LogError(aex.InnerException);
            }
            return distances;
        }

        private void RemoveMainPath()
        {
            paths.RemoveAt(0);
            routeSizes.RemoveAt(0);

            for (int i = 0; i < routeSizes.Count; i++)
            {
                routeSizes[i] = (routeSizes[i].idx - 1, routeSizes[i].distance);
            }
        }

        private void Get_Path(List<float> distances, ScrNode start, ScrNode end)
        {
            try
            {
                float dis = 0;
                Debug.Log($"start {start.idx} endo {end.idx}");
                var path = Get_Route(start, end, out dis);

                if (path == null) return;

                lock (distances)
                {
                    distances.Add(dis);
                    paths.Add(path);
                }
            }
            catch (NullReferenceException)
            {
                throw;
            }
        }
        #endregion
    }

    public struct RequiredLists
    {
        public LinkedList<ScrNode> unCheckedNodes;
        public Dictionary<ScrNode, NodeData> data;
        public LinkedList<ScrNode> shortestPath;
        public ScrNode start;
        public ScrNode end;
        public bool endReached;

        public void Initialize()
        {
            unCheckedNodes = new LinkedList<ScrNode>();
            data = new Dictionary<ScrNode, NodeData>();
            shortestPath = new LinkedList<ScrNode>();
            endReached = false;
        }

        public void Initialize(ScrNode start, ScrNode end)
        {
            unCheckedNodes = new LinkedList<ScrNode>();
            data = new Dictionary<ScrNode, NodeData>();
            shortestPath = new LinkedList<ScrNode>();
            this.start = start;
            this.end = end;
            endReached = false;
        }
    }

    public enum NodeState
    {
        Unchecked,
        Waiting,
        Checked,
    }

    public struct NodeData
    {
        public float weight;
        public NodeState state;
        public ScrNode prev;

        public NodeData(ScrNode prev)
        {
            weight = 0;
            state = NodeState.Unchecked;
            this.prev = prev;
        }

        public NodeData(NodeState state)
        {
            weight = 0;
            this.state = state;
            prev = null;
        }

        public void Update_Weight(float value, ScrNode prev) =>
            (this.weight, this.prev) = (value, prev);

        public void Set_State(NodeState value = NodeState.Checked) =>
            this.state = value;

        public void Deconstructor(out ScrNode node) =>
            node = this.prev;
    }
}

