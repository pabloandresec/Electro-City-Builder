using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

public class GameController : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int width = 16;
    [SerializeField] private int height = 16;
    [Header("Game Settings")]
    [Range(0.2f,10)]
    [SerializeField] private float refreshRate;
    [SerializeField] private GameSession currentGameSessionData;
    [Header("References")]
    [SerializeField] private Tile highlightTile;
    [SerializeField] private Tilemap overlayTilemap;
    [SerializeField] private Tilemap buildTilemap;
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

    private Building selectedBuilding;
    private bool selected = false;
    private int[,] map; //For pathfinding & map draw
    private float tPassed = 0;

    public int Width { get => width; }
    public int Height { get => height; }
    public int[,] Map { get => map;}
    public List<BuildingData> Buildings { get => buildings; }
    public List<ComponentData> Components { get => components; }
    public Building[] ActiveBuildings { get => activeBuildings; }
    public List<Vector2Int> roads;
    public GameObject trafficPrefab;

    void Start()
    {
        map = new int[width, height];
        roads = new List<Vector2Int>();
        InitDataIndexation(); //Indexa los datos para facilitar su acceso y ahorrar memoria
        PopulatingActiveBuildings(); //Crear los datos de edificios activos en los que se 

        map[8, 8] = buildings[2].Index; //Central
        for (int x = 0; x < 16; x++)//Roads down the middle
        {
            roads.Add(new Vector2Int(x,7));
            map[x, 7] = buildings[3].Index;
            activeBuildings[TilePosToIndex(x, 7)] = new Building(buildings[3], TilePosToIndex(x, 7));
        }
        map[7, 6] = buildings[4].Index; // starter building
        map[8, 6] = buildings[4].Index; // another starter building

        activeBuildings[TilePosToIndex(8, 8)] = new Building(buildings[2], TilePosToIndex(8, 8));
        activeBuildings[TilePosToIndex(7, 6)] = new Building(buildings[4], TilePosToIndex(7, 6));
        activeBuildings[TilePosToIndex(8, 6)] = new Building(buildings[4], TilePosToIndex(8, 6));

        activeBuildings[TilePosToIndex(7, 6)].AddBuildingComponent(components[0]);
        activeBuildings[TilePosToIndex(8, 6)].AddBuildingComponent(components[0]);

        for(int i = 0;i < 10; i++)
        {
            Vector3 r = RequestRandomRoadWorldPos();
            GameObject g = Instantiate(trafficPrefab, new Vector3(r.x,r.y,0), Quaternion.identity) as GameObject;
            g.name = "Traffico "+i.ToString();
        }

        RefreshMap();
        RefreshGame();
        ui.UpdateUI(currentGameSessionData.money, CalculateAvailablePower());

        GetComponent<PGrid>().InitGrid(); //Comentar para PATHFINDING DESHABILITADO
        InputController.OnTap += TileTapped;
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

    public Vector3 RequestRandomRoadWorldPos()
    {
        return buildTilemap.CellToWorld((Vector3Int)roads[Random.Range(0,roads.Count)]);
    }
    public Vector2Int RequestRandomRoadCellPos()
    {
        return roads[Random.Range(0,roads.Count)];
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
                        activeBuildings[TilePosToIndex(selectedTile.x, selectedTile.y)].AddBuildingComponent(components[index]);
                        currentGameSessionData.money -= components[index].cost;
                        GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(5);
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
                        activeBuildings[TilePosToIndex(selectedTile.x, selectedTile.y)].AddBuildingComponent(components[index]);
                        currentGameSessionData.money -= components[index].cost;
                        GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(5);
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

    public void TryBuild(int index)
    {
        if(selected)
        {
            if(currentGameSessionData != null)
            {
                if(currentGameSessionData.money >= buildings[index].buildingCost)
                {
                    Build(selectedTile, index);
                    currentGameSessionData.money -= buildings[index].buildingCost;
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
    }

    public void Build(Vector3Int tilePos, int buildingIndex)
    {
        map[tilePos.x, tilePos.y] = buildingIndex;
        activeBuildings[TilePosToIndex(tilePos.x, tilePos.y)] = new Building(buildings[buildingIndex], TilePosToIndex(tilePos.x, tilePos.y));
        ui.UpdateUI(currentGameSessionData.money, CalculateAvailablePower());
        RefreshMap();
        GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(2);
        Debug.Log("Building constructed");
    }

    private int CalculateAvailablePower()
    {
        int total = 0;
        foreach(Building b in activeBuildings)
        {
            total += b.totalPowerConsumption;
        }
        return total;
    }

    private void RefreshMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                buildTilemap.SetTile(new Vector3Int(x, y, 0), buildings[map[x, y]].tile);
            }
        }
    }

    public void TileTapped(Vector3 worldPos)
    {
        if(state == PlayerState.MENUS || UIController.tweening)
        {
            Debug.Log("Using Menus!!");
            return;
        }
        if(selected)
        {
            DeselectTile();
        }
        else
        {
            SelectTile(worldPos);
            Debug.Log("Sending info of " + selectedTile + " index " +TilePosToIndex(selectedTile.x, selectedTile.y));
            int selectedBuilding = activeBuildings[TilePosToIndex(selectedTile.x, selectedTile.y)].ListIndex;
            ui.SetSelectedTileMenu(true, buildings[selectedBuilding]);
        }
    }

    public void SwitchState(int newState)
    {
        PlayerState s = (PlayerState)newState;
        SwitchState(s);
    }

    public void SwitchState(PlayerState newState)
    {
        //Logica
        state = newState;
    }

    public void DeselectTile()
    {
        //Clear previous selection
        overlayTilemap.SetTile(selectedTile, null);
        overlayTilemap.RefreshTile(selectedTile);
        selected = false;
        ui.SetSelectedTileMenu(false, null);
    }

    private void Update()
    {
        RefreshGame();
        if(Input.GetKeyDown(KeyCode.T))
        {
            TileTapped(new Vector3(0.999f, 3.209f, 0));
        }
    }

    private void RefreshGame()
    {
        tPassed += Time.deltaTime;
        if(tPassed >= refreshRate)
        {
            tPassed = 0;
            UpdateBuildings();
        }
    }

    private void UpdateBuildings()
    {
        Debug.Log("Refreshing buildings");
        int totalPow = 0;
        for (int i = 0; i < activeBuildings.Length; i++)
        {
            activeBuildings[i].UpdateComponentsLife(components);
            totalPow += activeBuildings[i].totalPowerConsumption;
        }
        ui.UpdateUI(currentGameSessionData.money, totalPow);
    }

    private void SelectTile(Vector3 worldPos)
    {
        
        //Calculate new selected pos
        Vector3Int newSelectedPos = overlayTilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
        //Clamping the new selected pos;
        newSelectedPos = new Vector3Int(Mathf.Clamp(newSelectedPos.x, 0, width-1), Mathf.Clamp(newSelectedPos.y, 0, height-1), 0);
        //Updating view;
        overlayTilemap.SetTile(newSelectedPos, highlightTile);
        overlayTilemap.RefreshTile(newSelectedPos);
        //Set selected tile;
        selectedTile = newSelectedPos;
        testIndex = TilePosToIndex(selectedTile.x, selectedTile.y);
        Debug.Log("Buildings index = " + testIndex);
        selected = true;
    }

    private void OnDestroy()
    {
        InputController.OnTap -= TileTapped;
        Debug.Log("Cleaned Events");
    }

    private int TilePosToIndex(int x, int y)
    {
        int index = x + y * width;
        return index;
    }
    private Vector3Int IndexToTilePos(int indx)
    {
        int y = (int)(indx / width);
        int x = indx - (y * width);
        return new Vector3Int(x, y, 0);
    }

    public void TestIndex()
    {
        Debug.Log("Index " + testIndex + " is " + IndexToTilePos(testIndex));
    }
}
