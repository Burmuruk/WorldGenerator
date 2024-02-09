using MyDearAnima.Controll;
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
            
            public DisableInTime<GameObject> dtShowAmount { get; set; }
        }

        private void Awake()
        {
            inventory = FindObjectOfType<Inventory>();

            inventory.OnResourceChanged += UpdateResource;
        }

        private void Start()
        {
            for (int i = 0; i < panels.Length; i++)
                panels[i].dtShowAmount = new(2, panels[i].panel);
        }

        public void UpdateResource(ResourceType type, int amount)
        {
            foreach (var panel in panels)
            {
                if (panel.type == type)
                {
                    if (panel.dtShowAmount.IsRunning)
                        panel.dtShowAmount.Restart();
                    else
                        StartCoroutine(panel.dtShowAmount.EnableInTime());

                    panel.text.text = amount.ToString();
                }
            }
        }

        //public void ShowRequirement(ResourceType type)
        //{
        //    foreach (var panel in panels)
        //    {
        //        if (panel.type == type)
        //        {
        //            if (panel.dtShowAmount.IsRunning)
        //                panel.dtShowAmount.Restart();
        //            else
        //                StartCoroutine(panel.dtShowAmount.EnableInTime());

        //            panel.text.text = amount.ToString();
        //        }
        //    }
        //}
    }
}
