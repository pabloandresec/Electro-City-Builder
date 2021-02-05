using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class CharController : MonoBehaviour
{
    [SerializeField] private CharacterData[] characters;
    [Header("UI references")]
    [SerializeField] GameObject characterMenu;

    public void DebugADialogFromACharacter(int characterIndex, Emotion e, string dialog, Action onDialogEnd)
    {
        Image image = characterMenu.transform.Find("Img_Person").GetComponent<Image>();
        CharacterEmotion ce = characters[characterIndex].GetEmotion(e);
        if(ce != null && ce.sounds.Length > 0)
        {
            GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(ce.sounds[Random.Range(0, ce.sounds.Length)]);
        }
        image.sprite = ce.sprite;
        TextMeshProUGUI text = characterMenu.transform.Find("Img_Bubble/Txt_Text").GetComponent<TextMeshProUGUI>();
        text.text = dialog;
        Button but = characterMenu.transform.Find("Img_Bubble").GetComponent<Button>();
        but.onClick.AddListener(() => {
            onDialogEnd?.Invoke();
            GetComponent<UIController>().FadeOutMenu(characterMenu);
            Debug.Log("Pushing button");
        });
    }

    public void DebugActionsFromACharacter(int characterIndex, Emotion e, Dictionary<string, Action> dialog)
    {

    }
}
