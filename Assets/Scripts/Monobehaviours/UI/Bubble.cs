using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class Bubble : MonoBehaviour
{
    [SerializeField] public string id;
    [SerializeField] private Transform popUpButtonHolder;
    private bool init = false;
    Vector2 initSize = Vector2.one;

    public void AddButton(GameObject instancedBubbleButton, string text, Sprite icon, Action onClick)
    {
        if(!init)
        {
            init = true;
            initSize = GetComponent<RectTransform>().sizeDelta;
        }
        instancedBubbleButton.transform.name = "But_" + text;
        instancedBubbleButton.GetComponent<Image>().color = Color.clear;
        instancedBubbleButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        instancedBubbleButton.transform.GetChild(1).GetComponent<Image>().sprite = icon;
        instancedBubbleButton.GetComponent<Button>().onClick.AddListener(() => onClick());
        GetComponent<RectTransform>().sizeDelta = new Vector2 (initSize.x * transform.GetChild(0).childCount ,initSize.y);
    }

    public void AddRectTransform(GameObject obj)
    {
        if (!init)
        {
            init = true;
            initSize = GetComponent<RectTransform>().sizeDelta;
        }
        GetComponent<RectTransform>().sizeDelta = new Vector2(initSize.x * transform.GetChild(0).childCount, initSize.y);
    }

    public void AddDescription(GameObject instancedDescription, string text)
    {
        if (!init)
        {
            init = true;
            initSize = GetComponent<RectTransform>().sizeDelta;
        }
        GetComponent<RectTransform>().sizeDelta = new Vector2(initSize.x * transform.GetChild(0).childCount, initSize.y * transform.GetChild(0).childCount);
    }
}
