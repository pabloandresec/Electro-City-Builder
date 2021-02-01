using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] mainMenus;
    [SerializeField] private Transform categoryListHolder;
    [SerializeField] private Transform buildingListHolder;
    [SerializeField] private GameController game;
    [Space(5)]
    [Header("Game UI")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private TextMeshProUGUI power;
    [SerializeField] private TextMeshProUGUI money;
    [Header("Tile manipulation menus")]
    [SerializeField] private GameObject tileSelectedMenu;
    [SerializeField] private GameObject selectedDeviceComponentsMenu;
    [Header("Prefabs")]
    [SerializeField] private GameObject buyItemPrefab;
    [SerializeField] private GameObject popUpPrefab;
    [SerializeField] private Transform popUpHolder;
    private List<GameObject> activeBubbles;
    [Header("Settings")]
    [SerializeField] private float fadeTime = 0.1f;
    [SerializeField] private TileMenuMode tileMenuMode = TileMenuMode.MENU;

    [Space(5)]
    [Header("DEBUG!")]
    public Button buyButton;

    private int directionToFadeFrom = 0;
    private Dictionary<string, Vector2> menusPos;
    public static bool tweening = false;

    private void Awake()
    {
        activeBubbles = new List<GameObject>();
        menusPos = new Dictionary<string, Vector2>();
        CanvasGroup[] canvases = GetComponentsInChildren<CanvasGroup>(true);
        string s = "canvases found : " + canvases.Length + "\n";
        foreach (CanvasGroup c in canvases)
        {
            s += c.transform.name + "\n";
            menusPos.Add(c.transform.name, c.GetComponent<RectTransform>().anchoredPosition);
        }
        Debug.Log(s);
    }

    public void SwitchMenu(int i)
    {
        if (i < 0 || i >= mainMenus.Length)
        {
            Debug.LogError("Out of index error in menus array");
        }
        foreach (GameObject m in mainMenus)
        {
            m.SetActive(false);
        }
        mainMenus[i].SetActive(true);
    }

    public void SetMenuVisibility(string path, bool s)
    {
        Transform t = transform.Find(path);
        if (t != null)
        {
            t.gameObject.SetActive(s);
        }
        else
        {
            Debug.Log("Missing button");
        }
    }

    public void UpdateUI(int currentMoney, int powerInUse, int powerAvailable)
    {
        money.text = currentMoney.ToString();
        power.text = powerInUse.ToString() + " / "+ powerAvailable.ToString();
        powerSlider.maxValue = powerAvailable;
        powerSlider.value = powerInUse;
    }

    public void SetComponentsAmounts(Transform list)
    {
        Building building = game.GetSelectedBuildingData();
        if (building == null)
        {
            Debug.Log("Error! nothing selected");
            return;
        }
        BuildingData data = game.Buildings[building.ListIndex];
        ComponentCategory[] values = (ComponentCategory[])Enum.GetValues(typeof(ComponentCategory));
        foreach (ComponentCategory cat in values)
        {
            List<BuildingComponent> bc = building.components.FindAll(comp => game.Components[comp.index].category.ToString() == cat.ToString());
            bool limitFound = false;
            ComponentLimit cl = null;
            foreach (ComponentLimit c in data.limits)
            {
                if (c.category == cat)
                {
                    cl = c;
                    limitFound = true;
                    break;
                }
            }
            Transform it = list.Find(cat.ToString());
            if (it != null)
            {
                it.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = bc.Count.ToString();
                if (limitFound)
                {
                    it.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = cl.val.ToString();
                }
            }
        }
    }

    public void ShakeMenu(GameObject menu)
    {
        RectTransform menuRect = menu.GetComponent<RectTransform>();

    }

    public void FillListWithAvailableBuildings(Transform holderList)
    {
        for (int i = 0; i < holderList.childCount; i++)
        {
            Destroy(holderList.GetChild(i).gameObject);
        }

        for (int i = 4; i < game.Buildings.Count; i++)
        {
            GameObject itemInstantiated = Instantiate(buyItemPrefab, holderList, false);
            UIItem uiItem = itemInstantiated.GetComponent<UIItem>();
            uiItem.FillData(game.Buildings[i]);
            uiItem.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("Trying to build " + uiItem.Index);
                game.TryBuild(uiItem.Index);
                game.SwitchState(0);
                FadeOutMenu(uiItem.transform.parent.parent.gameObject);
            });
        }
        Debug.Log(holderList.name + " filled with buildings!");
    }

    public void FillListWithAvailableComponents(int category)
    {
        ComponentCategory selectedCategory = (ComponentCategory)category;
        Transform holderList = selectedDeviceComponentsMenu.transform.GetChild(2);

        for (int i = 0; i < holderList.childCount; i++)
        {
            Destroy(holderList.GetChild(i).gameObject);
        }

        List<ComponentData> categorizedComponents = game.Components.FindAll(comp => comp.category == selectedCategory);
        selectedDeviceComponentsMenu.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = selectedCategory.ToString();

        for (int i = 0; i < categorizedComponents.Count; i++)
        {
            GameObject itemInstantiated = Instantiate(buyItemPrefab, holderList, false);
            UIItem uiItem = itemInstantiated.GetComponent<UIItem>();
            uiItem.FillData(categorizedComponents[i]);
            uiItem.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("Trying to add " + categorizedComponents[uiItem.Index].displayName);
                game.TryAddBuildingComponent(uiItem.Index);
            });
        }
        Debug.Log(holderList.name + " filled with components!");
    }

    public void SetSelectedTileMenu(bool v, BuildingData buildingData)
    {
        if (v)
        {
            Debug.Log("Showing tile menu");
            if (tileMenuMode == TileMenuMode.BUBBLE)
            {
                SpawnSelectionBubble(buildingData);
            }
            else
            {
                SpawnTileSelectionMenu(buildingData);
            }
        }
        else
        {
            Debug.Log("Disabling Tile menu");
            if (powerSlider.gameObject.activeSelf)
            {
                SetDirectionOfFade(2);
                FadeOutMenu(powerSlider.gameObject);
            }
            if (tileMenuMode == TileMenuMode.BUBBLE)
            {
                DespawnTileSelectionBubble();
            }
            else
            {
                FadeOutMenu(tileSelectedMenu);
            }  
        }
    }

    private void SpawnTileSelectionMenu(BuildingData buildingData)
    {
        SetDirectionOfFade(2);
        FadeInMenu(tileSelectedMenu);

        tileSelectedMenu.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = buildingData.name;
        tileSelectedMenu.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = buildingData.description;
        switch (buildingData.Index)
        {
            case 0:
                tileSelectedMenu.transform.GetChild(2).GetChild(0).gameObject.SetActive(true);
                tileSelectedMenu.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
                break;
            case 1:
                tileSelectedMenu.transform.GetChild(2).GetChild(0).gameObject.SetActive(false);
                tileSelectedMenu.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
                break;
            case 2: // Central
                SetDirectionOfFade(0);
                FadeInMenu(powerSlider.gameObject);
                tileSelectedMenu.transform.GetChild(2).GetChild(0).gameObject.SetActive(false);
                tileSelectedMenu.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
                break;
            default:
                tileSelectedMenu.transform.GetChild(2).GetChild(0).gameObject.SetActive(false);
                tileSelectedMenu.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
                break;
        }
        bool upgradeAvailable = buildingData.upgradeBuildingName == "" ? false : true;
        tileSelectedMenu.transform.GetChild(2).GetChild(1).gameObject.SetActive(upgradeAvailable);
        tileSelectedMenu.transform.GetChild(2).GetChild(2).gameObject.SetActive(buildingData.hasComponents);
    }

    private void DespawnTileSelectionBubble()
    {
        Debug.Log("disabling " + game.SelectedTile.ToString() + " bubble");
        GameObject b = activeBubbles.Find(bub => bub.name == game.SelectedTile.ToString());
        menusPos.Remove(game.SelectedTile.ToString());
        activeBubbles.Remove(b);
        FadeOutWorldMenu(b);
    }

    private void SpawnSelectionBubble(BuildingData buildingData)
    {
        GameObject bubbleGO = Instantiate(popUpPrefab, game.SelectedToWorldPosition() + new Vector3(0, 0.75f, 0), Quaternion.identity);
        bubbleGO.transform.SetParent(popUpHolder);
        bubbleGO.name = game.SelectedTile.ToString();
        menusPos.Add(bubbleGO.name, bubbleGO.GetComponent<RectTransform>().position);

        Bubble bubble = bubbleGO.GetComponent<Bubble>();
        activeBubbles.Add(bubbleGO);
        SetDirectionOfFade(2);
        FadeInWorldMenu(bubbleGO);
    }

    #region FadeMenu
    public void FadeInMenu(GameObject menu)
    {
        tweening = true;

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        Vector2 intialPos = menuRect.anchoredPosition + GetTGTPos(menuRect);
        menuRect.anchoredPosition = intialPos;
        Debug.Log("getting " + menu.transform.name + " og pos");
        Vector2 tgtPos = menusPos[menu.transform.name];

        SetAlpha(menu, 0, 1);
        menu.SetActive(true);
        LeanTween.value(menu, (vec) =>
        {
            menuRect.anchoredPosition = vec;
        }, intialPos, tgtPos, fadeTime).setOnComplete(() =>
        {
            tweening = false;
            Debug.Log("TweenFade IN of " + menu.transform.name + " complete!");
        });
    }

    public void FadeOutMenu(GameObject menu)
    {
        tweening = true;

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        Vector2 intialPos = menuRect.anchoredPosition;
        Vector2 tgtPos = menuRect.anchoredPosition + GetTGTPos(menuRect);

        SetAlpha(menu, 1, 0);
        LeanTween.value(menu, (vec) =>
        {
            menuRect.anchoredPosition = vec;
        }, intialPos, tgtPos, fadeTime).setOnComplete(() =>
        {
            tweening = false;
            Debug.Log("TweenFade OUT of " + menu.transform.name + " complete!");
            menu.SetActive(false);
        });
    }
    public void FadeInWorldMenu(GameObject menu)
    {
        tweening = true;

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        Vector2 intialPos = (Vector2)menuRect.transform.position + GetTGTPos(menuRect);
        menuRect.transform.position = intialPos;
        Debug.Log("getting " + menu.transform.name + " og pos");
        Vector2 tgtPos = menusPos[menu.name];

        SetAlpha(menu, 0, 1);
        menu.SetActive(true);
        LeanTween.value(menu, (vec) =>
        {
            menuRect.transform.position = vec;
        }, intialPos, tgtPos, fadeTime).setOnComplete(() =>
        {
            tweening = false;
            Debug.Log("TweenFade IN of " + menu.name + " complete!");
        });
    }

    public void FadeOutWorldMenu(GameObject menu)
    {
        tweening = true;

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        Vector2 intialPos = menuRect.transform.position;
        Vector2 tgtPos = (Vector2)menuRect.transform.position + GetTGTPos(menuRect);

        SetAlpha(menu, 1, 0);
        LeanTween.value(menu, (vec) =>
        {
            menuRect.transform.position = vec;
        }, intialPos, tgtPos, fadeTime).setOnComplete(() =>
        {
            tweening = false;
            Debug.Log("TweenFade OUT of " + menu.name + " complete!");
            menu.SetActive(false);
            Destroy(menu);
        });
    }
    private void SetAlpha(GameObject nextMenu, float from, float to)
    {
        CanvasGroup canvas = nextMenu.GetComponent<CanvasGroup>();
        canvas.interactable = false;
        canvas.alpha = from;
        LeanTween.value(nextMenu, (v) => { canvas.alpha = v; }, from, to, fadeTime).setOnComplete(()=> 
        {
            if(canvas.alpha == 1)
            {
                canvas.interactable = true;
            }
        });
    }

    private Vector2 GetTGTPos(RectTransform rt)
    {
        Vector2 t = Vector2.zero;
        switch (directionToFadeFrom)
        {
            case 0: //North
                t += new Vector2(0, rt.rect.size.y);
                break;
            case 1: //East
                t += new Vector2(rt.rect.size.x, 0);
                break;
            case 2: //South
                t -= new Vector2(0, rt.rect.size.y);
                break;
            case 3: //West
                t -= new Vector2(rt.rect.size.x, 0);
                break;
        }

        return t;
    }

    public void SetDirectionOfFade(int direction)
    {
        directionToFadeFrom = Mathf.Clamp(direction, 0, 3);
    }
    #endregion

    #region ButtonFuctions
    public void BuyButtonFunction() // Va a tratar de comprar la parcela
    {
        game.TryBuild(1); //Intenta comprar parcela
    }
    public void UpgradeButtonFunction()
    {
        game.SwitchState(1); // cambia al modo menu
    }
    public void ComponentButtonFunction()
    {
        DespawnTileSelectionBubble(); //Despawn menu
        game.SwitchState(1); // cambia al modo menu
        SetComponentsAmounts(categoryListHolder);
        FadeInMenu(categoryListHolder.gameObject);
    }
    public void BuildButtonFunction()
    {
        DespawnTileSelectionBubble(); //Despawn menu
        game.SwitchState(1); // cambia al modo menu
        FillListWithAvailableBuildings(buildingListHolder);
        FadeInMenu(buildingListHolder.gameObject);
    } 
    #endregion
}

public enum TileMenuMode
{
    BUBBLE,
    MENU
}
