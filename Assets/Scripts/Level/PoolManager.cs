using System;
using System.Collections.Generic;
using UnityEngine;
using WorldG.Control;

namespace WorldG.level
{
    public class PoolManager : MonoBehaviour
    {
        [SerializeField] PieceCollection pieceCollection;

        private Dictionary<TileType, LinkedList<Piece>> _piecesPool = new();
        private Dictionary<ToppingType, LinkedList<Topping>> _toppingsPool = new();
        private Dictionary<CharacterType, LinkedList<Minion>> _charactersPool = new();
        private Dictionary<int, List<Minion>> _minions = new();

        public Dictionary<CharacterType, LinkedList<Minion>> Minions { get => _charactersPool; }
        
        public struct MinionGroup
        {
            int id;
            List<Minion> minions;
        }

        public void GetPiece(ref Piece piece, TileType tileType, int idVersion = -1)
        {
            if (_piecesPool.ContainsKey(piece.TileType))
            {
                var cur = _piecesPool[tileType].First;
                for (int i = 0; i < _piecesPool[tileType].Count; i++)
                {
                    if (!cur.Value.Prefab.activeSelf)
                    {
                        if (idVersion >= 0 ? cur.Value.Version == idVersion : true)
                        {
                            RealivePiece(cur.Value);
                            piece.Prefab = cur.Value.Prefab;
                            return;
                        }
                    }
                    else break;

                    cur = cur.Next;
                }

            }

            var newPiece = pieceCollection.GetPiece(tileType);
            piece.Prefab = Instantiate(newPiece.Prefab, newPiece.Prefab.transform.position, Quaternion.identity, transform);

            newPiece.Prefab = piece.Prefab;

            AddPieceToPool(newPiece);
        }

        public Piece GetPiece(TileType tileType, (int idx, SideType sideType)[] sides)
        {
            List<Piece> pieces = null;
            Piece curPiece = null;

            if (_piecesPool.ContainsKey(tileType))
            {
                pieces = new();
                foreach (var cur in _piecesPool[tileType])
                {
                    if (!cur.Prefab.activeSelf)
                        pieces.Add(cur);
                    else
                        break;
                }

                curPiece = PieceCollection.FindPieceByRoads(pieces.ToArray(), sides);
                if (curPiece != null)
                    curPiece.Rotation = 0;
                if (curPiece != null && !PieceCollection.RotateUntilMatch(ref curPiece, in sides))
                    print("Error al rotar");
            }

            if (curPiece != null)
            {
                RealivePiece(curPiece);
                return curPiece;
            }

            curPiece = pieceCollection.GetPiece(tileType, sides);
            if (curPiece == null) return null;

            curPiece.Prefab = Instantiate(curPiece.Prefab, curPiece.Prefab.transform.position, Quaternion.identity, transform);

            var (go, ttype, sType, sTypes, mats, rot, comp, vID) = curPiece;
            var copy = new Piece(go, ttype, sType, sTypes, mats, rot, comp, vID);

            AddPieceToPool(curPiece);

            return copy;
        }

        //public Piece GetPiece(Piece piece)
        //{
        //    var sides = new (int idx, SideType type)[piece.Entrances.Length];

        //    for (int i = 0; i < piece.Entrances.Length; i++)
        //    {
        //        sides[i] = (piece.Entrances[i], piece[piece.Entrances[i]]);
        //    }

        //    var curPiece = PieceCollection.FindPieceByRoads(_piecesPool[piece.TileType].ToArray(), sides);

        //    if (curPiece != null) return curPiece;
            
            
            
        //    return null;
        //}

        public void KillPiece(ref Piece piece, TileType tileType)
        {
            var cur = _piecesPool[tileType].First;

            for (int i = 0; i < _piecesPool[tileType].Count; i++)
            {
                if (cur.Value.Prefab.GetInstanceID() == piece.Prefab.GetInstanceID())
                {
                    break;
                }

                cur = cur.Next;
            }

            _piecesPool[tileType].Remove(cur);
            _piecesPool[tileType].AddFirst(cur);

            cur.Value.Prefab.SetActive(false);
            piece.Rotation = 0;
            piece.Rotate();
            cur.Value.Prefab.transform.rotation = Quaternion.Euler(Vector3.zero);
        }

        private void RealivePiece(Piece piece)
        {
            _piecesPool[piece.TileType].Remove(piece);
            _piecesPool[piece.TileType].AddLast(piece);
            piece.Prefab.SetActive(true);
        }

        private void AddPieceToPool(Piece piece)
        {
            if (!_piecesPool.ContainsKey(piece.TileType))
            {
                _piecesPool.Add(piece.TileType, new());
            }

            _piecesPool[piece.TileType].AddLast(piece);
            piece.Prefab.gameObject.SetActive(true);
            piece.Rotation = 0;
        }

        private void SetPieceAvailable()
        {
            throw new NotImplementedException();
        }

        public void GetTopping(ref Piece piece, ref Topping topp, int version = -1)
        {
            if (_toppingsPool.ContainsKey(topp.Type))
            {
                var cur = _toppingsPool[topp.Type].First;
                for (int i = 0; i < _toppingsPool[topp.Type].Count; i++)
                {
                    if (!cur.Value.Prefab.activeSelf)
                    {
                        if (version >= 0 ? cur.Value.Version == topp.Version : true)
                        {
                            piece.SetTopping(cur.Value);
                            SetToppingAvailable(ref piece);

                            if (piece.Topping.Prefab)
                                SetToppingAvailable(ref piece, false);
                            return;
                        }
                    }
                    else break;

                    cur = cur.Next;
                }
            }

            if (piece.Topping.Prefab)
                SetToppingAvailable(ref piece, false);

            if (!_toppingsPool.ContainsKey(topp.Type))
                _toppingsPool.Add(topp.Type, new());

            var topping = pieceCollection.GetTopping(topp.Type);
            topping.SetPrefab(Instantiate(topping.Prefab, topping.Prefab.transform.position, topping.Prefab.transform.rotation, transform));

            var (type, prefab, sides, versionID, haveSpline) = topping;
            var copy = new Topping(type, prefab, sides, versionID, haveSpline);

            piece.SetTopping(topping);
            piece.Topping.Prefab.SetActive(true);
            _toppingsPool[piece.ToppingType].AddLast(copy);
        }

        private void SetToppingAvailable(ref Piece piece, bool value = true)
        {
            var cur = _toppingsPool[piece.Topping.Type].First;
            for (int i = 0; i < _toppingsPool[piece.Topping.Type].Count; i++)
            {
                if (!cur.Value.Prefab.activeSelf)
                {
                    if (cur.Value.Prefab.GetInstanceID() == piece.Topping.Prefab.GetInstanceID())
                    {
                        piece.Topping.Prefab.SetActive(value);
                        _toppingsPool[piece.Topping.Type].Remove(cur);

                        if (value)
                            _toppingsPool[piece.Topping.Type].AddLast(cur);
                        else
                            _toppingsPool[piece.Topping.Type].AddFirst(cur);

                        break;
                    }
                }
                else break;

                cur = cur.Next;
            }
        }

        public Character GetCharacter(CharacterType type)
        {
            var character = pieceCollection.GetCharacter(type);
            GameObject characterIns = null;

            if (!_charactersPool.ContainsKey(type))
                _charactersPool.Add(type, new());
            else
            {
                var cur = _charactersPool[type].Last;
                for (int i= 0; i < _charactersPool.Count; i++)
                {
                    if (!cur.Value.gameObject.activeSelf)
                    {
                        characterIns = cur.Value.gameObject;
                        characterIns.SetActive(true);
                        characterIns.transform.position = character.Prefab.transform.position;

                        _charactersPool[type].Remove(cur);
                        _charactersPool[type].AddLast(cur);
                        break;
                    }

                    cur = cur.Previous;

                    if (cur == null) break;
                }
            }

            if (characterIns == null)
            {
                characterIns = Instantiate(character.Prefab, character.Prefab.transform.position, character.Prefab.transform.rotation, transform);
                _charactersPool[type].AddLast(characterIns.GetComponent<Minion>());
            }

            character.SetPiece(characterIns);

            return character;
        }

        public List<Minion> GetMinionsById(int id)
        {
            return null;
        }

        public LinkedList<Piece> GetRoads()
        {
            return _piecesPool[TileType.Road];
        }
    } 
}