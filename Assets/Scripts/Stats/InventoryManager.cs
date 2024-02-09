using UnityEngine;
using WorldG.UI;

namespace WorldG.Stats
{
    public class InventoryManager : MonoBehaviour
    {
        Inventory inventory;
        ResourcesPanel resourcesPanel;

        private void Awake()
        {
            inventory = FindObjectOfType<Inventory>();
            resourcesPanel = GetComponent<ResourcesPanel>();
        }

        public bool RequestResource(ResourceType type, int amount)
        {
            var result = inventory.SpendResource(type, amount);

            if (result == 0)
            {
                print("It's empty");
                return true;
            }
            else if (result < amount)
                print("not enough");

            return false;
        }

        public void AddResource(ResourceType type, int amount)
        { 
            inventory.AddResource(type, amount);

        }
    }
}
