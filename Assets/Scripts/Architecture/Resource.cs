using System;
using Unity.VisualScripting;
using UnityEngine;
using WorldG.Control;
using WorldG.level;

namespace WorldG.Architecture
{
    public class Resource : MonoBehaviour, ISelectable, IKillable, IClickable
    {
        LevelGenerator level;

        [SerializeField] float health = 100;
        private bool _isSelected = false;
        Action onDeselection;
        Action onDie;
        int id = -1;

        public bool IsSelected => _isSelected;
        public int ID { get => id == -1 ? id = GetHashCode() : id; }
        public Action OnDeselection { get => onDeselection; set => onDeselection += value; }

        public float Health => health;

        public Action OnDie { get => onDie; set => onDie += value; }

        public bool IsWorking => false;

        private void Awake()
        {
            level = FindObjectOfType<LevelGenerator>();
        }

        public void Select()
        {
            if (IsSelected) return;


            _isSelected = true;
        }

        public void Deselect()
        {
            _isSelected = false;
        }

        public void SetDamage(float damage)
        {
            health -= damage;
        }

        public void Click()
        {
            if (level.GetPieceInfo(transform.position).TileType == TileType.Road)
                return;

            //level.SetPiece
        }

        public void DoubleClick()
        {
            
        }
    }

}
