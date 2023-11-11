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

    public class PatrolPath<T> : ICollection<T> where T : ISplineNode
    {
        #region Variables
        [SerializeField] LinkedList<T> points = new LinkedList<T>();

        private LinkedListNode<T> lastSearched = null;

        private CyclicType cyclicType = CyclicType.None;

        class Enumerator : IEnumerator<T>
        {
            PatrolPath<T> collection;
            int currentIndex = -1;
            CyclicType cyclicType = CyclicType.None;
            LinkedListNode<T> current;
            bool goingRight = true;

            public Enumerator(PatrolPath<T> collection)
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

            T IEnumerator<T>.Current => current.Value;

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
        public int Count => ((ICollection<T>)points).Count;
        public bool IsReadOnly => ((ICollection<T>)points).IsReadOnly;
        public T Last { get => points.Last.Value; }
        public T First { get => points.First.Value; }
        public LinkedListNode<T> FirstNode { get => points.First; }
        public LinkedListNode<T> LastNode { get => points.Last; }
        public T LastSearched { get => lastSearched.Value; }
        public static bool operator true(PatrolPath<T> p) => p != null;
        public static bool operator false(PatrolPath<T> p) => p == null;
        #endregion

        public PatrolPath() { }

        public PatrolPath(CyclicType cyclicType = CyclicType.None, params T[] points)
        {
            foreach (var point in points)
            {
                Add(point);
            }

            this.cyclicType = cyclicType;
        }

        public void Initialize(CyclicType cyclicType = CyclicType.None, params T[] points)
        {
            foreach (var point in points)
            {
                Add(point);
            }

            this.cyclicType = cyclicType;
        }

        #region Public methods
        public IEnumerator<T> GetEnumerator()
        {
            using (var enumerator = new Enumerator(this))
            {
                while (enumerator.MoveNext())
                {
                    yield return (T)enumerator.Current;
                }
            }
        }

        //{

        //    return new Enumerator<T>(this);
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

            //return ((IEnumerable<T>)points).GetEnumerator();
        //}

        public void Add(T item)
        {
            points.AddLast(item);
        }

        public bool AddAfter(T node, T item)
        {
            var point = points.Find(node);

            if (point == null) return false;

            points.AddAfter(point, item);

            return true;
        }

        public bool AddBefore(T node, T item)
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

        public bool Contains(T item)
        {
            return points.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)points).CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (points.Remove(item))
            {
                return true;
            }

            return false;
        }

        public T Next(T item)
        {
            if (item)
                lastSearched = points.Find(item);

            return lastSearched.Next.Value;
        }

        public T Prev(T item)
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
