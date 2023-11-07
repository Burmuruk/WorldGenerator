using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldG.Control;
using WorldG.level;

namespace WorldG.Control
{
    public class MyButton : MonoBehaviour, IClickable
    {
        [SerializeField] CharacterType characterType;
        LevelGenerator levelGenerator;
        bool isWorking = false;
        public Action<CharacterType> OnClick;

        public bool IsWorking => isWorking;

        private void Awake()
        {
            levelGenerator = FindObjectOfType<LevelGenerator>();
        }

        public void Click()
        {
            if (IsWorking) return;

            isWorking = true;

            OnClick?.Invoke(characterType);

            Invoke("EnableButton", .5f);
        }

        public void DoubleClick()
        {

        }

        private void EnableButton() => isWorking = false;
    }
}
