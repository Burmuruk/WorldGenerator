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
        { SideType.Grass, new (SideType.Grass, (SideType.Water, .001f), (SideType.Dust, .25f)) } ,
        { SideType.Water, new (SideType.Water, (SideType.Grass, .15f)) },
        { SideType.Dust, new (SideType.Dust, (SideType.Grass, .15f)) },
    };

    readonly Dictionary<ToppingType, Probs<ToppingType>> toppingProbs = new()
    {
        { ToppingType.Wood, new (ToppingType.Wood, (ToppingType.Rock, .001f)) }
    };

    struct Probs <T> where T : Enum
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
    }

    private Queue<(Area, (int x, int y))> pendingAreas = new ();

    struct Area
    {
        ToppingType toppingType;
        SideType type;
        int size;
    }

    private void Start()
    {
        
        pieces = new Piece[size.x, size.y];
        PieceData.Initialize();
        
        CreatePiece((0, 0), SideType.None);
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

        return  this.probs[best.type].GetNextType(UnityEngine.Random.Range(0, 100));
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
        var curArea = pendingAreas.Dequeue();

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int cur = default;
            (int x, int y) nextDir = (curArea.Item2 + directions[i].x), curArea.Item2.y + directions[i].y));

            if (!IsCellValid(curArea.Item2, i, out cur)) continue;

            var nextType = pieces[cur.x, cur.y].Type;
        }
    }
}
