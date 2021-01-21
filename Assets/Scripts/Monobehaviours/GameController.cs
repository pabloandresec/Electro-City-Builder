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
    [SerializeField] private GameSession currentGame;
    [Header("References")]
    [SerializeField] private Tile highlightTile;
    [SerializeField] private Tilemap overlayTilemap;
    [SerializeField] private Tilemap buildTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private UIController ui;
    [SerializeField] private List<BuildingData> buildings;
    [SerializeField] private List<ComponentData> components;
    [Header("Edificios Activos")]
    [SerializeField] private Building[] activeBuildings;

    [Header("DEBUG")]
    [SerializeField] private int testIndex = 17;
    [SerializeField] private Vector3Int selectedTile = Vector3Int.zero;

    private bool selected = false;
    private int[] map;
    private float tPassed = 0;

    public int Width { get => width; }
    public int Height { get => height; }
    public int[] Map { get => map;}
    public List<BuildingData> Buildings { get => buildings; set => buildings = value; }
    public List<ComponentData> Components { get => components; set => components = value; }

    void Start()
    {
        map = new int[width * height];
        for (int i = 0; i < buildings.Count; i++)
        {
            buildings[i].SetIndex(i);
        }
        for (int i = 0; i < components.Count; i++)
        {
            components[i].SetIndex(i);
        }
        activeBuildings = new Building[width * height];

        map[TilePosToIndex(7, 6)] = buildings[2].Index;
        map[TilePosToIndex(8, 6)] = buildings[2].Index;
        map[TilePosToIndex(7, 7)] = buildings[2].Index;
        map[TilePosToIndex(8, 7)] = buildings[2].Index;

        activeBuildings[TilePosToIndex(7, 6)] = new Building(buildings[2], TilePosToIndex(7, 6));
        activeBuildings[TilePosToIndex(8, 6)] = new Building(buildings[2], TilePosToIndex(8, 6));
        activeBuildings[TilePosToIndex(7, 7)] = new Building(buildings[2], TilePosToIndex(7, 7));
        activeBuildings[TilePosToIndex(8, 7)] = new Building(buildings[2], TilePosToIndex(8, 7));

        activeBuildings[TilePosToIndex(7, 6)].AddBuildingComponent(components[0], 20);
        activeBuildings[TilePosToIndex(8, 6)].AddBuildingComponent(components[0], 20);
        activeBuildings[TilePosToIndex(7, 7)].AddBuildingComponent(components[0], 20);
        activeBuildings[TilePosToIndex(8, 7)].AddBuildingComponent(components[0], 20);

        RefreshMap();

        //GetComponent<PGrid>().InitGrid(); PATHFINDING DESHABILITADO
        InputController.OnTap += TileTapped;
    }

    private void RefreshMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                buildTilemap.SetTile(new Vector3Int(x, y, 0), buildings[map[TilePosToIndex(x, y)]].tile);
            }
        }
    }

    public void TileTapped(Vector3 worldPos)
    {
        if(selected)
        {
            DeselectTile();
            ui.SetSelectedTileMenu(false, null);
        }
        else
        {
            SelectTile(worldPos);
            int selectedBuilding = activeBuildings[TilePosToIndex(selectedTile.x, selectedTile.y)].ListIndex;
            ui.SetSelectedTileMenu(true, buildings[selectedBuilding]);
        }
    }

    private void DeselectTile()
    {
        //Clear previous selection
        overlayTilemap.SetTile(selectedTile, null);
        overlayTilemap.RefreshTile(selectedTile);
        selected = false;
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
        foreach(Building b in activeBuildings)
        {
            if(b != null)
            {
                Debug.Log("Updating " + buildings[b.ListIndex].name + " at " + IndexToTilePos(b.PositionIndex));
                b.UpdateComponentsLife(components);
            }
        }
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
