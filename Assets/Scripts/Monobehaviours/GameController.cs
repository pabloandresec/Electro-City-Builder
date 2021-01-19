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
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = buildTilemap.HasTile(new Vector3Int(x,y,0)) ? 1 : 0;
            }
        }
        /*
        map[0, 0] = 1;
        map[1, 0] = 1;
        map[2, 0] = 1;
        map[3, 0] = 1;
        map[2, 1] = 1;
        map[2, 2] = 1;
        map[3, 2] = 1;
        map[3, 3] = 1;
        map[3, 4] = 1;
        map[2, 4] = 1;
        map[1, 4] = 1;
        map[0, 4] = 1;
        map[0, 3] = 1;
        */
        Debug.Log("map init");

        GetComponent<PGrid>().InitGrid();
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
