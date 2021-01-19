using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Tile highlightTile;
    [SerializeField] private Tilemap overlayTilemap;
    [SerializeField] private Tilemap buildTilemap;
    [SerializeField] private Tilemap groundTilemap;

    private Vector3Int selectedTile = Vector3Int.zero;

    [SerializeField] private int[,] map;

    public int Width { get => width; }
    public int Height { get => height; }
    public int[,] Map { get => map;}

    void Start()
    {
        map = new int[width, height];
        Debug.Log("Map Init Empty");
        //GetComponent<PGrid>().InitGrid(); PATHFINDING DESHABILITADO
        InputController.OnTap += SelectTile;
    }

    private void SelectTile(Vector3 worldPos)
    {
        //Debug.Log("Selecting tile");
        Vector3Int newSelectedPos = overlayTilemap.WorldToCell(new Vector3(worldPos.x,worldPos.y,0));
        overlayTilemap.SetTile(selectedTile, null);
        overlayTilemap.RefreshTile(selectedTile);

        overlayTilemap.SetTile(newSelectedPos, highlightTile);
        overlayTilemap.RefreshTile(newSelectedPos);
        selectedTile = newSelectedPos;
    }

    private void OnDestroy()
    {
        InputController.OnTap -= SelectTile;
        Debug.Log("Cleaned Events");
    }
}
