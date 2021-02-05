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
    [SerializeField] private CharController c;
    [Space(5)]
    [Header("Game UI")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private TextMeshProUGUI power;
    [SerializeField] private TextMeshProUGUI money;
    [Header("Tile manipulation menus")]
    [SerializeField] private GameObject tileSelectedMenu;
    [SerializeField] private GameObject selectedDeviceComponentsMenu;
    [Header("NewMenus")]
    [SerializeField] private GameObject buyNewItemPrefab;
    [SerializeField] private GameObject newCategoriesListMenu;
    [SerializeField] private GameObject newComponentsListMenu;
    [SerializeField] private GameObject newBuildingListHolder;
    [SerializeField] private GameObject characterMenu;
    [Header("Prefabs")]
    [SerializeField] private GameObject buyItemPrefab;
    [SerializeField] private GameObject popUpPrefab;
    [SerializeField] private GameObject bubbleButtonPrefab;
    [SerializeField] private Transform popUpHolder;
    private List<GameObject> activeBubbles;
    [Header("Settings")]
    [SerializeField] private float fadeTime = 0.1f;
    [SerializeField] private TileMenuMode tileMenuMode = TileMenuMode.MENU;
    [Header("Icons")]
    [SerializeField] private Sprite[] icons;
    [Space(5)]
    [Header("DEBUG!")]
    public Button buyButton;

    private bool tileBubbleActive = false;
    private int directionToFadeFrom = 0;
    private Dictionary<string, Vector2> menusPos;
    private bool dialog = false;
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            DebugActiveMenuPositions();
        }
        if (Input.GetKeyDown(KeyCode.L) && !dialog)
        {
            dialog = true;
            game.SwitchState(1);
            FadeInMenu(characterMenu);
            InputController ic = GameObject.FindGameObjectWithTag("Input").GetComponent<InputController>();
            c.DebugADialogFromACharacter(0, Emotion.HAPPY, "Hola Soy INSERT NAME! Ahora voy a decir incoherencias y luego centrare la camara en la central electrica" +
                "svgggage asdasdadavndskuhrnbekjvybskjvhyrbvkjsvnbkjnbvrknjbvksvyrnbvkvnfjvhnvnkg Centracion!!", ()=> {
                    Debug.Log("Moving camera to position");
                    ic.MoveCameraToWorldPosition(game.TryGetActiveBuildingWorldPos(game.Buildings[2]), ()=>
                    {
                        game.SwitchState(0);
                        dialog = false;
                    });
                });
        }
    }

    private void DebugActiveMenuPositions()
    {
        string s = "Save menu positions \n";
        foreach (KeyValuePair<string, Vector2> m in menusPos)
        {
            s += m.Key + " at " + m.Value.ToString() + "\n";
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
        for (int i = 0; i < holderList.childCount; i++) //
        {                                               //
            Destroy(holderList.GetChild(i).gameObject); //
        }                                               //Limpia la lista!

        float contentSize = 0;
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

    public void FillNewListWithAvailableComponents(int category)
    {
        ComponentCategory selectedCategory = (ComponentCategory)category;
        Transform holderList = newComponentsListMenu.transform.GetChild(0).GetChild(0).GetChild(0);

        for (int i = 0; i < holderList.childCount; i++) //
        {                                               //
            Destroy(holderList.GetChild(i).gameObject); //
        }                                               //Limpia la lista!

        List<ComponentData> categorizedComponents = game.Components.FindAll(comp => comp.category == selectedCategory); //Encuentra todos los comp de la misma categoria seleccionada
        newComponentsListMenu.transform.Find("Img_Label/Txt_Label").GetComponent<TextMeshProUGUI>().text = selectedCategory.ToString(); //Asigna el titulo del menu

        for (int i = 0; i < categorizedComponents.Count; i++)
        {
            GameObject itemInstantiated = Instantiate(buyNewItemPrefab, holderList, false);
            itemInstantiated.transform.name = categorizedComponents[i].Index.ToString();
            itemInstantiated.transform.Find("Img_Icon").GetComponent<Image>().sprite = categorizedComponents[i].icon;
            itemInstantiated.transform.Find("Img_Icon/Img_Label/Txt_Cost").GetComponent<TextMeshProUGUI>().text = "$ "+categorizedComponents[i].cost.ToString();
            itemInstantiated.transform.Find("Txt_Label").GetComponent<TextMeshProUGUI>().text = categorizedComponents[i].displayName;

            itemInstantiated.GetComponent<Button>().onClick.AddListener(() =>
            {
                int pint = int.Parse(itemInstantiated.transform.name);
                Debug.Log("Trying to add " + game.Components[pint].displayName);
                game.TryAddBuildingComponent(pint);
            });
        }
        Debug.Log(holderList.name + " filled with components!");
    }

    public void FillNewListWithAvailableBuildings(Transform buildingMenu)
    {
        Transform holderList = buildingMenu.Find("Scr_BuildingsHolder/Viewport/Content");

        for (int i = 0; i < holderList.childCount; i++) //
        {                                               //
            Destroy(holderList.GetChild(i).gameObject); //
        }                                               //Limpia la lista!
        float contentSize = 0;
        for (int i = 4; i < game.Buildings.Count; i++)
        {
            GameObject item = Instantiate(buyNewItemPrefab, holderList, false);
            item.transform.name = game.Buildings[i].Index.ToString();
            contentSize += item.GetComponent<RectTransform>().sizeDelta.x - 100;
            //contentSize += item.GetComponent<RectTransform>().rect.size.x;
            item.transform.Find("Img_Icon").GetComponent<Image>().sprite = game.Buildings[i].icon;
            item.transform.Find("Img_Icon/Img_Label/Txt_Cost").GetComponent<TextMeshProUGUI>().text = "$ " + game.Buildings[i].buildingCost.ToString();
            item.transform.Find("Txt_Label").GetComponent<TextMeshProUGUI>().text = game.Buildings[i].name;
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                int pint = int.Parse(item.transform.name);
                Debug.Log("Trying to build " + game.Buildings[pint].Index);
                game.TryBuild(game.Buildings[pint].Index);
                game.SwitchState(0);
                FadeOutMenu(newBuildingListHolder);
            });
        }
        RectTransform holderSize = holderList.GetComponent<RectTransform>();
        holderSize.sizeDelta = new Vector2(contentSize, holderSize.sizeDelta.y);
        Debug.Log(holderList.name + " filled with buildings!");
    }

    public void SetSelectedTileMenu(bool v, BuildingData buildingData)
    {
        if (v)
        {
            Debug.Log("Showing tile menu");
            if (tileMenuMode == TileMenuMode.BUBBLE)
            {
                SpawnTileSelectionBubble(buildingData);
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
                if(tileBubbleActive)
                {
                    DespawnTileSelectionBubble();
                }
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

    #region Bubble
    public void AddAttentionBubble(string id, Vector3Int tilePos)
    {
        string n = id + " - notification";
        if (menusPos.ContainsKey(n))
        {
            Debug.Log("Menu already existed");
            return;
        }
        GameObject bubbleGO = Instantiate(popUpPrefab, game.CellToWorldPosition(tilePos) + new Vector3(0, 0.75f, 0), Quaternion.identity); //Crea la burbuja
        bubbleGO.transform.SetParent(popUpHolder);
        bubbleGO.name = n;

        GameObject image = Instantiate(bubbleButtonPrefab, bubbleGO.transform.GetChild(0), false);
        image.GetComponent<Button>().enabled = false;
        image.GetComponent<Image>().enabled = false;
        image.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Comp Roto";
        image.transform.GetChild(1).GetComponent<Image>().sprite = icons[4];
        menusPos.Add(bubbleGO.name, bubbleGO.GetComponent<RectTransform>().position);
        SetDirectionOfFade(2);
        GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(7);
        FadeInWorldMenu(bubbleGO, () => StartCoroutine(WaitAndExecute(1, () => {
            FadeOutWorldMenu(bubbleGO);
            menusPos.Remove(bubbleGO.name);
        })));
    }

    public void AddAddedBubble(string id, Vector3Int tilePos, float t)
    {
        string n = id + " - added a component";
        if (menusPos.ContainsKey(n))
        {
            //Debug.Log("Menu already existed");
            return;
        }
        GameObject bubbleGO = Instantiate(popUpPrefab, game.CellToWorldPosition(tilePos) + new Vector3(0, 0.75f, 0), Quaternion.identity); //Crea la burbuja
        bubbleGO.transform.SetParent(popUpHolder);
        bubbleGO.name = n;

        GameObject image = Instantiate(bubbleButtonPrefab, bubbleGO.transform.GetChild(0), false);
        image.GetComponent<Button>().enabled = false;
        image.GetComponent<Image>().enabled = false;
        image.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Gracias!!";
        image.transform.GetChild(1).GetComponent<Image>().sprite = icons[5];
        menusPos.Add(bubbleGO.name, bubbleGO.GetComponent<RectTransform>().position);
        SetDirectionOfFade(2);
        GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(7);
        FadeInWorldMenu(bubbleGO, () => StartCoroutine(WaitAndExecute(t, () => {
            FadeOutWorldMenu(bubbleGO);
            menusPos.Remove(bubbleGO.name);
        })));
    }

    private IEnumerator WaitAndExecute(float time, Action onTimerEnd)
    {
        yield return new WaitForSeconds(time);
        onTimerEnd?.Invoke();
    }

    private void DespawnTileSelectionBubble()
    {
        Debug.Log("disabling " + game.SelectedTile.ToString() + " bubble");
        GameObject b = activeBubbles.Find(bub => bub.name == game.SelectedTile.ToString());
        activeBubbles.Remove(b);
        FadeOutWorldMenu(b);
        menusPos.Remove(game.SelectedTile.ToString());
        tileBubbleActive = false;
    }

    private void SpawnTileSelectionBubble(BuildingData buildingData)
    {
        GameObject bubbleGO = Instantiate(popUpPrefab, game.SelectedToWorldPosition() + new Vector3(0, 0.75f, 0), Quaternion.identity);
        bubbleGO.transform.SetParent(popUpHolder);
        bubbleGO.name = game.SelectedTile.ToString();
        menusPos.Add(bubbleGO.name, bubbleGO.GetComponent<RectTransform>().position);

        Bubble bubble = bubbleGO.GetComponent<Bubble>();

        switch (buildingData.Index)
        {
            case 0: // Empty
                GameObject buyBut = Instantiate(bubbleButtonPrefab, bubbleGO.transform.GetChild(0), false);
                bubble.AddButton(buyBut, "Comprar", icons[0], BuyButtonFunction);
                break;
            case 1: //Construccion
                GameObject buildBut = Instantiate(bubbleButtonPrefab, bubbleGO.transform.GetChild(0), false);
                bubble.AddButton(buildBut, "Construir", icons[3], BuildButtonFunction);
                break;
            case 2: // Central
                SetDirectionOfFade(0);
                FadeInMenu(powerSlider.gameObject);
                break;
            case 3: // calle
                Debug.Log("Just tha street");
                break;
        }
        bool upgradeAvailable = buildingData.upgradeBuildingName == "" ? false : true;
        if (upgradeAvailable)
        {
            GameObject upgradeBut = Instantiate(bubbleButtonPrefab, bubbleGO.transform.GetChild(0), false);
            bubble.AddButton(upgradeBut, "Mejorar", icons[1], UpgradeButtonFunction);
        }
        if (buildingData.hasComponents)
        {
            GameObject compBut = Instantiate(bubbleButtonPrefab, bubbleGO.transform.GetChild(0), false);
            bubble.AddButton(compBut, "Componentes", icons[2], ComponentButtonFunction);
        }
        activeBubbles.Add(bubbleGO);
        SetDirectionOfFade(2);
        FadeInWorldMenu(bubbleGO, null);
        tileBubbleActive = true;
    } 
    #endregion

    #region FadeMenu
    public void FadeInMenu(GameObject menu)
    {
        tweening = true;

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        Vector2 intialPos = menuRect.anchoredPosition + GetTGTPos(menuRect);
        menuRect.anchoredPosition = intialPos;
        //Debug.Log("getting " + menu.transform.name + " og pos");
        Vector2 tgtPos = menusPos[menu.transform.name];

        SetAlpha(menu, 0, 1);
        menu.SetActive(true);
        LeanTween.value(menu, (vec) =>
        {
            menuRect.anchoredPosition = vec;
        }, intialPos, tgtPos, fadeTime).setOnComplete(() =>
        {
            tweening = false;
            //Debug.Log("TweenFade IN of " + menu.transform.name + " complete!");
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
            //Debug.Log("TweenFade OUT of " + menu.transform.name + " complete!");
            menu.SetActive(false);
        });
    }
    public void FadeInWorldMenu(GameObject menu, Action onFadeInEnd)
    {
        tweening = true;

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        Vector2 intialPos = (Vector2)menuRect.transform.position + GetTGTPos(menuRect);
        menuRect.transform.position = intialPos;
        //Debug.Log("getting " + menu.transform.name + " og pos");
        Vector2 tgtPos = menusPos[menu.name];

        SetAlpha(menu, 0, 1);
        menu.SetActive(true);
        LeanTween.value(menu, (vec) =>
        {
            menuRect.transform.position = vec;
        }, intialPos, tgtPos, fadeTime).setOnComplete(() =>
        {
            tweening = false;
            //Debug.Log("TweenFade IN of " + menu.name + " complete!");
            onFadeInEnd?.Invoke();
        });
    }

    public void FadeOutWorldMenu(GameObject menu)
    {
        tweening = true;
        Debug.Log("menu in question: " + menu.transform.name);
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
            //Debug.Log("TweenFade OUT of " + menu.name + " complete!");
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
        DespawnTileSelectionBubble(); //Despawn world menu
        game.SwitchState(1); // cambia al modo menu
        //SetComponentsAmounts(categoryListHolder); 
        FadeInMenu(newCategoriesListMenu.gameObject);
    }
    public void BuildButtonFunction()
    {
        DespawnTileSelectionBubble(); //Despawn menu
        game.SwitchState(1); // cambia al modo menu
        FillNewListWithAvailableBuildings(newBuildingListHolder.transform);
        FadeInMenu(newBuildingListHolder);
    } 
    #endregion
}

public enum TileMenuMode
{
    BUBBLE,
    MENU
}
