using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class Bubble : MonoBehaviour
{
    [SerializeField] public string id;
    [SerializeField] private Transform popUpButtonHolder;

    public void AddButton(GameObject popUpButton)
    {
        GameObject button = Instantiate(popUpButton, popUpButtonHolder, true);
    }

    public void SetName(string v)
    {
        transform.name = v;
    }
}
