using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldG.level
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] PieceCollection PieceData;
        [SerializeField] Piece[,] pieces;
        [SerializeField] Vector2Int size = new Vector2Int(10, 10);
        PoolManager _pool;
        Vector2Int startPoint;

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

        public Vector3 StartPoint
        {
            get
            {
                return GetOffset(startPoint);
            }
        }

        private void Start()
        {
            _pool = FindObjectOfType<PoolManager>();
            pieces = new Piece[size.x, size.y];
            PieceData.Initialize();

            GenerateContent((0, 0), SideType.None);
            CreatePieces();
            GenerateAreas();
            AddToppings();
            GetStartPoint();
        }

        private void Update()
        {

        }

        //public void Get

        private void GetStartPoint()
        {
            bool isValid = false;

            while (!isValid)
            {
                var randX = UnityEngine.Random.Range(1, size.x - 1);
                var randY = UnityEngine.Random.Range(1, size.y - 1);
                ref var cur = ref pieces[randX, randY];
                if (cur.Type == SideType.Grass && cur.Topping.Prefab == null)
                {
                    var nextPos = MovePosition(new(randX, randY), 2);

                    ref var nextPiece = ref pieces[nextPos.x, nextPos.y];
                    if (nextPiece.Type != SideType.Water && !nextPiece.Topping.Prefab)
                    {
                        //PieceData.ChangeType(cur, )
                        //PieceData.ChangeColor(cur, SideType.Mudd);
                        SetTopping(ref cur, ToppingType.House, 2);

                        var newDir = MovePosition(new(randX, randY), 2);
                        var newPos = GetOffset(newDir);
                        SetCharacter(newPos, CharacterType.Knight, 2);
                        //SetTopping(ref cur, ToppingType.);

                        ref var newPiece = ref pieces[newDir.x, newDir.y];
                        RepleacePiece(ref newPiece, TileType.Road, (5, SideType.Road));

                        startPoint = new(randX, randY);
                        isValid = true;
                    }
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

                cur = piece with
                {
                    Prefab = null,
                    Type = GetProbabilityPerSide(((int)curCord.x, (int)curCord.y), prevSide)
                };

                //topping

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
        }

        public Vector3 GetOffset(Vector2Int cord)
        {
            return GetOffset((cord.x, cord.y));
        }

        public Vector3 GetOffset((float x, float y) cord)
        {
            Vector3 curPos = new Vector3(cord.x * 2, 0, cord.y * -1.75f);

            if ((cord.y % 2 != 0))
                curPos += Vector3.right;
            return curPos;
        }

        public Vector2Int RemoveOffset(Vector3 position)
        {
            Vector2Int curPos = default;

            int x = (int)(position.x * .5f);
            int y = (int)((position.z / -1.75f));

            curPos += new Vector2Int(x, y);
            return curPos;
        }

        public void SetRoad(in Vector2Int cord)
        {
            ref var cur = ref pieces[cord.x, cord.y];

            if (cur.Topping.Prefab) return;
            if (cur.Type == SideType.Water) return;

            List<(int idx, SideType side)> connections = new();
            HexCircleMovement mov = new HexCircleMovement(size, cord, 1);
            int i = 0;

            foreach (var nextPiece in mov)
            {
                if (nextPiece.x < 0 || nextPiece.x >= size.x ||
                    nextPiece.y < 0 || nextPiece.y >= size.y)
                {
                    i++;
                    continue;
                }

                ref var neighbour = ref pieces[nextPiece.x, nextPiece.y];
                if (neighbour.TileType == TileType.Road && neighbour.Entrances.Length > 0 && neighbour[GetOpositeSide(i, neighbour.Types.Length)] != SideType.Road)
                {
                    var sides = GetSidesOfConnectedRoad(ref neighbour, in i);

                    RepleacePiece(ref neighbour, TileType.Road, sides);
                    connections.Add((i, SideType.Road));
                }
                i++;
            }

            if (connections.Count == 0) return;

            RepleacePiece(ref cur, TileType.Road, connections.ToArray());
        }

        private (int idx, SideType side)[] GetSidesOfConnectedRoad(ref Piece neighbour, in int start)
        {
            List<(int idx, SideType side)> sides = new();

            bool inside = false;
            int oposite = GetOpositeSide(start, neighbour.Types.Length);

            for (int i = 0; i < neighbour.Entrances.Length; i++)
            {
                if (!inside && neighbour.Entrances[i] > oposite)
                {
                    inside = true;
                    sides.Add((oposite, SideType.Road));
                }

                sides.Add((neighbour.Entrances[i], SideType.Road));
            }

            if (!inside)
                sides.Add((oposite, SideType.Road));

            return sides.ToArray();
        }

        private void CreatePieces()
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    ref var cur = ref pieces[i, j];

                    SetPiece(ref cur, (i, j));
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
            piece = default;
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

        public Vector2Int MovePosition(in Vector2Int cord, int idx)
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
            Piece nextPiece = default;
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
            piece = default;
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
                    if (curPos.x < 0 || curPos.x >= size.x ||
                    curPos.y < 0 || curPos.y >= size.y)
                        continue;

                    times = 0;
                    circleMove2 = new HexCircleMovement(size, curPos, 1);

                    if (pieces[curPos.x, curPos.y].Type == type) continue;

                    foreach (var neighbour in circleMove2)
                    {
                        if (neighbour.x < 0 || neighbour.x >= size.x ||
                        neighbour.y < 0 || neighbour.y >= size.y)
                            continue;

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
            Vector3 curPos = new(piece.Prefab.transform.position.x, 0, piece.Prefab.transform.position.z);
            Vector3 toppPosition = new(curPos.x, topp.Prefab.transform.position.y, curPos.z);

            topp.SetPrefab(Instantiate(topp.Prefab, toppPosition, topp.Prefab.transform.rotation, transform));
            topp.Rotate((int)rotation);
            piece.SetTopping(topp);
        }

        public Character SetCharacter(Vector3 pos, CharacterType type, uint rotation = 0)
        {
            //var character = PieceData.GetCharacter(type);
            //SetCharacter(pos, ref character, rotation);

            var character = _pool.AddCharacter(type);
            Vector3 toppPosition = new(pos.x, character.Prefab.transform.position.y, pos.z);

            character.Prefab.transform.position = toppPosition;
            character.Rotate((int)rotation);

            return character;
        }

        public void SetCharacter(Vector3 pos, ref Character character, uint rotation = 0)
        {
            Vector3 toppPosition = new(pos.x, character.Prefab.transform.position.y, pos.z);

            character.SetPiece(Instantiate(character.Prefab, toppPosition, character.Prefab.transform.rotation, transform));
            character.Rotate((int)rotation);
        }

        public void RepleacePiece(ref Piece piece, TileType type, params (int idx, SideType)[] sides)
        {
            _pool.AddPiece(ref piece);

            Piece newPiece = type == TileType.Road ? PieceData.GetPiece(type, sides) : PieceData.GetPiece(type);

            if (newPiece == null) return;

            var pos = piece.Prefab.transform.position;
            var rotation = piece.Prefab.transform.rotation;
            var prevType = piece.Type;

            piece = newPiece;
            piece.Prefab = Instantiate(newPiece.Prefab, pos, Quaternion.identity, transform);
            piece.Rotate();

            PieceData.ChangeType(piece, prevType);
        }

        public void SetPiece(ref Piece piece, in (int x, int y) pos)
        {
            Vector3 curPos = GetOffset(pos);

            SetPiece(ref piece, curPos);
        }

        public void SetPiece(ref Piece piece, Vector3 curPos)
        {
            piece.Prefab = Instantiate(PieceData.GetPiece(TileType.Solid).Prefab, position: curPos, rotation: Quaternion.identity, parent: transform);

            PieceData.ChangeType(piece, piece.Type);
        }
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

                //if (newPos.x >= 0 && newPos.x < _size.x &&
                //newPos.y >= 0 && newPos.y < _size.y)
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
