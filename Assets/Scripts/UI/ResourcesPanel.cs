using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using WorldG.Stats;

namespace WorldG.UI
{
    public class ResourcesPanel : MonoBehaviour
    {
        [SerializeField] ResourcePanel[] panels;
        Inventory inventory;

        [Serializable]
        public struct ResourcePanel
        {
            public ResourceType type;
            public GameObject panel;
            public TextMeshProUGUI text;
        }

        private void Awake()
        {
            inventory = FindObjectOfType<Inventory>();

            inventory.OnResourceChanged += UpdateResource;
        }

        private void UpdateResource(ResourceType type, int amount)
        {
            foreach (var panel in panels)
            {
                if (panel.type == type)
                {
                    float value = 0;
                    if (float.TryParse(panel.text.text, out value))
                    {
                        panel.panel.SetActive(true);
                        panel.text.text = amount.ToString();
                    }
                }
            }
        }
    }
}
