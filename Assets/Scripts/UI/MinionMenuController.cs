using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WorldG.Control;
using WorldG.level;

namespace WorldG.UI
{
    public class MinionMenuController : MonoBehaviour
    {
        [SerializeField] MinionMenu[] menus;

        PoolManager pool;
        PlayerController player;

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
        }

        public void ShowMenu()
        {
            foreach (var menu in menus)
            {
                if (menu.unlocked)
                {
                    menu.panel.SetActive(true);
                    menu.button.onClick.AddListener(() => ShowPiece(menu.type));
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
