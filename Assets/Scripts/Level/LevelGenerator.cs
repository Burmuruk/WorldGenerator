using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] PieceCollection PieceData;
    [SerializeField] Piece[,] pieces;
    [SerializeField] Vector2Int size = new Vector2Int(10, 10);

    [Space(), Header("Settings")]
    [SerializeField, Range(0, 9)] float randDirProb2 = 3;

    readonly Vector2Int[] directions =
    {
        new Vector2Int(1, -1),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, -1),
    };
    readonly Dictionary<ToppingType, Probabilities<ToppingType>> toppingProbs = new()
    {
        { ToppingType.Wood, new(ToppingType.Wood, (ToppingType.Rock, .001f)) }
    };
    private Queue<(Area, Vector2Int)> pendingAreas = new();

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

        PieceData.ChangeType(cur, GetProbabilityPerSide(((int)cord.x, (int)cord.y), prevSide));

        if (cur.Type != SideType.Grass) 
            pendingAreas.Enqueue((PieceData.GetArea(cur.Type), new((int)cord.x, (int)cord.y)));

        Vector2 newPos = new Vector2(cord.x + 1, cord.y);

        if (newPos.x >= size.x)
        {
            newPos.x = 0;
            newPos.y++;
        }

        if (newPos.y >= size.y) return;

        CreatePiece((newPos.x, newPos.y), cur.Type);

        static Vector3 GetOffset((float x, float y) cord)
        {
            Vector3 curPos = new Vector3(cord.x * 2, 0, cord.y * -1.75f);

            if ((cord.y % 2 != 0))
                curPos += Vector3.right;
            return curPos;
        }
    }

    private SideType GetProbabilityPerSide(in (int x, int y) cord, SideType type)
    {
        if (type == SideType.None) type = SideType.Grass;

        var probsPerSide = new Dictionary<SideType, int>();
        (SideType type, int prob) best = (SideType.None, int.MaxValue);

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int cur = default;
            if (!IsCellValid(new(cord.x, cord.y), i, out cur, out _)) continue;

            var nextType = pieces[cur.x, cur.y].Type;

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

        return this.PieceData.GetProbability(best.type).GetNextType(UnityEngine.Random.Range(0f, 100f));
    }

    private bool IsCellValid(in Vector2Int cord, int idx, out Vector2Int newPos, out Piece piece)
    {
        piece = null;
        newPos = new Vector2Int
        {
            x = cord.x + ((((cord.y % 2 == 0 && directions[idx].x > 0) || (cord.y % 2 != 0 && directions[idx].x < 0))
                         && directions[idx].y != 0) ? 0 : directions[idx].x),
            y = cord.y + directions[idx].y
        };

        if (newPos.x < 0 || newPos.x >= size.x ||
        newPos.y < 0 || newPos.y >= size.y)
            return false;

        if (!pieces[newPos.x, newPos.y] || pieces[newPos.x, newPos.y].Type == SideType.None)
            return false;

        piece = pieces[newPos.x, newPos.y];

        return true;
    }

    private Vector2Int MovePosition(in Vector2Int cord, int idx)
    {
        return new Vector2Int
        {
            x = cord.x + ((((cord.y % 2 == 0 && directions[idx].x > 0) || (cord.y % 2 != 0 && directions[idx].x < 0))
                         && directions[idx].y != 0) ? 0 : directions[idx].x),
            y = cord.y + directions[idx].y
        };
    }

    private int GetOpositeSide(int idx, int max)
    {
        idx += 3;

        if (idx >= max)
            idx -= max;

        return idx;
    }

    private void GenerateAreas()
    {
        while (pendingAreas.Count > 0)
        {
            var curArea = pendingAreas.Dequeue();
            int count = curArea.Item1.RandomSize();
            GetNextRandomType(curArea.Item1.Type, curArea.Item2, ref count);
        }
    }

    private SideType GetNextRandomType(SideType type, in Vector2Int pos, ref int count)
    {
        Piece nextPiece = null;
        if (count <= 0) return SideType.None;

        //(int idx, int count, Vector2Int pos) best = (-1, int.MaxValue, default);

        SideType nextSideType = SideType.None;
        float rand = UnityEngine.Random.Range(0f, 9f);
        Vector2Int newDir = default;

        if (rand < randDirProb2)
        {
            int dirIdx = UnityEngine.Random.Range(0, 6);

            newDir = MovePosition(new(pos.x, pos.y), dirIdx);

            if (!IsCellValid(pos, dirIdx, out newDir, out _) || pieces[newDir.x, newDir.y].Type == type)
            {
                GetNextRandomType(type, new(pos.x, pos.y), ref count);
                return SideType.None;
            }

            nextPiece = pieces[newDir.x, newDir.y];
        }
        else
        {
            //for (int i = 0; i < directions.Length; i++)
            //{
            //    Vector2Int curDir = default;
            //    Piece piece = null;
            //    if (!IsCellValid(pos, i, out curDir, out piece)) continue;

            //    if (piece.Type == type) continue;

            //    var curCount = 0;

            //    for (int j = 0; j < directions.Length; j++)
            //    {
            //        Piece curNeighbour = null;
            //        if (!IsCellValid(new(curDir.x, curDir.y), j, out _, out curNeighbour)) continue;

            //        if (curNeighbour.Type == type)
            //            curCount++;
            //    }

            //    if (curCount > 0 && curCount < best.count)
            //        best = (i, curCount, curDir);
            //}
            if (!FindBestPieceInCircle(type, pos, out nextPiece, out newDir, out _, PieceData.GetArea(type).Size))
                return SideType.None;
        }

        PieceData.GetProbability(type).IncreaseProb(type, 30 / count);
        --count;

        nextSideType = PieceData.GetProbability(type).GetNextType(UnityEngine.Random.Range(0, 100));
        PieceData.ChangeType(nextPiece, nextSideType);
        
        GetNextRandomType(type, new(newDir.x, newDir.y), ref count);

        return nextSideType;
    }

    private bool FindBestPieceInCircle(SideType type, Vector2Int pos, out Piece piece, out Vector2Int newPos, out int times, int deep = 1)
    {
        piece = null;
        newPos = pos;
        times = 0;
        if (deep <= 0) return false;
        
        (Vector2Int pos, int times) besto = (default, int.MaxValue);
        HexCircleMovement circleMove = new HexCircleMovement(size, pos, deep);

        foreach (var curPos in circleMove)
        {
            times = 0;

            foreach (var neighbour in circleMove)
            {
                if (pieces[neighbour.x, neighbour.y].Type == type)
                    times++;
            }

            if (times < besto.times)
                besto = (curPos, times);
        }

        if (deep - 1 > 0)
        {
            (Vector2Int pos, int times) newBest = default;
            FindBestPieceInCircle(type, besto.pos, out _, out besto.pos, out besto.times, deep - 1);

            if (newBest.times > besto.times)
            {
                times = newBest.times;
                newPos = newBest.pos;
            }

            return true;
        }

        if (besto.times > 0)
        {
            times = besto.times;
            newPos = besto.pos;
            return true;
        }

        return false;
    }
}

public class HexCircleMovement : IEnumerable<Vector2Int>
{
    Vector2Int _size;
    Vector2Int _pos;
    int _deep;
    HCMEnumerator _numerator;

    public class HCMEnumerator : IEnumerator<Vector2Int>
    {
        readonly (int x, int y, int times)[] _directions =
        {
            ((1, -1, 1)),
            ((1, 1, 1)),
            ((-1, 1, 1)),
            ((-1, 0, 1)),
            ((-1, -1, 1)),
            ((1, 0, 0)),
        };
        Vector2Int _size;
        Vector2Int _pos;
        int _deep;
        Vector2Int _current;
        int _idx = 0;
        int _curRadius = 0;

        public HCMEnumerator (Vector2Int size, Vector2Int pos, int radius)
        {
            _size = size;
            _pos = pos;
            _deep = radius;
            _current = pos;
            _idx = 0;
            _curRadius = 0;
        }

        public Vector2Int Current => Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            bool isValid = false;

            while (!isValid)
            {
                var newPos = new Vector2Int
                {
                    x = _current.x + ((((_current.y % 2 == 0 && _directions[_idx].x > 0) || (_current.y % 2 != 0 && _directions[_idx].x < 0))
                                 && _directions[_idx].y != 0) ? 0 : _directions[_idx].x),
                    y = _current.y + _directions[_idx].y
                };

                if (_idx >= _directions.Length) return false;

                _current = newPos;
                if (++_curRadius >= _directions[_idx].times + _deep - 1)
                {
                    ++_idx;
                    _curRadius = 0;
                }

                if (newPos.x >= 0 || newPos.x < _size.x ||
                newPos.y >= 0 || newPos.y < _size.y)
                    isValid = true;
            }

            return true;
        }

        public void Reset()
        {
            _current = _pos;
            _idx = 0;
            _curRadius = 0;
        }
    }

    public HexCircleMovement(Vector2Int size, Vector2Int pos, int deep)
    {
        _deep = deep;
        _size = size;
        _pos = pos;
    }

    public IEnumerator<Vector2Int> GetEnumerator()
    {
        return new HCMEnumerator(_size, _pos, _deep);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
