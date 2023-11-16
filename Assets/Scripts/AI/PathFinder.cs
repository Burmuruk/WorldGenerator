using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using WorldG.Patrol;

namespace Coco.AI.PathFinding
{
    public enum FinderType
	{
		None,
		Dijkstra,
		AStar
	}

	public class PathFinder
	{
		#region Varibles
		INodeListSupplier nodesList;
		IPathFinder algorithem;
		public List<LinkedList<IPathNode>> paths;
		public List<(int idx, float distance)> routeSizes = null;
		LinkedList<IPathNode>.Enumerator enumerator;
		public int? shorstestNodeIdx = null;
		public int curPath = -1;

		//states
		(IPathNode start, IPathNode end)[] curNodes = null;
		public bool isCalculating = false;
		public event Action OnPathCalculated;

        #endregion

        public Vector3 Start { get => nodesList.StartNode; }
        public Vector3 End { get => nodesList.EndNode; }

        #region public
        public LinkedList<IPathNode> BestRoute { get => isCalculating ? null : paths[routeSizes[curPath].idx]; }

        public int? ShorstestNodeIdx { get => shorstestNodeIdx; }

        public PathFinder()
        {

        }

        public PathFinder(INodeListSupplier nodesList)
        {
            this.nodesList = nodesList;
            paths = new List<LinkedList<IPathNode>>();
            enumerator = default;
        }

        public void SetNodeList(INodeListSupplier nodesList)
        {
            this.nodesList = nodesList;
            paths = new List<LinkedList<IPathNode>>();
            enumerator = default;
        }

        public LinkedList<IPathNode> Get_Route(IPathNode start, IPathNode end, out float distance)
        {
            return algorithem.Get_Route(start, end, out distance);
        }

        public void Find_BestRoute<T>(params (Vector3 start, Vector3 end)[] pairs) where T : IPathFinder, new()
        {
            if (isCalculating) return;
            if (nodesList.Nodes == null) return;

            if (algorithem == null) algorithem = new T();
            algorithem.SetNodeList(nodesList.Nodes);
            shorstestNodeIdx = null;
            curNodes = new (IPathNode start, IPathNode end)[pairs.Length];
            var distances = new List<float>();
            Task<List<float>> task;

            int idx = paths.Count;
            int idxDis = (routeSizes ??= new()).Count;

            for (int i = 0; i < pairs.Length; i++)
            {
                curNodes[i].start = nodesList.FindNearestNode(pairs[i].start);
                curNodes[i].end = nodesList.FindNearestNode(pairs[i].end);
            }

            try
            {
                isCalculating = true;
                task = Task.Run(() => GetAllRoutes(curNodes));

                //task.Wait();
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

        public void GoToEnd<T>(Vector3 start) where T : IPathFinder, new()
        {
            Find_BestRoute<T>((start, nodesList.EndNode));

        }

        public Vector3? GetNextNode()
        {
            if (curPath < 0 || routeSizes.Count <= 0 || routeSizes[curPath].idx < 0) return null;

            if (enumerator.Current == null)
                enumerator = BestRoute.GetEnumerator();

            if (enumerator.MoveNext())
            {
                return enumerator.Current.Position;
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
                if (curNodes[i].end.ID == paths[idx].Last.Value.ID || curNodes[i].end.ID == paths[idx].First.Value.ID)
                {
                    shorstestNodeIdx = i;
                }
            }

            routeSizes.RemoveAt(routeSizes.Count - 1);

            if (curPath < 0)
                curPath = idxDis;

            Debug.Log($"Sorteado index {idxDis} distance {routeSizes[idxDis].distance} to {paths[idx].Last.Value.ID}");

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

        private List<float> GetAllRoutes((IPathNode start, IPathNode end)[] nodes)
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

        private void Get_Path(List<float> distances, IPathNode start, IPathNode end)
        {
            try
            {
                float dis = 0;
                Debug.Log($"start {start.ID} endo {end.ID}");
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
        public LinkedList<IPathNode> unCheckedNodes;
        public Dictionary<IPathNode, NodeData> data;
        public LinkedList<IPathNode> shortestPath;
        public IPathNode start;
        public IPathNode end;
        public bool endReached;

        public void Initialize()
        {
            unCheckedNodes = new LinkedList<IPathNode>();
            data = new Dictionary<IPathNode, NodeData>();
            shortestPath = new LinkedList<IPathNode>();
            endReached = false;
        }

        public void Initialize(IPathNode start, IPathNode end)
        {
            unCheckedNodes = new LinkedList<IPathNode>();
            data = new Dictionary<IPathNode, NodeData>();
            shortestPath = new LinkedList<IPathNode>();
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
        public IPathNode prev;

        public NodeData(IPathNode prev)
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

        public void Update_Weight(float value, IPathNode prev) =>
            (this.weight, this.prev) = (value, prev);

        public void Set_State(NodeState value = NodeState.Checked) =>
            this.state = value;

        public void Deconstructor(out IPathNode node) =>
            node = this.prev;
    }
}

