using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] PieceCollection PieceData;
    [SerializeField] Piece[,] pieces;
    [SerializeField] Vector2Int size = new Vector2Int(10, 10);

    readonly Vector2Int[] directions =
    {
        new Vector2Int(1, -1),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, -1),
    };
    private Queue<(Area, Vector2Int)> pendingAreas = new();
    
    private void Start()
    {

        pieces = new Piece[size.x, size.y];
        PieceData.Initialize();

        GenerateContent((0, 0), SideType.None);
        CreatePieces();
        GenerateAreas();
        AddToppings();
        GetStartPoint();
    }

    private void GetStartPoint()
    {
        bool isValid = false;

        while (!isValid)
        {
            var randX = UnityEngine.Random.Range(0, size.x);
            var randY = UnityEngine.Random.Range(0, size.y); 
            ref var cur = ref pieces[randX, randY];
            if (cur.Type == SideType.Grass && cur.Topping.Prefab == null)
            {
                //PieceData.ChangeType(cur, )
                PieceData.ChangeColor(cur, SideType.Mudd);
                SetTopping(ref cur, ToppingType.House, 2);
                isValid = true;
            }
        }
    }

    private void GenerateContent(in (float x, float y) cord, SideType prevSide)
    {
        var curCord = cord;

        while (true)
        {
            if (curCord.x < 0 || curCord.x >= size.x ||
            curCord.y < 0 || curCord.y >= size.y)
                return;

            ref var cur = ref pieces[(int)curCord.x, (int)curCord.y];

            if (cur) return;

            var piece = PieceData.GetPiece(TileType.Solid);
            //Vector3 curPos = GetOffset(curCord);

            //cur = piece with
            //{
            //    piece = Instantiate(piece.piece, position: curPos, rotation: Quaternion.identity, parent: transform)
            //};
            cur = piece with { 
                piece = null,
                Type = GetProbabilityPerSide(((int)curCord.x, (int)curCord.y), prevSide) 
                //topping
            };

            //PieceData.ChangeType(cur, GetProbabilityPerSide(((int)curCord.x, (int)curCord.y), prevSide));

            if (cur.Type != SideType.Grass)
                pendingAreas.Enqueue((PieceData.GetArea(cur.Type), new((int)curCord.x, (int)curCord.y)));

            Vector2 newPos = new Vector2(curCord.x + 1, curCord.y);

            if (newPos.x >= size.x)
            {
                newPos.x = 0;
                newPos.y++;
            } 

            if (newPos.y >= size.y) return;

            curCord = (newPos.x, newPos.y);
        }

        //CreatePiece((newPos.x, newPos.y), cur.Type);

    }
    static Vector3 GetOffset((float x, float y) cord)
    {
        Vector3 curPos = new Vector3(cord.x * 2, 0, cord.y * -1.75f);

        if ((cord.y % 2 != 0))
            curPos += Vector3.right;
        return curPos;
    }

    private void CreatePieces()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                ref var cur = ref pieces[i, j];

                Vector3 curPos = GetOffset((i,j));
                cur = cur with
                {
                    piece = Instantiate(PieceData.GetPiece(TileType.Solid).piece, position: curPos, rotation: Quaternion.identity, parent: transform)
                };

                PieceData.ChangeColor(cur, cur.Type);
            }
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
            var size = curArea.Item1.GetRandomSize();
            GetNextRandomType(curArea.Item1.Type, curArea.Item2, size, size);

            PieceData.GetProbability(curArea.Item1.Type).ResetProbs();
        }
    }

    private SideType GetNextRandomType(SideType type, in Vector2Int pos, in int size, int times)
    {
        Piece nextPiece = null;
        if (times <= 0) return SideType.None;

        SideType nextSideType = SideType.None;
        float rand = UnityEngine.Random.Range(0f, 1f);
        Vector2Int newDir = default;

        if (rand < PieceData.GetArea(type).RandomDirection)
        {
            int dirIdx = UnityEngine.Random.Range(0, 6);

            newDir = MovePosition(new(pos.x, pos.y), dirIdx);

            if (!IsCellValid(pos, dirIdx, out newDir, out _) || pieces[newDir.x, newDir.y].Type == type)
            {
                GetNextRandomType(type, new(pos.x, pos.y), in size, times);
                return SideType.None;
            }

            nextPiece = pieces[newDir.x, newDir.y];
        }
        else
        {
            if (!FindBestPieceInCircle(type, pos, out nextPiece, out newDir, out _, PieceData.GetArea(type).DeepSearch))
            {
                return SideType.None;
            }
        }

        PieceData.GetProbability(type).IncreaseProb(type, 30 / times);
        nextSideType = PieceData.GetProbability(type).GetNextType(UnityEngine.Random.Range(0, 100));

        if (nextSideType == type)
            times--;
        
        PieceData.ChangeType(nextPiece, nextSideType);
        //PieceData.ChangeColor(nextPiece, SideType.Dust);
        
        GetNextRandomType(type, new(newDir.x, newDir.y), in size, times);

        return nextSideType;
    }

    private bool FindBestPieceInCircle(SideType type, Vector2Int pos, out Piece piece, out Vector2Int newPos, out int times, int deep = 1)
    {
        piece = null;
        newPos = pos;
        times = 0;
        if (deep <= 0) return false;
        
        (Vector2Int pos, int times) besto = (default, 0);
        HexCircleMovement circleMove;
        HexCircleMovement circleMove2;

        while (deep > 0)
        {
            circleMove = new HexCircleMovement(size, pos, deep);
            
            foreach (var curPos in circleMove)
            {
                
                times = 0;
                circleMove2 = new HexCircleMovement(size, curPos, 1);

                if (pieces[curPos.x, curPos.y].Type == type) continue;

                foreach (var neighbour in circleMove2)
                {
                    if (pieces[neighbour.x, neighbour.y].Type == type)
                    {
                        times++;
                    }
                }

                if (times > besto.times && times > 0)
                    besto = (curPos, times);
            }

            --deep;
        }
            

        if (besto.times > 0 && besto.times != int.MaxValue)
        {
            times = besto.times;
            newPos = besto.pos;
            piece = pieces[newPos.x, newPos.y];
            return true;
        }

        return false;
    }

    private void AddToppings()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                ref var cur = ref pieces[i, j];
                Vector3 curPos = GetOffset((i, j));
                var topp = PieceData.GetRandomTopp(cur.Type);

                if (topp.HasValue)
                {
                    SetTopping(ref cur, topp.Value);
                    //Vector3 toppPosition = new(curPos.x, topp.Value.Prefab.transform.position.y, curPos.z);

                    //topp.Value.SetPrefab(Instantiate(topp.Value.Prefab, toppPosition, Quaternion.identity, transform));
                    //cur.SetTopping(topp.Value);
                }
            }
        }
    }

    private void SetTopping(ref Piece piece, ToppingType type, uint rotation = 0)
    {
        SetTopping(ref piece, PieceData.GetTopping(type), rotation);
    }

    private void SetTopping(ref Piece piece, Topping topp, uint rotation = 0)
    {
        Vector3 curPos = new(piece.piece.transform.position.x, 0, piece.piece.transform.position.z);
        Vector3 toppPosition = new(curPos.x, topp.Prefab.transform.position.y, curPos.z);

        topp.SetPrefab(Instantiate(topp.Prefab, toppPosition, topp.Prefab.transform.rotation, transform));
        topp.Rotate((int)rotation);
        piece.SetTopping(topp);
    }
}

public class HexCircleMovement : IEnumerable<Vector2Int>
{
    Vector2Int _size;
    Vector2Int _pos;
    int _deep;

    public class HCMEnumerator : IEnumerator<Vector2Int>
    {
        (int x, int y, int times)[] _directions =
        {
            (1, -1, 1),
            (1, 1, 1),
            (-1, 1, 1),
            (-1, 0, 1),
            (-1, -1, 1),
            (1, -1, 1),
            (1, 0, 0),
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

        public Vector2Int Current => _current;

        object IEnumerator.Current => _current;

        public void Dispose()
        {
            _directions = null;
        }

        public bool MoveNext()
        {
            bool isValid = false;

            while (!isValid)
            {
                if (_idx >= _directions.Length) return false;

                var newPos = new Vector2Int
                {
                    x = _current.x + ((((_current.y % 2 == 0 && _directions[_idx].x > 0) || (_current.y % 2 != 0 && _directions[_idx].x < 0))
                                 && _directions[_idx].y != 0) ? 0 : _directions[_idx].x),
                    y = _current.y + _directions[_idx].y
                };

                if (newPos.x >= 0 && newPos.x < _size.x &&
                newPos.y >= 0 && newPos.y < _size.y)
                    isValid = true;

                _current = newPos;
                if (++_curRadius >= _directions[_idx].times + _deep - 1)
                {
                    ++_idx;
                    _curRadius = 0;
                }
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
