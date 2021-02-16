using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameController : MonoBehaviour
{
    #region Variables
    [Header("Map Settings")]
    [SerializeField] private int width = 16;
    [SerializeField] private int height = 16;
    [Header("Game Settings")]
    [Range(0.5f, 10)]
    [SerializeField] private float gameTimeSpeedMult = 1;
    [Range(0.2f, 30)]
    [SerializeField] private float buildingRefreshRate = 10;
    [SerializeField] private bool randomBuilds = false;
    [SerializeField] private GameSession currentGameSessionData;
    [Header("References")]
    [SerializeField] private Tile highlightTile;
    [SerializeField] private Tilemap overlayTilemap;
    [SerializeField] private Tilemap buildTilemap;
    [SerializeField] private Tilemap roadTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private UIController ui;
    [SerializeField] private List<BuildingData> buildings;
    [SerializeField] private List<ComponentData> components;
    [Header("Juego Activo")]
    [SerializeField] private Building[] activeBuildings;
    [SerializeField] private PlayerState state = PlayerState.GAME;
    [Header("DEBUG")]
    [SerializeField] private int testIndex = 17;
    [SerializeField] private Vector3Int selectedTile = -Vector3Int.one;
    [Header("Limits")]
    private bool limited = false;
    private Vector3Int limitedToTile = -Vector3Int.one;
    private Action OnLimitedCleared;

    private Building selectedBuilding;
    private bool selected = false;
    private float tPassed = 0;
    private uint totalPassed = uint.MinValue;

    public static int currentWidth { get => GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().width; }
    public static int currentHeight { get => GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().height; }

    public List<BuildingData> Buildings { get => buildings; }
    public List<ComponentData> Components { get => components; }
    public Building[] ActiveBuildings { get => activeBuildings; }
    public Vector3Int SelectedTile { get => selectedTile; }
    public int Width { get => width; }
    public int Height { get => height; }
    public PlayerState State { get => state; }
    public bool RandomBuilds { get => randomBuilds; }
    #endregion

    #region Eventos
    public static Action<BuildActionInfo> BuildingContructed;
    public static Action<ComponentData> ComponentAdded;
    public static Action OnMainScriptReady;
    #endregion

    void Start()
    {
        InitDataIndexation(); //Indexa los datos para facilitar su acceso y ahorrar memoria
        PopulatingActiveBuildings(); //Crear los datos de edificios activos en los que se 

        //Scan Roads
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (roadTilemap.HasTile(new Vector3Int(x, y, 0)))
                {
                    activeBuildings[TilePosToIndex(x, y)] = new Building(buildings[3], TilePosToIndex(x, y));
                }
            }
        }

        activeBuildings[TilePosToIndex(8, 8)] = new Building(buildings[2], TilePosToIndex(8, 8));
        activeBuildings[TilePosToIndex(7, 5)] = new Building(buildings[4], TilePosToIndex(7, 5));

        RefreshMap();
        RefreshGame();

        GetComponent<PGrid>().InitGrid(); //Comentar para PATHFINDING DESHABILITADO
        InputController.OnTap += TileTapped;

        OnMainScriptReady?.Invoke();
    }

    public void SetBuildingElectricVisibility(bool state, Vector3Int cell, BuildingData data)
    {
        TileBase tb = null;
        tb = state ? data.tile : data.tileOff;
        buildTilemap.SetTile(cell, tb);
        Debug.Log("building electricity in " + cell + " is now " + state);
    }

    private void Update()
    {
        RefreshGame();
        if (Input.GetKeyDown(KeyCode.T))
        {
            //TileTapped(new Vector3(0.999f, 3.209f, 0));
            SpawnMoneyBubbles();
        }
    }

    private void RefreshGame()
    {
        tPassed += Time.deltaTime * gameTimeSpeedMult;
        if (tPassed >= buildingRefreshRate)
        {
            tPassed = 0;
            UpdateBuildings(buildingRefreshRate);
            ui.RefreshComponentCategoryBarModeB();
            totalPassed += (uint)buildingRefreshRate;

            if(totalPassed % 3600 == 0)
            {
                SpawnMoneyBubbles();
            }
        }
    }

    public void SpawnMoneyBubbles()
    {
        foreach (Building b in activeBuildings)
        {
            if(b.ListIndex > 3)
            {
                ui.AddMoneyBubble(b.PositionIndex.ToString(), Utils.IndexToTilePos(b.PositionIndex, width), b, -1);
            }
        }
    }

    private void UpdateBuildings(float timePassed)
    {
        Debug.Log("Refreshing buildings");
        int totalPow = 0;
        for (int i = 0; i < activeBuildings.Length; i++)
        {
            activeBuildings[i].UpdateComponentsLife(components, timePassed);
            totalPow += activeBuildings[i].totalPowerConsumption;
        }
        ui.UpdateUI(currentGameSessionData.money, totalPow, 1000);
    }

    public void SetMoney(int money)
    {
        currentGameSessionData.money = money;
        ui.UpdateUI(currentGameSessionData.money);
    }


    /*
    private void SetupTutorial()
    {
        MissionController mc = GameObject.FindGameObjectWithTag("Mission").GetComponent<MissionController>();
        InputController ic = GameObject.FindGameObjectWithTag("Input").GetComponent<InputController>();
        CharController cc = ui.GetComponent<CharController>();

        SwitchState(1);
        cc.AssignAnActionAtEndOfDialog(0, () =>
        {
            mc.CreateCameraMission();
            SwitchState(0);
            ic.LockInput(false, true, true);
        });
        cc.AssignAnActionAtEndOfDialog(1, () =>
        {
            mc.CreateZoomMission();
            ic.LockInput(true, false, true);
            SwitchState(0);
        });
        cc.AssignAnActionAtEndOfDialog(2, () =>
        {
            Vector3 tgtPos = groundTilemap.CellToWorld(new Vector3Int(7, 5, 0)) + new Vector3(0, 0.5f, 0);
            ic.MoveCameraToWorldPosition(tgtPos, 1, () =>
             {
                 SwitchState(1);
                 ui.AddAttentionBubble("Tutorial bubble", new Vector3Int(7, 5, 0));
                 ui.WaitAndExecuteFunction(1, () => {
                     ui.GetComponent<CharController>().ShowDialog(3);
                 });
             });
        });
        cc.AssignAnActionAtEndOfDialog(3, () => //tile selection
        {
            SwitchState(0);
            ic.LockInput(true, true, false);
            LimitSelectionOfTiles(new Vector3Int(7, 5, 0), () =>
            {
                ic.LockInput(true, true, true);
                UnlimitSelectionsOfTiles();
                cc.ShowDialog(4);
                Debug.Log("Context menu opened! Limit removed and bubble Locked");
            });
        });
        cc.AssignAnActionAtEndOfDialog(4, () => //pop up
        {
            ui.ButtonPressed = (s) => {
                if (s == "Components")
                {
                    cc.ShowDialog(5);
                    Debug.LogWarning("Component bubble button pressed");
                    ui.ButtonPressed = null;
                }
            };
        });
        cc.AssignAnActionAtEndOfDialog(5, () => //Categorias
        {
            string[] buttonsToDisable = new string[] {
                "But_CatExit",
                "CUBIERTAS",
                "INTERRUPTORES",
                "TOMACORRIENTES",
                "SENSORES",
                "TERMICAS",
                "But_CompExit"
            };
            ui.DisableButton(buttonsToDisable);
            ui.ButtonPressed = (s) => {
                if (s == "ILUMINACION")
                {
                    cc.ShowDialog(6);
                    Debug.LogWarning("Boton Iluminacion presionado");
                    ui.ButtonPressed = null;
                }
            };
        });
        cc.AssignAnActionAtEndOfDialog(6, () => //Explain Bar
        {
            ui.ButtonPressed = (s) => {
                if (s == "0" || s == "1" || s == "2" || s == "3")
                {
                    string[] buttonsToDisable = new string[] {
                        "0",
                        "1",
                        "2",
                        "3"
                    };
                    ui.DisableButton(buttonsToDisable);
                    cc.ShowDialog(7);
                    Debug.LogWarning("Producto Comprado");
                    ui.EnableADisabledButton("But_CompExit");
                    ui.EnableADisabledButton("But_CatExit");
                    ui.ButtonPressed = (exit) => {
                        if(exit == "But_CatExit")
                        {
                            ic.MoveCameraToWorldPosition(overlayTilemap.CellToWorld(new Vector3Int(8,8,0)), 3, ()=> {
                                cc.ShowDialog(8);
                                ui.ButtonPressed = null;
                            });
                        }
                    };
                }
            };
        });
        cc.AssignAnActionAtEndOfDialog(8, () =>
        {
            Debug.LogWarning("Trying to select 8,7 at "+ state +" state");
            LimitSelectionOfTiles(new Vector3Int(8, 7, 0), () => {
                cc.ShowDialog(10);
            });
            ic.MoveCameraToWorldPosition(overlayTilemap.CellToWorld(new Vector3Int(8, 7, 0)) + new Vector3(0,0.5f,0), 0.8f, () => {
                cc.ShowDialog(9);
                ui.ButtonPressed = null;
            });
        });
        cc.AssignAnActionAtEndOfDialog(9, () =>
        {
            ic.LockInput(true, true, false);
        });
        cc.AssignAnActionAtEndOfDialog(10, () =>
        {
            ic.LockInput(true, true, true);
            ui.ButtonPressed = (exit) => {
                if (exit == "Buy")
                {
                    cc.ShowDialog(11);
                    ui.ButtonPressed = null;
                    UnlimitSelectionsOfTiles();
                    LimitSelectionOfTiles(new Vector3Int(8, 7, 0), null);
                    Debug.LogWarning("Error Step");
                    ui.EnableAllButtons();
                    ui.DisableButton(new string[] { "But_BuildExit" });
                }
            };

        });
        cc.AssignAnActionAtEndOfDialog(11, () =>
        {
            ic.LockInput(true, true, false);
            ui.ButtonPressed = (build) => {
                if (build == "Build")
                {
                    cc.ShowDialog(12);
                    ui.ButtonPressed = null;
                }
            };
        });
        cc.AssignAnActionAtEndOfDialog(12, () =>
        {
            mc.CreateAnyBuildingMission();
        });
        cc.AssignAnActionAtEndOfDialog(13, () =>
        {
            SwitchState(0);
            ic.LockInput(false,false,false);
            UnlimitSelectionsOfTiles();
            ui.EnableAllButtons();
        });
        //Start Tutorial
        cc.ShowDialog(0);
    }
    */
    public void UnlimitSelectionsOfTiles()
    {
        limited = false;
        limitedToTile = -Vector3Int.one ;
        OnLimitedCleared = null;
        Debug.Log("Limit selection of tiles disabled");
    }

    public void LimitSelectionOfTiles(Vector3Int cell, Action _OnTileSelected)
    {
        limited = true;
        limitedToTile = cell;
        OnLimitedCleared = _OnTileSelected;
        Debug.Log("Limit selection of tiles active... Can only select" + cell);
    }

    public Vector3 TryGetActiveBuildingWorldPos(BuildingData buildingData)
    {
        Building b = activeBuildings.First(val => val.ListIndex == buildingData.Index);
        if(b != null)
        {
            Vector3Int cell = Utils.IndexToTilePos(b.PositionIndex, width);
            return groundTilemap.CellToWorld(cell);
        }
        else
        {
            return -Vector3.one;
        }
    }

    public void AddMoney(int money)
    {
        if(currentGameSessionData != null)
        {
            currentGameSessionData.money += money;
        }
    }

    public bool HasActiveBuilding(BuildingData buildingData)
    {
        Building b = activeBuildings.First(val => val.ListIndex == buildingData.Index);
        if (b != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Building GetSelectedBuildingData()
    {
        if(selected)
        {
            return activeBuildings[TilePosToIndex(selectedTile.x, selectedTile.y)];
        }
        else
        {
            return null;
        }
    }

    private void PopulatingActiveBuildings()
    {
        activeBuildings = new Building[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int posIndex = TilePosToIndex(x, y);
                activeBuildings[posIndex] = new Building(buildings[0], posIndex);
            }
        }
    }

    
    private void InitDataIndexation()
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            buildings[i].SetIndex(i);
        }
        for (int i = 0; i < components.Count; i++)
        {
            components[i].SetIndex(i);
        }
    }

    /// <summary>
    /// Intentara añadir un edificio de la lista "building" usando el indice propocionado y la posicion seleccionada
    /// </summary>
    /// <param name="index"></param>
    public void TryAddBuildingComponent(int index)
    {
        if (selected)
        {
            if (currentGameSessionData != null)
            {
                ComponentData componentData = components[index]; //Datos del componente a añadir
                Building building = activeBuildings[TilePosToIndex(selectedTile.x, selectedTile.y)]; // Edificio a modificar
                BuildingData buildingData = buildings[building.ListIndex]; // Data para referencias del edificio

                ComponentLimit currentAmount = building.currentCompAmounts.Find(amount => amount.category == componentData.category);// Trata de obtener la cantidad de esos dispositivos
                int currentDeviceAmount = 0;
                if (currentAmount != null)
                {
                    currentDeviceAmount = currentAmount.val;
                } 
                int maxDeviceAllowed = -1; // Trata de obtener la cantidad maxima permitida de esos dispositivos
                foreach (ComponentLimit c in buildingData.limits)
                {
                    if (c.category == components[index].category)
                    {
                        maxDeviceAllowed = c.val;
                        break;
                    }
                }
                if(maxDeviceAllowed > 0)
                {
                    if(currentDeviceAmount >= maxDeviceAllowed)
                    {
                        Debug.Log("No more slots available");
                        GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(6);
                        return;
                    }
                    if (currentGameSessionData.money >= components[index].cost)
                    {
                        AddComponent(index);
                    }
                    else
                    {
                        GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(6);
                        Debug.Log(" NA MANEY !!");
                    }
                }
                else
                {
                    Debug.LogWarning(" No Limit set for "+ components[index].displayName);
                    if (currentGameSessionData.money >= components[index].cost)
                    {
                        AddComponent(index);
                    }
                    else
                    {
                        GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(6);
                        Debug.Log(" NA MANEY !!");
                    }
                }
            }
            else
            {
                Debug.LogError("No session Data!!");
                GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(6);
            }
        }
        else
        {
            Debug.Log("No tile selected");
            GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(6);
        }
    }

    /// <summary>
    /// Añade un componente a cada edificio
    /// </summary>
    /// <param name="index"></param>
    private void AddComponent(int index)
    {
        Building targetBuilding = activeBuildings[TilePosToIndex(selectedTile.x, selectedTile.y)];

        targetBuilding.AddBuildingComponent(components[index]);
        SetBuildingElectricVisibility(true, new Vector3Int(selectedTile.x, selectedTile.y, 0), buildings[targetBuilding.ListIndex]);
        currentGameSessionData.money -= components[index].cost;
        ui.UpdateUI(currentGameSessionData.money);
        ComponentAdded?.Invoke(components[index]);
        GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(5);
    }

    public void TryBuild(int index)
    {
        bool builded = false;
        Debug.LogWarning("Trying to build " + index);
        int indexSelected = index;
        if (index > 3 && randomBuilds)
        {
            indexSelected =  Random.Range(4, buildings.Count);
        }
        Debug.LogWarning("TryBuild has commited to " + indexSelected);

        if (selected)
        {
            if(currentGameSessionData != null)
            {
                if(currentGameSessionData.money >= buildings[indexSelected].buildingCost)
                {
                    builded = true;
                    Build(selectedTile, indexSelected);
                    currentGameSessionData.money -= buildings[indexSelected].buildingCost;
                    ui.UpdateUI(currentGameSessionData.money);
                    DeselectTile();
                    GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(5);
                }
                else
                {
                    Debug.Log(" NA MANEY !!");
                    GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(6);
                }
            }
            else
            {
                Debug.LogError("No session Data!!");
            }
        }
        else
        {
            Debug.Log("No tile selected");
        }
        BuildActionInfo info = new BuildActionInfo(CellToWorldPosition(selectedTile), selectedTile, buildings[indexSelected], builded);
        BuildingContructed?.Invoke(info);
    }

    public void Build(Vector3Int tilePos, int buildingIndex)
    {
        activeBuildings[TilePosToIndex(tilePos.x, tilePos.y)] = new Building(buildings[buildingIndex], TilePosToIndex(tilePos.x, tilePos.y));
        RefreshMap();
        Debug.Log("Building constructed");
    }

    /// <summary>
    /// Bkn???
    /// </summary>
    /// <returns></returns>
    private int CalculatePowerUsage()
    {
        int total = 0;
        foreach(Building b in activeBuildings)
        {
            total += b.totalPowerConsumption;
        }
        return total;
    }

    /// <summary>
    /// De acuerdo a los edificios dibuja el mapa correspondiente
    /// </summary>
    private void RefreshMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Building b = activeBuildings[TilePosToIndex(x, y)];
                BuildingData bd = buildings[b.ListIndex];
                if(bd.Index != 3) //no es igual a calle
                {
                    buildTilemap.SetTile(new Vector3Int(x, y, 0), bd.tile);
                }
                //buildTilemap.SetTile(new Vector3Int(x, y, 0), bd.tile);
            }
        }
    }


    #region Cambio de estados
    /// <summary>
    /// Cambia el estado de juego
    /// </summary>
    /// <param name="newState"></param>
    public void SwitchState(int newState)
    {
        PlayerState s = (PlayerState)newState;
        SwitchState(s);
    }
    /// <summary>
    /// Cambia el estado de juego
    /// </summary>
    /// <param name="newState"></param>
    public void SwitchState(PlayerState newState)
    {
        //Logica
        state = newState;
    }
    #endregion

    /// <summary>
    /// Cuando el jugador presiona en una celda
    /// </summary>
    /// <param name="worldPos"></param>
    public void TileTapped(Vector3 worldPos)
    {
        if (state == PlayerState.MENUS || UIController.tweening)
        {
            Debug.Log("Using Menus!!");
            return;
        }
        if (selected)
        {
            DeselectTile();
        }
        else
        {
            SelectTile(worldPos);
            if(selected)
            {
                Debug.Log("Sending info of " + selectedTile + " index " + TilePosToIndex(selectedTile.x, selectedTile.y));
                int selectedBuilding = activeBuildings[TilePosToIndex(selectedTile.x, selectedTile.y)].ListIndex;
                ui.SetSelectedTileMenu(true, buildings[selectedBuilding]);
            }
        }
    }

    /// <summary>
    /// Trata de seleccionar una celda valida
    /// </summary>
    /// <param name="worldPos"></param>
    private void SelectTile(Vector3 worldPos)
    {
        //Calculate new selected pos
        Vector3Int newSelectedPos = overlayTilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
        if(limited)
        {
            if(newSelectedPos == limitedToTile)
            {
                OnLimitedCleared?.Invoke();
            }
            else
            {
                Debug.LogWarning("Limited Selection Active!!");
                return;
            }
        }
        //Clamping the new selected pos;
        newSelectedPos = new Vector3Int(Mathf.Clamp(newSelectedPos.x, 0, width - 1), Mathf.Clamp(newSelectedPos.y, 0, height - 1), 0);
        //Updating view;
        overlayTilemap.SetTile(newSelectedPos, highlightTile);
        overlayTilemap.RefreshTile(newSelectedPos);
        //Set selected tile;
        selectedTile = newSelectedPos;
        testIndex = TilePosToIndex(selectedTile.x, selectedTile.y);
        Debug.Log("Buildings index = " + testIndex);
        selected = true;
    }

    public void DeselectTile()
    {
        //Clear previous selection
        overlayTilemap.SetTile(selectedTile, null);
        overlayTilemap.RefreshTile(selectedTile);
        selected = false;
        ui.SetSelectedTileMenu(false, null);
    }

    private void OnDestroy()
    {
        InputController.OnTap -= TileTapped;
        Debug.Log("Cleaned Events");
    }

    public int TilePosToIndex(int x, int y)
    {
        int index = x + y * width;
        return index;
    }
    public Vector3Int IndexToTilePos(int indx)
    {
        int y = (int)(indx / width);
        int x = indx - (y * width);
        return new Vector3Int(x, y, 0);
    }

    public Vector3 SelectedToWorldPosition()
    {
        return groundTilemap.CellToWorld(selectedTile);
    }

    public Vector3 CellToWorldPosition(Vector3Int cell)
    {
        return groundTilemap.CellToWorld(cell);
    }

    public void TestIndex()
    {
        Debug.Log("Index " + testIndex + " is " + IndexToTilePos(testIndex));
    }
}
