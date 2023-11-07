using System;
using UnityEngine;

namespace WorldG.Patrol
{
    public interface ISplineNode
    {
        public uint ID { get; }
        public NodeData NodeData { get; }
        public bool IsSelected { get; }
        public Vector3 Position { get; }
        public Action OnStart { get; set; }
        public Transform Transform { get; }
    }
}
