using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WorldG.Stats
{
    public enum ResourceType
    {
        None,
        Wood,
        Stone,
        Water,
        Food
    }

    public class Inventory : MonoBehaviour
    {
        private Dictionary<ResourceType, int> _resources;

        public event Action<ResourceType, int> OnResourceChanged;

        private void Start()
        {
            _resources = new Dictionary<ResourceType, int>();

            foreach (ResourceType item in Enum.GetValues(typeof(ResourceType)))
            {
                _resources.Add(item, 0);
            }
        }

        public void AddResource(ResourceType type, int amount)
        {
            _resources[type] += amount;
            OnResourceChanged?.Invoke(type, _resources[type]);
        }
    }
}
