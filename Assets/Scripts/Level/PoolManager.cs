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

        public Dictionary<int, List<Minion>> Minions { get => _minions; }
        
        public struct MinionGroup
        {
            int id;
            List<Minion> minions;
        }

        private void Awake()
        {

        }

        public void AddPiece(ref Piece piece)
        {
            if (piece.Topping.Prefab)
                AddTopping(ref piece);

            if (!_piecesPool.ContainsKey(piece.TileType))
            {
                _piecesPool.Add(piece.TileType, new());
            }

            _piecesPool[piece.TileType].AddLast(piece);
            piece.Prefab.gameObject.SetActive(false);

            //piece.Prefab.gameObject.GetComponent<IKillable>().OnDie += SetPieceAvailable;
        }

        private void SetPieceAvailable()
        {
            throw new NotImplementedException();
        }

        public void AddTopping(ref Piece piece)
        {
            if (!piece.Topping.Prefab) return;

            if (!_toppingsPool.ContainsKey(piece.ToppingType))
                _toppingsPool.Add(piece.ToppingType, new());

            _toppingsPool[piece.ToppingType].AddLast(piece.Topping);
            piece.Topping.Prefab.SetActive(false);
            piece.SetTopping(default);

            //piece.Prefab.gameObject.GetComponent<IKillable>().OnDie += SetToppingAvailable;
        }

        private void SetToppingAvailable()
        {

        }

        public Character AddCharacter(CharacterType type)
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
    } 
}