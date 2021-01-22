using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] mainMenus;
    [Space(5)]
    [SerializeField] private TextMeshProUGUI power;
    [SerializeField] private TextMeshProUGUI money;
    [SerializeField] private GameObject tileSelectedMenu;
    [SerializeField] private GameObject buyItemPrefab;
    [SerializeField] private GameController game;

    [Space(5)]
    [Header("DEBUG!")]
    public Button buyButton;

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

    public void SetMenuVisibility(string name, bool s)
    {
        Transform t = transform.Find(name);
        if(t != null)
        {
            t.gameObject.SetActive(s);
        }
        else
        {
            Debug.Log("Missing button");
        }
    }

    public void UpdateUI(int currentMoney, int powerAvailable)
    {
        money.text = currentMoney.ToString();
        power.text = powerAvailable.ToString();
    }

    public void FillListWithAvailableBuildings(Transform holderList)
    {
        for (int i = 2; i < game.Buildings.Count; i++)
        {
            GameObject itemInstantiated = Instantiate(buyItemPrefab, holderList, false);
            UIItem uiItem = itemInstantiated.GetComponent<UIItem>();
            uiItem.FillData(game.Buildings[i]);
            uiItem.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("Trying to build " + uiItem.Index);
                game.TryBuild(uiItem.Index);
                uiItem.transform.parent.parent.gameObject.SetActive(false);
            });
        }
        Debug.Log(holderList.name + " filled with buildings!");
    }

    public void FillListWithAvailableComponents(Transform holderList)
    {
        for (int i = 0; i < game.Components.Count; i++)
        {
            GameObject itemInstantiated = Instantiate(buyItemPrefab, holderList, false);
            UIItem uiItem = itemInstantiated.GetComponent<UIItem>();
            uiItem.FillData(game.Components[i]);
            uiItem.GetComponent<Button>().onClick.AddListener(() =>
            {
                game.TryAddBuildingComponent(game.Components[i].Index);
            });
        }
        Debug.Log(holderList.name + " filled with components!");
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
