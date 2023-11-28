using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WorldG.level;

public class MinionMenuController : MonoBehaviour
{
    [SerializeField] MinionMenu[] menus;

    PoolManager pool;

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
    }

    public void ShowMenu()
    {
        foreach (var menu in menus)
        {
            if (menu.unlocked)
                menu.panel.SetActive(true);
        }
    }

    private void ShowPiece()
    {
        pool.GetTopping();
    }
}
