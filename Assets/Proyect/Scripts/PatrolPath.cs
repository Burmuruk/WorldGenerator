using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldG.Patrol
{
    public enum CyclicType
    {
        None,
        Circle,
        Backwards
    }

    public class PatrolPath : ICollection<PatrolPoint>
    {
        #region Variables
        [SerializeField] LinkedList<PatrolPoint> points = new LinkedList<PatrolPoint>();

        private LinkedListNode<PatrolPoint> lastSearched = null;

        private CyclicType cyclicType = CyclicType.None;

        class Enumerator : IEnumerator<PatrolPoint>
        {
            PatrolPath collection;
            int currentIndex = -1;
            CyclicType cyclicType = CyclicType.None;
            LinkedListNode<PatrolPoint> current;
            bool goingRight = true;

            public Enumerator(PatrolPath collection)
            {
                this.collection = collection;
                this.cyclicType = collection.cyclicType;
                current = collection.FirstNode;
                goingRight = true;
            }

            public object Current
            {
                get { return current.Value; }
            }

            PatrolPoint IEnumerator<PatrolPoint>.Current => current.Value;

            public void Dispose()
            {
                return;
            }

            public bool MoveNext()
            {
                if (cyclicType == CyclicType.Circle)
                {
                    if (goingRight)
                    {
                        if (currentIndex == -1)
                        {
                            currentIndex++;
                            current = collection.FirstNode;
                        }
                        else if (currentIndex >= collection.Count - 1)
                        {
                            currentIndex = 0;
                            current = collection.FirstNode;
                        }
                        else
                        {
                            currentIndex++;
                            current = current.Next;
                        }
                    }
                    else if (!goingRight)
                    {
                        if (currentIndex <= 0)
                        {
                            currentIndex = collection.Count - 1;
                            current = collection.LastNode;
                        }
                        else
                        {
                            currentIndex--;
                            current = current.Previous;
                        }
                    }
                }
                else if (cyclicType == CyclicType.Backwards)
                {
                    if (goingRight)
                    {
                        if (currentIndex == -1)
                        {
                            currentIndex++;
                            current = collection.FirstNode;
                        }
                        else if (currentIndex >= collection.Count - 1)
                        {
                            currentIndex--;
                            current = current.Previous;
                            goingRight = false;
                        }
                        else
                        {
                            currentIndex++;
                            current = current.Next;
                        }
                    }
                    else if (!goingRight)
                    {
                        if (currentIndex <= 0)
                        {
                            currentIndex++;
                            current = current.Next;
                            goingRight = true;
                        }
                        else
                        {
                            currentIndex--;
                            current = current.Previous;
                        }
                    }
                }
                else if (cyclicType == CyclicType.None)
                {
                    if (goingRight)
                    {
                        if (currentIndex == -1)
                        {
                            currentIndex++;
                            current = collection.FirstNode;
                        }
                        else if (currentIndex >= collection.Count - 1)
                            return false;
                        else
                        {
                            currentIndex++;
                            current = current.Next;
                            return true;
                        }
                    }
                    else if (!goingRight)
                    {
                        if (currentIndex <= 0)
                            return false;
                        else
                        {
                            currentIndex--;
                            current = current.Previous;
                            return true;
                        }
                    }
                }

                return true;
            }

            public void Reset()
            {
                currentIndex = -1;
            }
        }
        #endregion

        #region Properties
        public int Count => ((ICollection<PatrolPoint>)points).Count;
        public bool IsReadOnly => ((ICollection<PatrolPoint>)points).IsReadOnly;
        public PatrolPoint Last { get => points.Last.Value; }
        public PatrolPoint First { get => points.First.Value; }
        public LinkedListNode<PatrolPoint> FirstNode { get => points.First; }
        public LinkedListNode<PatrolPoint> LastNode { get => points.Last; }
        public PatrolPoint LastSearched { get => lastSearched.Value; }
        public static bool operator true(PatrolPath p) => p != null;
        public static bool operator false(PatrolPath p) => p == null; 
        #endregion

        public PatrolPath(CyclicType cyclicType = CyclicType.None, params PatrolPoint[] points)
        {
            foreach (var point in points)
            {
                Add(point);
            }

            this.cyclicType = cyclicType;
        }

        #region Public methods
        public IEnumerator<PatrolPoint> GetEnumerator()
        {
            using (var enumerator = new Enumerator(this))
            {
                while (enumerator.MoveNext())
                {
                    yield return (PatrolPoint)enumerator.Current;
                }
            }
        }

        //{

        //    return new Enumerator<PatrolPoint>(this);
            //foreach (var hi in points)
            //{
            //    yield return hi;
            //}

            //using(var enumerator = GetEnumerator())
            //{
            //    enumerator.MoveNext();
            //    while (true)
            //    {
            //        yield return enumerator.Current;

            //        if (!enumerator.MoveNext())
            //            enumerator.Reset();
            //    }
            //}

            //return ((IEnumerable<PatrolPoint>)points).GetEnumerator();
        //}

        public void Add(PatrolPoint item)
        {
            points.AddLast(item);
        }

        public bool AddAfter(PatrolPoint node, PatrolPoint item)
        {
            var point = points.Find(node);

            if (point == null) return false;

            points.AddAfter(point, item);

            return true;
        }

        public bool AddBefore(PatrolPoint node, PatrolPoint item)
        {
            var point = points.Find(node);

            if (point == null) return false;

            points.AddBefore(point, item);

            return true;
        }

        public void Clear()
        {
            points.Clear();
        }

        public bool Contains(PatrolPoint item)
        {
            return points.Contains(item);
        }

        public void CopyTo(PatrolPoint[] array, int arrayIndex)
        {
            ((ICollection<PatrolPoint>)points).CopyTo(array, arrayIndex);
        }

        public bool Remove(PatrolPoint item)
        {
            if (points.Remove(item))
            {
                return true;
            }

            return false;
        }

        public PatrolPoint Next(PatrolPoint item = null)
        {
            if (item)
                lastSearched = points.Find(item);

            return lastSearched.Next.Value;
        }

        public PatrolPoint Prev(PatrolPoint item = null)
        {
            if (item)
                lastSearched = points.Find(item);

            return lastSearched.Previous.Value;
        } 
        #endregion

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)points).GetEnumerator();
        }
    }
}
