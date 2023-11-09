using Coco.AI;
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
        List<IPathNode> roads = null;
        PoolManager pool;

        public struct FarmingRecord
        {
            int ID;
            List<Minion> Minions { get; set; }
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
                    print("Selected");
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
            if (roads != null && roads.Count > 0) return ;

            roadsConnections = new();
            var pieces = pool.GetRoads();
            roads = new List<IPathNode>();

            int i = 0;
            foreach (var node in pieces)
            {
                var newPoint = new GameObject("Road" + i, typeof(MyNode));
                newPoint.transform.position = node.Prefab.transform.position + Vector3.up * 2;
                var newNode = newPoint.GetComponent<MyNode>();
                newPoint.transform.parent = transform;

                roads.Add(newNode);
                i++;
            }

            roadsConnections.SetTarget(roads.ToArray(), maxAngle: 20);
        }

        private void Initialize()
        {

        }
    }
}
