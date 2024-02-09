using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WorldG.Control;
using WorldG.level;
using WorldG.Stats;

namespace WorldG.UI
{
    public class MinionMenuController : MonoBehaviour
    {
        [SerializeField] MinionMenu[] menus;

        PoolManager pool;
        PlayerController player;
        InventoryManager inventory;

        [Serializable]
        public struct MinionMenu
        {
            [SerializeField] public GameObject panel;
            [SerializeField] public ToppingType type;
            [SerializeField] public bool unlocked;
            [SerializeField] public Button button;
        }

        private void Awake()
        {
            pool = FindObjectOfType<PoolManager>();
            player = FindObjectOfType<PlayerController>();
            inventory = FindObjectOfType<InventoryManager>();
        }

        public void ShowMenu()
        {
            foreach (var menu in menus)
            {
                if (menu.unlocked)
                {
                    menu.panel.SetActive(true);
                    menu.button.onClick.RemoveAllListeners();
                    menu.button.onClick.AddListener(() => ShowPiece(menu.type));
                }
            }
        }

        public void HideMenu()
        {
            foreach (var menu in menus)
            {
                if (menu.panel.activeSelf)
                {
                    menu.panel.SetActive(false);
                }
            }
        }

        private void ShowPiece(ToppingType type)
        {
            var topping = pool.GetTopping(this, type);

            player.TemporalPiece = topping;
        }
    } 
}
