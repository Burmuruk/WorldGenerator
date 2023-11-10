using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldG.level;

public class CharacterCreator : MonoBehaviour
{
    [SerializeField] CharacterType characterType;
    public Action<CharacterType> OnClick;
    bool isWorking = false;

    public bool IsWorking => isWorking;

    public void SetCharacters(CharacterType type)
    {
        characterType = type;
    }

    public void Click()
    {
        if (IsWorking) return;

        isWorking = true;

        OnClick?.Invoke(characterType);

        Invoke("EnableButton", .5f);
    }

    private void EnableButton() => isWorking = false;
}
