using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class MinionPopUpMenu : MonoBehaviour
{
    [SerializeField] CharImage[] characterImage;
    [SerializeField] GameObject panel;
    [SerializeField] Button[] buttons;
    private int curId = -1;
    Action action;

    [Serializable]
    public struct CharImage
    {
        public Sprite image;
        public CharacterType characterType;
    }

    // Start is called before the first frame update
    void Start()
    {
        buttons = new Button[panel.transform.childCount];

        for (int i = 0; i < panel.transform.childCount; i++)
        {
            buttons[i] = panel.transform.GetChild(i).GetComponent<Button>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlace(Vector3 position, Action<CharacterType> callBack, in int id, params CharacterType[] characters)
    {
        if (curId != -1 && curId == id) { HideButtons(); return; }

        curId = id;
        transform.position = position;
        var lookAt = GetComponent<LookAtConstraint>();
        ConstraintSource constraint = new()
        {
            weight = 1,
            sourceTransform = Camera.main.transform,
        };

        lookAt.AddSource(constraint);

        for (int i = 0; i < characters.Length; i++)
        {
            var cur = characters[i];
            
            for (int j = 0; j < characterImage.Length; j++)
            {
                if (cur == characterImage[j].characterType)
                {
                    buttons[i].gameObject.GetComponent<Image>().sprite = characterImage[j].image;
                    buttons[i].gameObject.SetActive(true);
                    int idx = i;
                    action += () => callBack(characters[idx]);
                }
            }
        }


    }

    public void RunCallBack()
    {
        action?.Invoke();
    }

    private void HideButtons()
    {
        foreach (var button in buttons)
        {
            button.gameObject.SetActive(false);
        }

        curId = -1;
        transform.position += Vector3.up * -50;
    }
}
