using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] mainMenus;
    [SerializeField] private GameObject tileSelectedMenu;
    [SerializeField] private GameController game;

    public void SwitchMenu(int i)
    {
        if(i < 0 || i >= mainMenus.Length)
        {
            Debug.LogError("Out of index error in menus array");
        }
        foreach(GameObject m in mainMenus)
        {
            m.SetActive(false);
        }
        mainMenus[i].SetActive(true);
    }

    public void SetSelectedTileMenu(bool v, BuildingData buildingData)
    {
        if(v)
        {
            tileSelectedMenu.SetActive(v);
            tileSelectedMenu.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = buildingData.name;
            tileSelectedMenu.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = buildingData.description;
            switch(buildingData.Index)
            {
                case 0:
                    tileSelectedMenu.transform.GetChild(2).GetChild(0).gameObject.SetActive(true);
                    tileSelectedMenu.transform.GetChild(2).GetChild(1).gameObject.SetActive(false);
                    tileSelectedMenu.transform.GetChild(2).GetChild(2).gameObject.SetActive(false);
                    tileSelectedMenu.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
                break;
                case 1:
                    tileSelectedMenu.transform.GetChild(2).GetChild(0).gameObject.SetActive(false);
                    tileSelectedMenu.transform.GetChild(2).GetChild(1).gameObject.SetActive(false);
                    tileSelectedMenu.transform.GetChild(2).GetChild(2).gameObject.SetActive(false);
                    tileSelectedMenu.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
                break;
                default:
                    tileSelectedMenu.transform.GetChild(2).GetChild(0).gameObject.SetActive(false);
                    tileSelectedMenu.transform.GetChild(2).GetChild(1).gameObject.SetActive(true);
                    tileSelectedMenu.transform.GetChild(2).GetChild(2).gameObject.SetActive(true);
                    tileSelectedMenu.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
                break;
            }
        }
        else
        {
            tileSelectedMenu.SetActive(v);
        }
    }
}
