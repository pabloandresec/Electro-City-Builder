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
    [SerializeField] private int minimalAllowedToBuildIndex = 3;
    [SerializeField] private List<BuildingData> buildings;
    [SerializeField] private List<ComponentData> components;
    [Header("Juego Activo")]
    [SerializeField] private Building[] activeBuildings;
    [SerializeField] private PlayerState state = PlayerState.GAME;
    [Header("Animation")]
    [SerializeField] private GameObject buildingAnimationPrefab;
    [SerializeField] private List<GameObject> spawnedBuildingAnims;
    [Header("DEBUG")]
    [SerializeField] private Vector3Int selectedTile = -Vector3Int.one;
    [Header("Limits")]
    private bool limited = false;
    private Vector3Int limitedToTile = -Vector3Int.one;
    private Action OnLimitedCleared;
    private List<Vector3Int> lockedCells;

    private Building selectedBuilding;
    private Building tempSel = null;
    private bool selected = false;
    private float tPassed = 0;
    private uint totalPassed = uint.MinValue;
    private bool squishing = false;

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
                    activeBuildings[Utils.TilePosToIndex(x, y, width)] = new Building(buildings[3], Utils.TilePosToIndex(x, y, width));
                }
            }
        }

        activeBuildings[Utils.TilePosToIndex(8, 7, width)] = new Building(buildings[2], Utils.TilePosToIndex(8, 7, width));
        LockCell(new Vector3Int(9, 7, 0));
        activeBuildings[Utils.TilePosToIndex(7, 5, width)] = new Building(buildings[4], Utils.TilePosToIndex(7, 5, width));

        DrawMap();
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
        //Debug.Log("building electricity in " + cell + " is now " + state);
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
            activeBuildings[i].UpdateComponentsLife(this, timePassed);
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
    /// Will lock the cell so cannot be selected
    /// </summary>
    public void LockCell(Vector3Int cell)
    {
        if(lockedCells == null)
        {
            lockedCells = new List<Vector3Int>();
        }
        lockedCells.Add(cell);
        Debug.Log("Cell "+cell+" locked!");
    }
    /// <summary>
    /// Unlocks a cell is it exists
    /// </summary>
    /// <param name="cell"></param>
    public void UnlockCell(Vector3Int cell)
    {
        if (lockedCells == null)
        {
            Debug.Log("No locked cells");
            return;
        }
        lockedCells.Remove(cell);
        Debug.Log("Cell " + cell + " lock REMOVED!");
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
            return activeBuildings[Utils.TilePosToIndex(selectedTile.x, selectedTile.y, width)];
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
        if (selected)
        {
            if (currentGameSessionData != null)
            {
                ComponentData componentData = components[index]; //Datos del componente a añadir
                Building building = activeBuildings[Utils.TilePosToIndex(selectedTile.x, selectedTile.y, width)]; // Edificio a modificar
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
        Building targetBuilding = activeBuildings[Utils.TilePosToIndex(selectedTile.x, selectedTile.y, width)];

        targetBuilding.AddBuildingComponent(components[index]);
        SetBuildingElectricVisibility(true, new Vector3Int(selectedTile.x, selectedTile.y, 0), buildings[targetBuilding.ListIndex]);
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

        if (index > minimalAllowedToBuildIndex && randomBuilds)
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
                    Build(selectedTile, indexSelected);
                    builded = true;
                    currentGameSessionData.money -= buildings[indexSelected].buildingCost;
                    ui.UpdateUI(currentGameSessionData.money);
                    DeselectTile();
                    GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(8);
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
        BuildingData selectedBuildingData = buildings[buildingIndex];
        int buildGridIndex = Utils.TilePosToIndex(tilePos.x, tilePos.y, width);
        activeBuildings[buildGridIndex] = new Building(selectedBuildingData, buildGridIndex);
        DrawTile(tilePos);
        Debug.Log("Building constructed");
    }

    public void SquishTile(Vector3Int cell)
    {
        if(cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
        {
            Debug.Log("Cell outside of bounds");
            return;
        }
        squishing = true;
        Debug.Log("Squishing " + cell);
        GameObject animBuilding = Instantiate(buildingAnimationPrefab, buildTilemap.CellToWorld(cell), Quaternion.identity); // Spawn anim prefab
        animBuilding.transform.name = cell + "_anim";
        SpriteRenderer sr = animBuilding.transform.GetChild(0).GetComponent<SpriteRenderer>();

        Building actBuilding = activeBuildings[Utils.TilePosToIndex(cell.x, cell.y, width)];
        BuildingData buildingData = buildings[actBuilding.ListIndex];
        Sprite s = buildTilemap.GetSprite(cell);

        sr.sprite = s;

        buildTilemap.SetTile(cell, null);
        buildTilemap.RefreshTile(cell);
        DrawLockedTiles();

        if (spawnedBuildingAnims == null)
        {
            spawnedBuildingAnims = new List<GameObject>();
            spawnedBuildingAnims.Add(animBuilding);
            return;
        }
        if (spawnedBuildingAnims.Contains(animBuilding))
        {
            Debug.Log(animBuilding.transform.name + " already exist");
            return;
        }

        spawnedBuildingAnims.Add(animBuilding);
        Debug.Log("Spawned buiding for anim " + animBuilding);

        Action onEnd = () =>
        {
            spawnedBuildingAnims.Remove(animBuilding);
            Debug.Log("Despawned " + animBuilding);
            int index = Utils.TilePosToIndex(cell.x, cell.y, width);
            TileBase tile = actBuilding.components.Count == 0 ? buildingData.tileOff : buildingData.tile;
            buildTilemap.SetTile(cell, tile);
            buildTilemap.RefreshTile(cell);
            DrawLockedTiles();
            Destroy(animBuilding);
            squishing = false;
        };
        LeanTween.scale(animBuilding, new Vector3(0.9f, 0.9f, 1f), 0.2f).setEaseInOutBounce().setOnComplete(() =>
        {
            LeanTween.scale(animBuilding, new Vector3(1f, 1f, 1f), 0.2f).setEaseInOutBounce().setOnComplete(() =>
            {
                onEnd?.Invoke();
            });
        });
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
    /// Dibuja solo alrededor de la celda en cuestion
    /// </summary>
    /// <param name="cell"></param>
    private void DrawTile(Vector3Int cell)
    {
        Building b = activeBuildings[Utils.TilePosToIndex(cell.x, cell.y, width)];
        BuildingData bd = buildings[b.ListIndex];
        if (bd.Index != minimalAllowedToBuildIndex)
        {
            if (b.components == null || b.components.Count == 0)
            {
                buildTilemap.SetTile(cell, bd.tileOff);
            }
            else
            {
                buildTilemap.SetTile(cell, bd.tile);
            }
        }
        buildTilemap.RefreshTile(cell);
        DrawLockedTiles();
    }

    /// <summary>
    /// De acuerdo a los edificios dibuja el mapa correspondiente
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
                if (bd.Index != 3)
                {
                    if(b.components == null || b.components.Count == 0)
                    {
                        buildTilemap.SetTile(cell, bd.tileOff);
                    }
                    else
                    {
                        buildTilemap.SetTile(cell, bd.tile);
                    }
                }
                buildTilemap.RefreshTile(cell);
            }
        }
        DrawLockedTiles();
        Debug.Log("Map Drawed");
    }

    public void DrawLockedTiles()
    {
        foreach (Vector3Int l in lockedCells)
        {
            buildTilemap.SetTile(l, null);
            buildTilemap.RefreshTile(l);
        }
        Debug.Log("Lock tiles Drawed");
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
    /// Cuando el jugador presiona en una celda
    /// </summary>
    /// <param name="worldPos"></param>
    public void TileTapped(Vector3 worldPos)
    {
        if (UIController.tweening)
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
                int selBuildingIndex = activeBuildings[Utils.TilePosToIndex(selectedTile.x, selectedTile.y, width)].ListIndex;
                Debug.Log("Sending info of " + selectedTile + " index " + selBuildingIndex);
                ui.SetSelectedTileMenu(true, buildings[selBuildingIndex]);
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
        bool isThisCellSelectable = CheckForActiveLimitedSelection(newSelectedPos);
        if (!isThisCellSelectable) return;
        bool isThisCellLocked = CheckForActiveCellLocks(newSelectedPos);
        if (!isThisCellLocked) return;

        //Clamping the new selected pos;
        newSelectedPos = new Vector3Int(Mathf.Clamp(newSelectedPos.x, 0, width - 1), Mathf.Clamp(newSelectedPos.y, 0, height - 1), 0);
        //Updating view;
        overlayTilemap.SetTile(newSelectedPos, highlightTile);
        overlayTilemap.RefreshTile(newSelectedPos);
        //Set selected tile;
        selectedTile = newSelectedPos;
        tempSel = activeBuildings[Utils.TilePosToIndex(selectedTile.x, selectedTile.y, width)];
        Debug.Log("Selected tile position index = " + tempSel.ListIndex);
        //SquishTest
        if (tempSel.ListIndex > minimalAllowedToBuildIndex)
        {
            SquishTile(newSelectedPos);
        }
        selected = true;
    }

    /// <summary>
    /// Se fija si la casilla suminstrada esta bloqueada
    /// </summary>
    /// <param name="newSelectedPos"></param>
    /// <returns></returns>
    private bool CheckForActiveCellLocks(Vector3Int _cell)
    {
        if(lockedCells == null)
        {
            return true;
        }
        bool allowed = true;
        foreach(Vector3Int cell in lockedCells)
        {
            if(cell == _cell)
            {
                Debug.Log("cell " + cell + " is locked");
                allowed = false;
                break;
            }
        }
        Debug.Log("Cell " + _cell + " lock is " + allowed);
        return allowed;
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

    /*
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
    */
    public Vector3 SelectedToWorldPosition()
    {
        return groundTilemap.CellToWorld(selectedTile);
    }

    public Vector3 CellToWorldPosition(Vector3Int cell)
    {
        return groundTilemap.CellToWorld(cell);
    }

}
