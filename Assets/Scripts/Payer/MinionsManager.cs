using System.Collections.Generic;
using UnityEngine;
using WorldG.Architecture;
using WorldG.Control;
using WorldG.Patrol;

namespace WorldG.level
{
    public class MinionsManager : MonoBehaviour
    {
        Dictionary<CharacterType, LinkedList<FarmingRecord>> minions;
        List<Minion> selected = new();

        NodeListSuplier roadsConnections;
        List<MyNode> roads = null;
        PoolManager pool;

        public struct FarmingRecord
        {
            int ID;
            public List<Minion> Minions { get; set; }
        }

        private void Awake()
        {
            pool = FindObjectOfType<PoolManager>();
        }

        public void SelectGroup(Building building)
        {
            selected.Clear();

            foreach (Minion minion in building.Minions)
            {
                if (!minion.IsWorking)
                {
                    minion.Select();
                    //print("Selected");
                    selected.Add(minion);
                }
            }
        }

        public void SelectMinion(Minion minion)
        {
            selected.Clear();
            minion.Select();
        }

        public void DeSelect()
        {
            selected.Clear();
        }

        public void SetTarget(ISelectable selectable, object args = null)
        {
            if (selected.Count < 0) return;
            object hi = 2;
            GetRoadsConnections();
            switch (selectable)
            {
                case Resource r:
                    selected.ForEach((m) => m.SetTask(m.SetWork, r.ID));
                    foreach (Minion minion in selected)
                        minion.SetTask(minion.SetWork, hi);
                    break;

                case Building f:
                    selected.ForEach((m) => m.SetTask(m.SetWork, f.ID));
                    break;

                case Minion minion:
                    selected.ForEach((m) => m.SetTask(m.SetWork, minion));
                    break;

                default:
                    selected.ForEach((m) => m.SetTask(m.Move, args));
                    break;
            }
        }

        private void GetRoadsConnections()
        {
            print("Road");
            //if (roads != null && roads.Count > 0) return ;

            roadsConnections = new();
            var pieces = pool.GetRoads();
            //roads = new List<IPathNode>();

            //bool needMore = roads != null && roads.Count <= 0;
            int total = (roads??= new()).Count;
            var cur = pieces.First;
            int i = 0;
            for (; i < pieces.Count; i++)
            {
                GameObject newPoint = null;
                MyNode newNode = null;
                if (i >= total)
                {
                    newPoint = new GameObject("Road" + i, typeof(MyNode));
                    newNode = newPoint.GetComponent<MyNode>();

                    roads.Add(newNode);
                }
                else
                {
                    newPoint = roads[i].gameObject;
                    newNode = newPoint.GetComponent<MyNode>();
                }

                while (newPoint.transform.childCount > 0)
                    Destroy(newPoint.transform.GetChild(0));

                newPoint.transform.position = cur.Value.Prefab.transform.position + Vector3.up * 2;
                newPoint.transform.parent = transform;
                newNode.SetIndex((uint)i);
                
                if (cur.Value.Topping.Patrol)
                {
                    var newPatrol = Instantiate(cur.Value.Topping.Patrol.gameObject, newPoint.transform);
                    NodeListSuplier list = new();
                    list.SetTarget(null, maxAngle: 20);
                    newPatrol.GetComponent<PatrolController>().FindNodes<MyNode>(CyclicType.None, list);
                }
                
                cur = cur.Next;
            }

            if (i < roads.Count)
                while (i < roads.Count)
                    Destroy(roads[i]);

            roadsConnections.SetTarget(roads.ToArray(), maxAngle: 20, maxDistance: 2.5f);

            foreach (var group in pool.Minions.Values)
            {
                if (group != null)
                    foreach (var minion in group)
                        minion.SetConnections(roadsConnections);
            }
        }

        private void Initialize()
        {

        }
    }
}
