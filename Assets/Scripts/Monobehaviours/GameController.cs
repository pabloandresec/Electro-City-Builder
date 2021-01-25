using UnityEngine;
using System.Collections;
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
    public List<BuildingData> Buildings { get => buildings; set => buildings = value; }
    public List<ComponentData> Components { get => components; set => components = value; }

    void Start()
    {
        //Debug test Button que establece la funcion del boton comprar en una casilla vacia
        ui.buyButton.onClick.AddListener(() =>
        {
            Debug.Log("Buy button pressed");
            Build(selectedTile, 1);
            DeselectTile();
        });
        // End debug test Button

        map = new int[width, height];
        InitDataIndexation(); //Indexa los datos para facilitar su acceso y ahorrar memoria
        PopulatingActiveBuildings(); //Crear los datos de edificios activos en los que se 

        map[7, 6] = buildings[2].Index;
        map[8, 6] = buildings[2].Index;
        map[7, 7] = buildings[2].Index;
        map[8, 7] = buildings[2].Index;

        activeBuildings[TilePosToIndex(7, 6)] = new Building(buildings[2], TilePosToIndex(7, 6));
        activeBuildings[TilePosToIndex(8, 6)] = new Building(buildings[2], TilePosToIndex(8, 6));
        activeBuildings[TilePosToIndex(7, 7)] = new Building(buildings[2], TilePosToIndex(7, 7));
        activeBuildings[TilePosToIndex(8, 7)] = new Building(buildings[2], TilePosToIndex(8, 7));

        activeBuildings[TilePosToIndex(7, 6)].AddBuildingComponent(components[0]);
        activeBuildings[TilePosToIndex(8, 6)].AddBuildingComponent(components[0]);
        activeBuildings[TilePosToIndex(7, 7)].AddBuildingComponent(components[0]);
        activeBuildings[TilePosToIndex(8, 7)].AddBuildingComponent(components[0]);

        RefreshMap();
        RefreshGame();
        ui.UpdateUI(currentGameSessionData.money, CalculateAvailablePower());

        //GetComponent<PGrid>().InitGrid(); PATHFINDING DESHABILITADO
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
                Building b = activeBuildings[TilePosToIndex(selectedTile.x, selectedTile.y)]; // Edificio a modificar
                BuildingData d = buildings[b.ListIndex]; // Data para referencias del edificio
                ComponentLimit currentAount = b.currentCompAmounts.Find(val => val.category == components[index].category);// Trata de obtener la cantidad de esos dispositivos
                int currentDeviceAmount = 0;
                if (currentAount != null)
                {
                    currentDeviceAmount = currentAount.val;
                } 
                int maxDeviceAllowed = -1; // Trata de obtener la cantidad maxima permitida de esos dispositivos
                foreach (ComponentLimit c in d.limits)
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
                        return;
                    }
                    if (currentGameSessionData.money >= components[index].cost)
                    {
                        activeBuildings[TilePosToIndex(selectedTile.x, selectedTile.y)].AddBuildingComponent(components[index]);
                        currentGameSessionData.money -= components[index].cost;
                    }
                    else
                    {
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
                    }
                    else
                    {
                        Debug.Log(" NA MANEY !!");
                    }
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
                }
                else
                {
                    Debug.Log(" NA MANEY !!");
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
        if(state == PlayerState.MENUS)
        {
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

    private int TilePosToIndex(int x,int y)
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
