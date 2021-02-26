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
    [SerializeField] private MapDrawController map;
    [SerializeField] private UIController ui;
    [SerializeField] private SelectionHandler selectionHandler;
    [Header("Juego Activo")]
    [SerializeField] private Building[] activeBuildings;
    [SerializeField] private PlayerState state = PlayerState.GAME;

    [Header("--------- REFERENCE DATA ----------")]
    [HideInInspector]
    [SerializeField] private List<BuildingData> buildings;
    [HideInInspector]
    [SerializeField] private List<ComponentData> components;
    [Min(0)]
    [Tooltip("Todos los edificios cuyo indice sea menor o igual a este numero no podra ser construidos/listados")]
    [SerializeField] private int nonAllowedToBuildMaxIndex = 3;

    private bool limited = false;
    private Vector3Int limitedToTile = -Vector3Int.one;
    private Action OnLimitedCleared;

    private float tPassed = 0;
    private uint totalPassed = uint.MinValue;
    

    public static int currentWidth { get => GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().width; }
    public static int currentHeight { get => GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().height; }

    public List<BuildingData> Buildings { get => buildings; }
    public List<ComponentData> Components { get => components; }
    public Building[] ActiveBuildings { get => activeBuildings; }
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

        map.ScanRoads();

        activeBuildings[Utils.TilePosToIndex(8, 7, width)] = new Building(buildings[2], Utils.TilePosToIndex(8, 7, width));
        map.LockCell(new Vector3Int(9, 7, 0));
        activeBuildings[Utils.TilePosToIndex(7, 5, width)] = new Building(buildings[4], Utils.TilePosToIndex(7, 5, width));

        DrawMap();
        RefreshGame();

        GetComponent<PGrid>().InitGrid(); //Comentar para PATHFINDING DESHABILITADO
        InputController.OnTap += TileTapped;

        OnMainScriptReady?.Invoke();
    }

    private void Update()
    {
        RefreshGame();
        if (Input.GetKeyDown(KeyCode.T))
        {
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
            BuildingData bd = Buildings[b.ListIndex];

            if(bd.rent > 0)
            {
                if(b.components == null || b.components.Count == 0)
                {
                    Debug.Log(b.PositionIndex + " has no components");
                }
                else
                {
                    ui.AddMoneyBubble(b.PositionIndex.ToString(), Utils.IndexToTilePos(b.PositionIndex, width), b, -1);
                }
            }
        }
    }

    private void UpdateBuildings(float timePassed)
    {
        Debug.Log("Refreshing buildings");
        int totalPow = 0;
        for (int i = 0; i < activeBuildings.Length; i++)
        {
            activeBuildings[i].UpdateComponentsLife(this, map, timePassed);
            totalPow += activeBuildings[i].totalPowerConsumption;
        }
        ui.UpdateUI(currentGameSessionData.money, totalPow, 1000);
        DrawMap();
    }

    public void SetMoney(int money)
    {
        currentGameSessionData.money = money;
        ui.UpdateUI(currentGameSessionData.money);
    }

    public void UnlimitSelectionsOfTiles()
    {
        limited = false;
        limitedToTile = -Vector3Int.one ;
        OnLimitedCleared = null;
        Debug.Log("Limit selection of tiles disabled");
    }

    

    /// <summary>
    /// Limita la seleccion de tiles a una unica casilla
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="_OnTileSelected"></param>
    public void LimitSelectionOfTiles(Vector3Int cell, Action _OnTileSelected)
    {
        limited = true;
        limitedToTile = cell;
        OnLimitedCleared = _OnTileSelected;
        Debug.Log("Limit selection of tiles active... Can only select" + cell);
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
        if(selectionHandler.HasSelection())
        {
            return selectionHandler.Selection.Building;
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
                int posIndex = Utils.TilePosToIndex(x, y, width);
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
    /// Intentara añadir un edificio de la lista "building" usando el indice propocionado y la posicion seleccionada REFACTORIAZARRR!
    /// </summary>
    /// <param name="index"></param>
    public void TryAddBuildingComponent(int index)
    {
        if(!selectionHandler.HasSelection())
        {
            Debug.Log("No selection");
            GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(6);
            return;
        }
        if(currentGameSessionData == null)
        {
            Debug.Log("No session data");
            GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(6);
            return;
        }
        if(currentGameSessionData.money < components[index].cost)
        {
            GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(6);
            Debug.Log("Na money");
            return;
        }

        ComponentData componentData = components[index]; //Datos del componente a añadir
        Building building = selectionHandler.Selection.Building;
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
        if (maxDeviceAllowed > 0 && currentDeviceAmount >= maxDeviceAllowed) //Si hay un limite de componentes
        {
            Debug.Log("No more slots available");
            GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(6);
            return;
        }
        AddComponent(index);
    }

    /// <summary>
    /// Añade un componente a cada edificio
    /// </summary>
    /// <param name="index"></param>
    private void AddComponent(int index)
    {
        //Building targetBuilding = activeBuildings[Utils.TilePosToIndex(selectedTile.x, selectedTile.y, width)];
        Building targetBuilding = selectionHandler.Selection.Building;
        Vector3Int cell = selectionHandler.Selection.CellPosition;

        targetBuilding.AddBuildingComponent(components[index]);
        map.SetBuildingElectricVisibility(true, new Vector3Int(cell.x, cell.y, 0), buildings[targetBuilding.ListIndex]);
        currentGameSessionData.money -= components[index].cost;
        ui.UpdateUI(currentGameSessionData.money);
        ComponentAdded?.Invoke(components[index]);
        GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(5);
    }

    /// <summary> 
    /// Va a intentar a construir un edificio REFACTORIZAR
    /// </summary>
    /// <param name="index"></param>
    public void TryBuild(int index)
    {
        bool builded = false;
        Debug.LogWarning("Trying to build " + index);
        int indexSelected = index;

        if (index > nonAllowedToBuildMaxIndex && randomBuilds)
        {
            indexSelected =  Random.Range(4, buildings.Count);
        }
        Debug.LogWarning("TryBuild has commited to " + indexSelected);

        if(!selectionHandler.HasSelection())
        {
            Debug.Log("No tile selected");
            return;
        }
        if(currentGameSessionData == null)
        {
            Debug.Log("No Session data");
            return;
        }
        if(currentGameSessionData.money < buildings[indexSelected].buildingCost)
        {
            Debug.Log(" NA MANEY !!");
            GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(6);
            return;
        }

        Build(selectionHandler.Selection.CellPosition, indexSelected);
        builded = true;
        currentGameSessionData.money -= buildings[indexSelected].buildingCost;
        ui.UpdateUI(currentGameSessionData.money);
        GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(8);
        BuildActionInfo info = new BuildActionInfo(map.CellToWorldPosition(selectionHandler.Selection.CellPosition), selectionHandler.Selection.CellPosition, buildings[indexSelected], builded);
        BuildingContructed?.Invoke(info);
        DeselectTile();
    }

    public void Build(Vector3Int tilePos, int buildingIndex)
    {
        BuildingData selectedBuildingData = buildings[buildingIndex];
        int buildGridIndex = Utils.TilePosToIndex(tilePos.x, tilePos.y, width);
        activeBuildings[buildGridIndex] = new Building(selectedBuildingData, buildGridIndex);
        RefreshCell(tilePos);
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
    /// Refresca la celda en cuestion de acuerdo
    /// </summary>
    /// <param name="cell"></param>
    private void RefreshCell(Vector3Int cell)
    {
        Building b = activeBuildings[Utils.TilePosToIndex(cell.x, cell.y, width)];
        BuildingData bd = buildings[b.ListIndex];
        if (bd.Index != 3) //Refactorizar
        {
            if (b.components == null || b.components.Count == 0)
            {
                map.DrawTile(cell, bd.tileOff);
            }
            else
            {
                map.DrawTile(cell, bd.tile);
            }
        }
        map.DrawLockedTiles();
    }

    /// <summary>
    /// De acuerdo a los edificios dibuja el mapa correspondiente(HARCODED IGNORE STREET)
    /// </summary>
    private void DrawMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Building b = activeBuildings[Utils.TilePosToIndex(x, y, width)];
                BuildingData bd = buildings[b.ListIndex];
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (bd.Index != 3) //Esto indica que no se dibujara la calle en la capa de edificios
                {
                    if (b.components == null || b.components.Count == 0)
                    {
                        map.DrawTile(cell, bd.tileOff);
                    }
                    else
                    {
                        map.DrawTile(cell, bd.tile);
                    }
                }
            }
        }
        map.DrawLockedTiles();
        Debug.Log("Map Drawed");
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
        switch (newState)
        {
            case PlayerState.GAME:
                GameObject.FindGameObjectWithTag("Input").GetComponent<InputController>().LockInput(false, false, false);
                break;
            case PlayerState.MENUS:
                GameObject.FindGameObjectWithTag("Input").GetComponent<InputController>().LockInput(false, false, true);
                break;
        }


        state = newState;
    }
    #endregion

    /// <summary>
    /// Cuando el jugador hace un tap o click...
    /// </summary>
    /// <param name="worldPos"></param>
    public void TileTapped(Vector3 worldPos)
    {
        if (UIController.tweening)
        {
            Debug.Log("Using Menus!!");
            return;
        }
        if (selectionHandler.HasSelection())
        {
            DeselectTile();
        }
        else
        {
            SelectTile(worldPos);
        }
    }

    /// <summary>
    /// Trata de seleccionar una celda valida
    /// </summary>
    /// <param name="worldPos"></param>
    private void SelectTile(Vector3 worldPos)
    {
        //Calculate new selected pos
        Vector3Int newSelectedPos = map.WorldPosToCellPos(worldPos);
        bool isThisCellSelectable = CheckForActiveLimitedSelection(newSelectedPos);
        if (!isThisCellSelectable) return;
        bool isThisCellLocked = map.CheckForActiveCellLocks(newSelectedPos);
        if (!isThisCellLocked) return;

        //Clamping the new selected pos;
        newSelectedPos = new Vector3Int(Mathf.Clamp(newSelectedPos.x, 0, width - 1), Mathf.Clamp(newSelectedPos.y, 0, height - 1), 0);
        //Updating view;
        map.DrawSelectOverlay(newSelectedPos);
        //Getting active data
        Building buildingSelected = activeBuildings[Utils.TilePosToIndex(newSelectedPos.x, newSelectedPos.y, width)];
        //New Selection
        selectionHandler.Select(newSelectedPos, buildingSelected);
        ui.SetSelectedTileMenu(true, buildings[buildingSelected.ListIndex]);

        Debug.Log("Selected tile position index = " + buildingSelected.ListIndex);
        //SquishTest
        if (buildingSelected.ListIndex > nonAllowedToBuildMaxIndex)
        {
            map.SquishTile(newSelectedPos);
        }
    }

    

    /// <summary>
    /// Se fija si la sellecion esta limitada a cierta casilla
    /// </summary>
    /// <param name="newSelectedPos"></param>
    /// <returns></returns>
    private bool CheckForActiveLimitedSelection(Vector3Int newSelectedPos)
    {
        bool allowed = true;
        if (limited)
        {
            if (newSelectedPos == limitedToTile)
            {
                allowed = true;
                OnLimitedCleared?.Invoke();
            }
            else
            {
                allowed = false;
                Debug.LogWarning("Limited Selection Active!!");
            }
        }
        Debug.Log("Selection limited = " + limited);
        return allowed;
    }

    /// <summary>
    /// Deselecciona una tile
    /// </summary>
    public void DeselectTile()
    {
        map.ClearSelectOverlay(selectionHandler.Selection.CellPosition);//Clear previous selection
        ui.SetSelectedTileMenu(false, null);//Refresh UI
        selectionHandler.Deselect();//Deselect
    }

    private void OnDestroy()
    {
        InputController.OnTap -= TileTapped;
        Debug.Log("Cleaned Events");
    }
}
