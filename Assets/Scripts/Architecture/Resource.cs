using System;
using UnityEngine;
using WorldG.Control;

namespace WorldG.Architecture
{
    public class Resource : MonoBehaviour, ISelectable, IKillable
    {
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
    }

}
