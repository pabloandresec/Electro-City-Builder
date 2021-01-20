using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [Header("References")]
    [SerializeField] private Tile highlightTile;
    [SerializeField] private Tilemap overlayTilemap;
    [SerializeField] private Tilemap buildTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private List<BuildingData> buildings;
    [Header("Edificios Activos")]
    [SerializeField] private Building[] activeBuildings;

    [Header("DEBUG")]
    [SerializeField] private int testIndex = 17;
    [SerializeField] private Vector3Int selectedTile = Vector3Int.zero;

    private int[,] map;

    public int Width { get => width; }
    public int Height { get => height; }
    public int[,] Map { get => map;}

    void Start()
    {
        activeBuildings = new Building[width * height];
        map = new int[width, height];
        Debug.Log("Map Init Empty");
        //GetComponent<PGrid>().InitGrid(); PATHFINDING DESHABILITADO
        InputController.OnTap += SelectTile;
    }

    private void SelectTile(Vector3 worldPos)
    {
        //Clear previous selection
        overlayTilemap.SetTile(selectedTile, null);
        overlayTilemap.RefreshTile(selectedTile);
        //Calculate new selected pos
        Vector3Int newSelectedPos = overlayTilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
        //Clamping the new selected pos;
        newSelectedPos = new Vector3Int(Mathf.Clamp(newSelectedPos.x, 0, width-1), Mathf.Clamp(newSelectedPos.y, 0, height-1), 0);
        //Updating view;
        overlayTilemap.SetTile(newSelectedPos, highlightTile);
        overlayTilemap.RefreshTile(newSelectedPos);
        //Set selected tile;
        selectedTile = newSelectedPos;
        Debug.Log("Buildings index = " + TilePosToIndex(selectedTile.x, selectedTile.y));
    }

    private void OnDestroy()
    {
        InputController.OnTap -= SelectTile;
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
