using System;
using UnityEngine;
using WorldG.Control;

namespace WorldG.Architecture
{
    public class Resource : MonoBehaviour, ISelectable
    {
        private bool _isSelected = false;
        Action onDeselection;
        int id = -1;

        public bool IsSelected => _isSelected;
        public int ID { get => id == -1 ? id = GetHashCode() : id; }
        public Action OnDeselection { get => onDeselection; set => onDeselection += value; }

        public void Select()
        {
            if (IsSelected) return;


            _isSelected = true;
        }

        public void Deselect()
        {
            _isSelected = false;
        }
    }

}
