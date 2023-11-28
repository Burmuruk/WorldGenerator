﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WorldG.Control;

namespace WorldG.level
{
    public class PoolManager : MonoBehaviour
    {
        [SerializeField] Transform goLevel;
        [SerializeField] PieceCollection pieceCollection;

        private Dictionary<TileType, LinkedList<Piece>> _piecesPool = new();
        private Dictionary<ToppingType, LinkedList<Topping>> _toppingsPool = new();
        private Dictionary<CharacterType, LinkedList<Minion>> _charactersPool = new();
        private Dictionary<int, List<Minion>> _minions = new();
        private Dictionary<ToppingType, LinkedList<UnmanagedTopping>> _unmanagedToppings = new();

        private Transform piecesParent;
        private Transform toppingsParent;
        private Transform characterParent;
        private bool checkUnmaged = false;

        private void Awake()
        {
            piecesParent = goLevel.transform.Find("Floor");
            if (piecesParent == null)
            {
                var newPar = new GameObject("Floor");
                newPar.transform.parent = goLevel.transform;
            }

            toppingsParent = goLevel.transform.Find("Toppings");
            if (toppingsParent == null)
            {
                var newPar = new GameObject("Toppings");
                newPar.transform.parent = goLevel.transform;
            }

            characterParent = goLevel.transform.Find("Toppings");
            if (characterParent == null)
            {
                var newPar = new GameObject("Toppings");
                newPar.transform.parent = goLevel.transform;
            }
        }

        public Dictionary<CharacterType, LinkedList<Minion>> Minions { get => _charactersPool; }

        public struct UnmanagedTopping
        {
            public Topping Topping { get; private set; }
            public System.Object Owner { get; private set; }
            public bool IsAlive { get =>  Owner != null; }
            public ToppingType Type { get; private set; }
            public int Version { get; private set; }

            public UnmanagedTopping(Topping topping, System.Object owner, ToppingType type, int version = -1)
            {
                Topping = topping;
                Owner = owner;
                Type = type; 
                Version = version;
            }

            public void SetOwner(object owner) => Owner = owner;
        }
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
            piece.Prefab = Instantiate(newPiece.Prefab, newPiece.Prefab.transform.position, Quaternion.identity, piecesParent);

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

            curPiece.Prefab = Instantiate(curPiece.Prefab, curPiece.Prefab.transform.position, Quaternion.identity, piecesParent);

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

        public void GetTopping(ref Piece piece, ToppingType toppingType, int version = -1)
        {
            if (_toppingsPool.ContainsKey(toppingType))
            {
                var cur = _toppingsPool[toppingType].First;
                for (int i = 0; i < _toppingsPool[toppingType].Count; i++)
                {
                    if (!cur.Value.Prefab.activeSelf)
                    {
                        if (version >= 0 ? cur.Value.Version == version : true)
                        {
                            var worker = piece.SetTopping(cur.Value);
                            SetToppingAvailable(ref piece);
                            var (pref, typep) = (piece.Topping.Prefab, piece.ToppingType);
                            worker.OnStop += () => DisableTopping(pref, typep);

                            if (piece.Topping.Prefab)
                                SetToppingAvailable(ref piece);
                            return;
                        }
                    }
                    else break;

                    cur = cur.Next;
                }
            }

            if (piece.Topping.Prefab)
                SetToppingAvailable(ref piece);

            if (!_toppingsPool.ContainsKey(toppingType))
                _toppingsPool.Add(toppingType, new());

            var topping = pieceCollection.GetTopping(toppingType);
            topping.SetPrefab(Instantiate(topping.Prefab, topping.Prefab.transform.position, topping.Prefab.transform.rotation, toppingsParent));

            var (type, prefab, sides, versionID, haveSpline) = topping;
            var copy = new Topping(type, prefab, sides, versionID, haveSpline);

            var worker1 = piece.SetTopping(topping);
            var (pref1, typep1) = (piece.Topping.Prefab, piece.ToppingType);

            worker1.OnStop += () => DisableTopping(pref1, typep1);
            piece.Topping.Prefab.SetActive(true);

            _toppingsPool[piece.ToppingType].AddLast(copy);
        }

        public Topping GetTopping(object owner, ToppingType toppingType, int version = -1)
        {
            UnmanagedTopping newTopping = default;
            if (FindUnamagedTopping(toppingType, out newTopping, version))
            {
                newTopping.SetOwner(owner);

                _unmanagedToppings[toppingType].Remove(newTopping);
                newTopping.Topping.Prefab.SetActive(true);
                _unmanagedToppings[toppingType].AddLast(newTopping);

                return newTopping.Topping;
            }

            //if (piece.Topping.Prefab)
            //    SetToppingAvailable(ref piece);

            if (!_unmanagedToppings.ContainsKey(toppingType))
                _unmanagedToppings.Add(toppingType, new());

            var topping = pieceCollection.GetTopping(toppingType);
            topping.SetPrefab(Instantiate(topping.Prefab, topping.Prefab.transform.position, topping.Prefab.transform.rotation, toppingsParent));

            topping.Prefab.GetComponent<Collider>().enabled = false;
            var (type, prefab, sides, versionID, haveSpline) = topping;
            var copy = new Topping(type, prefab, sides, versionID, haveSpline);

            topping.Prefab.SetActive(true);
            var unmanaged = new UnmanagedTopping(topping, owner, type, version);
            _unmanagedToppings[type].AddLast(unmanaged);
            return topping;
        }

        private bool FindUnamagedTopping(ToppingType toppingType, out UnmanagedTopping topping, int version = -1)
        {
            topping = default;

            if (_unmanagedToppings.ContainsKey(toppingType))
            {
                var cur = _unmanagedToppings[toppingType].First;

                for (int i = 0; i < _unmanagedToppings[toppingType].Count; i++)
                {
                    if (!cur.Value.Topping.Prefab.activeSelf)
                    {
                        if (version >= 0 ? cur.Value.Version == version : true)
                        {
                            topping = cur.Value;
                            return true;
                        }

                        break;
                    }

                    cur = cur.Next;
                }
            }

            return false;
        }

        private bool FindTopping(ToppingType type, out UnmanagedTopping topping, int version = -1)
        {
            topping = default;

            //if (_toppingsPool.ContainsKey(type))
            //{
            //    var cur = _toppingsPool[type].First;
            //    for (int i = 0; i < _toppingsPool[type].Count; i++)
            //    {
            //        if (!cur.Value.Prefab.activeSelf)
            //        {
            //            if (version >= 0 ? cur.Value.Version == version : true)
            //            {
            //                var worker = piece.SetTopping(cur.Value);
            //                SetToppingAvailable(ref piece);
            //                var (pref, typep) = (piece.Topping.Prefab, piece.ToppingType);
            //                worker.OnStop += () => DisableTopping(pref, typep);

            //                if (piece.Topping.Prefab)
            //                    SetToppingAvailable(ref piece);
            //                return;
            //            }
            //        }
            //        else break;

            //        cur = cur.Next;
            //    }
            //}

            return false;
        }

        public void DisableUnamanagedTopping(Topping topping)
        {
            if (_unmanagedToppings.ContainsKey(topping.Type))
            {
                var cur = _unmanagedToppings[topping.Type].Last;

                for (int i = 0; i < _unmanagedToppings[topping.Type].Count; i++)
                {
                    if (cur.Value.Topping.Prefab.activeSelf)
                    {
                        if (topping.Prefab.GetInstanceID() == cur.Value.Topping.Prefab.GetInstanceID())
                        {
                            cur.Value.SetOwner(null);

                            _unmanagedToppings[topping.Type].Remove(cur);
                            cur.Value.Topping.Prefab.SetActive(false);
                            _unmanagedToppings[topping.Type].AddFirst(cur);

                            return;
                        }
                    }
                    else
                        break;

                    cur = cur.Previous;
                }
            }
        }

        public void SetUnmanagetTopping()
        {

        }

        private void SetToppingAvailable(ref Piece piece)
        {
            var cur = _toppingsPool[piece.Topping.Type].First;
            for (int i = 0; i < _toppingsPool[piece.Topping.Type].Count; i++)
            {
                if (!cur.Value.Prefab.activeSelf)
                {
                    if (cur.Value.Prefab.GetInstanceID() == piece.Topping.Prefab.GetInstanceID())
                    {
                        piece.Topping.Prefab.SetActive(true);
                        _toppingsPool[piece.Topping.Type].Remove(cur);

                        _toppingsPool[piece.Topping.Type].AddLast(cur);

                        break;
                    }
                }
                else break;

                cur = cur.Next;
            }
        }

        public void DisableTopping(GameObject piece, ToppingType type)
        {
            var cur = _toppingsPool[type].Last;
            for (int i = 0; i < _toppingsPool[type].Count; i++)
            {
                if (cur.Value.Prefab.activeSelf)
                {
                    if (cur.Value.Prefab.GetInstanceID() == piece.GetInstanceID())
                    {
                        piece.SetActive(false);
                        _toppingsPool[type].Remove(cur);

                        _toppingsPool[type].AddFirst(cur);

                        break;
                    }
                }
                else break;

                cur = cur.Previous;
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
                characterIns = Instantiate(character.Prefab, character.Prefab.transform.position, character.Prefab.transform.rotation, characterParent);
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

        private IEnumerator CheckUnmanagedToppings()
        {
            while (checkUnmaged)
            {
                foreach (var key in _unmanagedToppings.Keys)
                {
                    var cur = _unmanagedToppings[key].First;

                    for (int i = 0; i < _unmanagedToppings[key].Count; i++)
                    {
                        if (cur.Value.Owner == null)
                        {
                            var next = cur.Next;
                            Destroy(cur.Value.Topping.Prefab);
                            _unmanagedToppings[key].Remove(cur);
                            
                            --i;
                            cur = next;
                            continue;
                        }

                        cur = cur.Next;
                    }
                }

                yield return new WaitForSeconds(60);
            }

            yield break;
        }
    } 
}