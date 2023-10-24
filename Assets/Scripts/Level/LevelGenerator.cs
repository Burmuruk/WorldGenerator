using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using static UnityEditor.Progress;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] PieceCollection PieceData;
    [SerializeField] Piece[,] pieces;
    [SerializeField] Vector2Int size = new Vector2Int(10, 10);
    //Piece piece = default;

    readonly Vector2Int[] directions =
    {
        new Vector2Int(1, -1),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, -1),
    };

    readonly Dictionary<SideType, Probs<SideType>> probs = new()
    {
        { SideType.Grass, new(SideType.Grass, (SideType.Water, .001f), (SideType.Dust, .25f)) },
        { SideType.Water, new(SideType.Water, (SideType.Grass, .15f)) },
        { SideType.Dust, new(SideType.Dust, (SideType.Grass, .15f)) },
    };

    readonly Dictionary<ToppingType, Probs<ToppingType>> toppingProbs = new()
    {
        { ToppingType.Wood, new(ToppingType.Wood, (ToppingType.Rock, .001f)) }
    };

    struct Probs<T> where T : Enum
    {
        (T type, float prob)[] probs;

        public Probs(T MainType, params (T type, float prob)[] ps)
        {
            List<(T type, float prob)> sorted = new();
            float mainVal = 0;
            foreach (var value in ps)
            {
                sorted.Add(value);
                mainVal += value.prob;
            }

            sorted.Add((MainType, 1 - mainVal));

            for (int i = 0; i < sorted.Count; i++)
            {
                for (int j = 0; j < sorted.Count; j++)
                {
                    if (sorted[i].prob < sorted[j].prob)
                    {
                        var value1 = sorted[j];

                        sorted[j] = sorted[i];
                        sorted[i] = value1;
                    }
                }
            }

            this.probs = new (T type, float prob)[sorted.Count];
            int curVal = 0;

            for (int i = 0; i < sorted.Count; i++)
            {
                curVal = (int)(sorted[i].prob * 100 + curVal);
                this.probs[i] = ((sorted[i].type, curVal));
            }
        }

        public T GetNextType(int rand)
        {
            T type = probs[0].type;

            float curDis = 0;
            for (int i = 0; i < probs.Length; i++)
            {
                if (rand > curDis && rand <= probs[i].prob)
                {
                    type = probs[i].type;
                    break;
                }

                curDis = probs[i].prob;
            }

            return type;
        }

        public void ResetProbs(T type)
        {
            var ranges = GetRanges();
            int firstIdx = 0;

            for (int i = 0; i < ranges.Length; i++)
            {
                if (probs[i].type.CompareTo(ranges[i].type) == 0)
                    firstIdx = i;
            }

            for (int i = 0, j = 1; i < ranges.Length; i++)
            {
                if (i == firstIdx)
                {
                    probs[0] = (ranges[i].type, ranges[i].prob);
                    continue;
                }

                probs[j] = (ranges[i].type, ranges[i].prob);
                j++;
            }
        }

        public void IncreaseProb(T type, float increasement)
        {
            var ranges = GetRanges();
            int firstIdx = 0;
            float valueToRest = ranges.Length > 1 ? increasement / ranges.Length - 1 : 0;

            for (int i = 0; i < ranges.Length; i++)
            {
                if (probs[i].type.CompareTo(ranges[i].type) == 0)
                    firstIdx = i;
            }

            for (int i = 0, j = 1; i < ranges.Length; i++)
            {
                if (i == firstIdx)
                {
                    probs[0] = (ranges[i].type, ranges[i].prob + increasement);
                    continue;
                }

                probs[j] = (ranges[i].type, ranges[i].prob - valueToRest is var r && r < 0 ? 0 : r);
                j++;
            }
        }

        private (T type, Range range, float prob)[] GetRanges()
        {
            (T type, Range range, float prob)[] probsRange = new (T type, Range range, float prob)[probs.Length];
            float value = 0;

            for (int i = 0; i < probs.Length; i++)
            {
                probsRange[i] = (probs[i].type, new Range((Index)value, (Index)probs[i].prob), probs[i].prob);
                value += probs[i].prob;
            }

            return probsRange;
        }
    }

    private Queue<(Area, (int x, int y))> pendingAreas = new();

    struct Area
    {
        ToppingType toppingType;
        SideType type;
        int size;

        public SideType Type { get => type; }
        public int Size { get => size; }

        public Area (SideType sideType, int size, ToppingType toppingType = ToppingType.None)
        {
            this.toppingType = toppingType;
            this.type = sideType;
            this.size = size;
        }
    }

    private void Start()
    {

        pieces = new Piece[size.x, size.y];
        PieceData.Initialize();

        CreatePiece((0, 0), SideType.None);
        GenerateAreas();
    }

    private void CreatePiece(in (float x, float y) cord, SideType prevSide)
    {

        if (cord.x < 0 || cord.x >= size.x ||
            cord.y < 0 || cord.y >= size.y)
            return;

        ref var cur = ref pieces[(int)cord.x, (int)cord.y];

        if (cur) return;

        var piece = PieceData.GetPiece(TileType.Solid);
        Vector3 curPos = GetOffset(cord);

        cur = piece with
        {
            piece = Instantiate(piece.piece, position: curPos, rotation: Quaternion.identity, parent: transform)
        };

        PieceData.ChangeColor(cur, GetProbability(((int)cord.x, (int)cord.y), prevSide));

        if (cur.Type == SideType.Water) pendingAreas.Enqueue((new Area(SideType.Water, 7), ((int)cord.x, (int)cord.y)));

        for (int i = 0; i < directions.Length; i++)
        {
            var newCords = directions[i];

            Vector2 newPos = new Vector2
            {
                x = cord.x + ((cord.y % 2 == 0 && newCords.y != 0) ? 0 : newCords.x),
                y = cord.y + newCords.y
            };

            CreatePiece((newPos.x, newPos.y), cur.Type);
        }

        static Vector3 GetOffset((float x, float y) cord)
        {
            Vector3 curPos = new Vector3(cord.x * 2, 0, cord.y * -1.75f);

            if ((cord.y % 2 != 0))
                curPos += Vector3.right;
            return curPos;
        }
    }

    private SideType GetProbability(in (int x, int y) cord, SideType type)
    {
        if (type == SideType.None) type = SideType.Grass;

        var probsPerSide = new Dictionary<SideType, int>();
        (SideType type, int prob) best = (SideType.None, int.MaxValue);
        

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int cur = default;

            if (!IsCellValid(cord, i, out cur)) continue;

            var nextType = pieces[cur.x, cur.y].Type;

            //if (pieces[cord.x, cord.y].piece || nextType != pieces[cord.x, cord.y].Type) continue;



            if (!probsPerSide.ContainsKey(nextType))
                probsPerSide.Add(nextType, 1);
            else
                probsPerSide[nextType] = probsPerSide[nextType] + 1;
        }

        foreach (var key in probsPerSide.Keys)
        {
            if (probsPerSide[key] < best.prob)
                best.prob = probsPerSide[key];
        }

        if (best.type == SideType.None) best.type = SideType.Grass;

        return this.probs[best.type].GetNextType(UnityEngine.Random.Range(0, 100));
    }

    private bool IsCellValid(in (int x, int y) cord, int idx, out Vector2Int newPos)
    {
        newPos = new Vector2Int
        {
            x = cord.x + ((cord.y % 2 == 0 && directions[idx].y != 0) ? 0 : directions[idx].x),
            y = cord.y + directions[idx].y
        };

        if (newPos.x < 0 || newPos.x >= size.x ||
        newPos.y < 0 || newPos.y >= size.y)
            return false;

        if (!pieces[newPos.x, newPos.y] || pieces[newPos.x, newPos.y].Type == SideType.None)
            return false;

        return true;
    }

    private int GetOpositeSide(int idx, int max)
    {
        idx += 3;

        if (idx >= max)
            idx -= max;

        return idx;
    }

    private void GetProbability(int x, int y, SideType sideType)
    {
        ref Piece piece = ref pieces[x, y];
        //var sides = new List<(int idx, SideType sideType)>();

        for (int i = 0; i < directions.Length; ++i)
        {
            if (x + directions[i].x is var nX && nX < 0 || nX > size.x ||
                y + directions[i].y is var nY && nY < 0 || nY > size.y)
                continue;

            ref var nextPiece = ref pieces[x + directions[i].x, y + directions[i].y];



            //if (nextPiece[i] != SideType.None)
            //{
            //    sides.Add((i, nextPiece[i]));
            //}
        }



        //if (sides.Count == 0)
        //{
        //    var rand = UnityEngine.Random.Range(0, 100);
        //    SideType nSideType = SideType.None;
        //    for (int i = GrassProbs.Length - 1; i > 0; i--)
        //    {
        //        if (rand <= GrassProbs[i].prob)
        //        {
        //            nSideType = GrassProbs[i].side;
        //        }
        //    }

        //    //PieceData.GetPiece(TileType.Solid, );
        //}
    }

    private void GenerateAreas()
    {
        while (pendingAreas.Count > 0)
        {
            var curArea = pendingAreas.Dequeue();
            int count = curArea.Item1.Size;

            while (count > 0)
            {
                Piece piece = null;
                var type = GetNextRandomType(curArea.Item1.Type, curArea.Item2, out piece);

                if (type == SideType.None) continue;

                PieceData.ChangeColor(piece, curArea.Item1.Type);
                count--;
            } 
        }
    }

    private SideType GetNextRandomType(SideType type, (int x, int y) pos, out Piece nextPiece)
    {
        (int idx, int count) best = (-1, int.MaxValue);

        nextPiece = null;
        SideType nextSideType = SideType.None;
        int rand = UnityEngine.Random.Range(0, 9);

        if (rand < 3)
        {
            best = (UnityEngine.Random.Range(0, 6), 0);
        }
        else
            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int curDir = default;

                if (!IsCellValid(pos, i, out curDir)) continue;

                var cur = pieces[curDir.x, curDir.y];
                if (cur.Type == type) continue;

                var curCount = 0;

                for (int j = 0; j < cur.types.Length; j++)
                {
                    if (cur.Type == type)
                    {
                        curCount++;
                    }
                }

                if (curCount < best.count)
                    best = (i, curCount);
            }

        probs[type].ResetProbs(type);
        probs[type].IncreaseProb(type, 10);
        nextSideType = probs[type].GetNextType(UnityEngine.Random.Range(0, 100));

        if (best.idx == -1)
            return SideType.None;

        nextPiece = pieces[directions[best.idx].x + pos.x, directions[best.idx].y + pos.y];
        return nextSideType;
    }
}
