﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class CharController : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private CharacterData[] characters;
    [Header("Dialogs")]
    [SerializeField] private Dialog[] dialogs;
    [Header("UI references")]
    [SerializeField] GameObject characterMenu;

    private Image image;
    private TextMeshProUGUI text;
    private Button but;
    private int currentDialog = 0;
    private int currentEntry = 0;

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        image = characterMenu.transform.Find("Img_Person").GetComponent<Image>();
        text = characterMenu.transform.Find("Img_Bubble/Txt_Text").GetComponent<TextMeshProUGUI>();
        but = characterMenu.transform.Find("Img_Bubble").GetComponent<Button>();
        but.onClick.AddListener(() => {
            NextEntry();
            Debug.Log("Pushing button");
        });
    }

    public void DebugADialogFromACharacter(int characterIndex, Emotion e, string dialog, Action onDialogEnd)
    {
        CharacterEmotion ce = characters[characterIndex].GetEmotion(e);
        if(ce != null && ce.sounds.Length > 0)
        {
            GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(ce.sounds[Random.Range(0, ce.sounds.Length)]);
        }
        image.sprite = ce.sprite;
        text.text = dialog;
    }

    public void ShowDialog(int index)
    {
        GetComponent<UIController>().FadeInMenu(characterMenu);
        currentDialog = index;
        currentEntry = -1;
        NextEntry();
    }

    private void NextEntry()
    {
        int nextEntry = currentEntry + 1;
        if(nextEntry >= dialogs[currentDialog].Entries.Length)
        {
            GetComponent<UIController>().FadeOutMenu(characterMenu);
            dialogs[currentDialog].OnDialogEnd?.Invoke();
        }
        else
        {
            Entry e = dialogs[currentDialog].Entries[nextEntry];
            CharacterEmotion ce = characters[e.CharacterIndex].GetEmotion(e.Emotion);
            image.sprite = ce.sprite;
            text.text = e.Text;
            if (ce != null && ce.sounds.Length > 0)
            {
                GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(ce.sounds[Random.Range(0, ce.sounds.Length)]);
            }
            currentEntry++;
        }
    }

    public void AssignAnActionAtEndOfDialog(int dialogIndex, Action _action)
    {
        if(_action != null)
        {
            dialogs[dialogIndex].AssingOnEndEvent(_action);
        }
        else
        {
            Debug.LogWarning("Null event passed");
        }
    }
}
[Serializable]
public class Dialog
{
    [SerializeField] private string name;
    [SerializeField] private Entry[] entries;
    [SerializeField] private Action onDialogEnd;

    public string Name { get => name; }
    public Entry[] Entries { get => entries; }
    public Action OnDialogEnd { get => onDialogEnd; }

    public void AssingOnEndEvent(Action _OnEnd)
    {
        onDialogEnd = _OnEnd;
        Debug.Log("Action assigned to "+ name);
    }
}
[Serializable]
public class Entry
{
    [TextArea()]
    [SerializeField] private string text;
    [Min(0)]
    [SerializeField] private int characterIndex;
    [SerializeField] private Emotion emotion;

    public string Text { get => text; }
    public int CharacterIndex { get => characterIndex; }
    public Emotion Emotion { get => emotion; }
}