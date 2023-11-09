using System;
using System.Collections.Generic;
using UnityEngine;
using WorldG.Patrol;

namespace Coco.AI.PathFinding
{
    public class AStar : IPathFinder
    {
        #region Variables
        IPathNode start;
        IPathNode end;
        bool pathCalculated = false;
        bool endReached = false;

        public LinkedList<IPathNode> shortestPath;
        private Dictionary<uint, Vector3> weights;

        #endregion

        #region Properties
        public bool Calculated { get => pathCalculated; }
        public LinkedList<IPathNode> ShortestPath
        {
            get
            {
                if (pathCalculated)
                    return shortestPath;
                else
                    return null;
            }
        }
        #endregion

        public AStar() { }

        public void SetNodeList(IPathNode[] nodes)
        {
            weights = new Dictionary<uint, Vector3>();

            try
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    weights.Add(nodes[i].ID, nodes[i].Position);
                }
            }
            catch (Exception)
            {
                Debug.LogError("posiciones");
                throw;
            }
        }

        public void ClearWeights() => weights = null;

        public LinkedList<IPathNode> Get_Route(IPathNode start, IPathNode end, out float distance)
        {
            distance = 0.0f;
            RequiredLists lists = new RequiredLists();
            lists.Initialize(start, end);

            try
            {
                Start_Algorithm(ref lists, out distance);
            }
            catch (NullReferenceException e)
            {
                Debug.LogError("path not founded " + e.Message + " id ");
                lists.shortestPath = null;
            }

            return lists.shortestPath;
        }

        public LinkedList<IPathNode> Find_Route(IPathNode start, IPathNode end, out float distance)
        {
            distance = 0;
            RequiredLists lists = new RequiredLists();
            lists.Initialize();
            this.start = start;
            this.end = end;

            Start_Algorithm(ref lists);
            Get_ShortestPath(ref lists, out distance);
            Clear();

            return lists.shortestPath;
        }

        public void Start_Algorithm(ref RequiredLists lists, out float distance)
        {
            distance = 0;
            Clear();

            try
            {
                Start_Algorithm(ref lists);
            }
            catch (Exception)
            {
                shortestPath = null;
                return;
            }

            try
            {
                Get_ShortestPath(ref lists, out distance);
                Debug.Log($"Shortest {lists.shortestPath.First.Value.ID} to {lists.shortestPath.Last.Value.ID}");
            }
            catch (Exception)
            {
                Debug.LogError("shortest obtained");
                throw;
            }
        }

        public void Start_Algorithm(out float distance)
        {
            distance = 0;
            Clear();
            RequiredLists lists = new RequiredLists();

            lists.Initialize();

            Start_Algorithm(ref lists);
            Get_ShortestPath(ref lists, out distance);

            shortestPath = lists.shortestPath;
            pathCalculated = true;
        }

        public void Start_Algorithm(ref RequiredLists lists)
        {
            try
            {
                ref var data = ref lists.data;

                data.Add(lists.start, new NodeData(null));
                IPathNode cur = lists.start;
                bool finished = false;
                lists.endReached = false;
                uint selectedNodesCount = 0;

                do
                {
                    try
                    {
                        (IPathNode node, float weight) minWeight = (null, float.MaxValue);

                        var curData = data[cur];
                        ChangeState(NodeState.Checked, cur, ref data);

                        if (cur.ID == lists.end.ID)
                        {
                            lists.endReached = true;
                            break;
                            //Update_CurrentNode(ref cur, unCheckedNodes.Last.Value, ref curWeight, ref lists);
                        }

                        foreach (var conection in cur.NodeConnections)
                        {
                            try
                            {
                                //if (conection.connectionType != ConnectionType.BIDIMENSIONAL) continue;

                                if (!data.ContainsKey(conection.node))
                                    Start(conection.node, ref lists);

                                if (data[conection.node].state == NodeState.Checked) continue;

                                var next = conection.node;
                                Update_Previous(ref next, cur, ref data);

                                if (data[next].state == NodeState.Unchecked)
                                {
                                    var dis = selectedNodesCount++;
                                    ChangeWeight(dis, next, cur, ref data);
                                    ChangeState(NodeState.Waiting, next, ref data);
                                }

                                float weight =  GetDistance(next.ID, lists.end.ID);

                                if (weight < minWeight.weight)
                                    minWeight = (next, weight);
                            }
                            catch (Exception)
                            {
                                Debug.Log("childs");
                                throw;
                            }
                        }

                        try
                        {
                            if (minWeight.weight != float.MaxValue)
                            {
                                cur = minWeight.node;
                                //Update_CurrentNode(ref cur, minWeight.node, ref curWeight, ref lists);
                            }
                            else if (data[cur].prev != null)
                            {
                                cur = data[cur].prev;
                                //Update_CurrentNode(ref cur, data[cur].prev, ref curWeight, ref lists);
                            }
                            else
                                finished = true;
                        }
                        catch (Exception)
                        {
                            Debug.Log("last ifs");
                            throw;
                        }
                    }
                    catch (Exception)
                    {
                        Debug.Log("Fist in Algorithem");
                        throw;
                    }

                } while (!finished);
            }
            catch (Exception)
            {
                Debug.Log("All a...");

                throw;
            }
        }

        public void Clear()
        {
            pathCalculated = false;
            endReached = false;
            shortestPath = null;
        }

        private void ChangeState(NodeState state, IPathNode cur, ref Dictionary<IPathNode, NodeData> data)
        {
            var node = data[cur];
            node.Set_State(state);

            data[cur] = node;
        }

        private void ChangeWeight(float weight, IPathNode node, IPathNode prev, ref Dictionary<IPathNode, NodeData> data)
        {
            var nodeData = data [node];
            nodeData.Update_Weight(weight, prev);

            data[node] = nodeData;
        }

        void Update_Previous(ref IPathNode cur, in IPathNode prev, ref Dictionary<IPathNode, NodeData> data)
        {
            var hi = data[cur];
            hi.prev = prev;

            data[cur] = hi;
        }

        private void Get_ShortestPath(ref RequiredLists lists, out float distance)
        {
            distance = 0f;
            if (!lists.endReached) return;

            ref var data = ref lists.data;
            ref var shortestPath = ref lists.shortestPath;
            ref var end = ref lists.end;
            ref var start = ref lists.start;

            distance = data[end].weight;
            shortestPath = new LinkedList<IPathNode>();
            IPathNode cur = end;

            while (data[cur].prev.ID != start.ID)
            {
                shortestPath.AddFirst(cur);

                IPathNode next = data[cur].prev;
                if (data[next].prev == null)
                {
                    Debug.LogError($"isNull from {cur.ID} to {next.ID}");
                    shortestPath = null;
                    return;
                }

                cur = data[cur].prev;
            }

            shortestPath.AddFirst(data[cur].prev);
        }

        private void Start(IPathNode node, ref RequiredLists lists)
        {
            lists.data.Add(node, new NodeData(NodeState.Unchecked));
        }

        private float GetDistance(uint idx, uint endIdx)
        {
            return Vector3.Distance(weights[idx], weights[endIdx]);
        }
    }
}
