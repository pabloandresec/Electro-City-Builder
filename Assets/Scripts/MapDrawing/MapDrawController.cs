using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapDrawController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameController gc;
    [Header("Tilemap settings")]
    [SerializeField] private Tile highlightTile;
    [SerializeField] private Tilemap overlayTilemap;
    [SerializeField] private Tilemap buildTilemap;
    [SerializeField] private Tilemap roadTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [Header("Squish animation")]
    [SerializeField] private GameObject buildingAnimationPrefab;
    [SerializeField] private List<GameObject> spawnedBuildingAnims;

    private bool squishing = false;
    private List<Vector3Int> lockedCells;

    /// <summary>
    /// Dibuja solo alrededor de la celda en cuestion
    /// </summary>
    /// <param name="cell"></param>
    public void DrawTile(Vector3Int cell, TileBase tile)
    {
        buildTilemap.SetTile(cell, tile);
        buildTilemap.RefreshTile(cell);
    }

    #region CellLocks
    public void DrawLockedTiles()
    {
        foreach (Vector3Int l in lockedCells)
        {
            buildTilemap.SetTile(l, null);
            buildTilemap.RefreshTile(l);
        }
        Debug.Log("Lock tiles Drawed");
    }

    /// <summary>
    /// Se fija si la casilla suminstrada esta bloqueada
    /// </summary>
    /// <param name="newSelectedPos"></param>
    /// <returns></returns>
    public bool CheckForActiveCellLocks(Vector3Int _cell)
    {
        if (lockedCells == null)
        {
            return true;
        }
        bool allowed = true;
        foreach (Vector3Int cell in lockedCells)
        {
            if (cell == _cell)
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
    /// Will lock the cell so cannot be selected
    /// </summary>
    public void LockCell(Vector3Int cell)
    {
        if (lockedCells == null)
        {
            lockedCells = new List<Vector3Int>();
        }
        lockedCells.Add(cell);
        Debug.Log("Cell " + cell + " locked!");
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
    #endregion

    /// <summary>
    /// Scanea los caminos y los añade como edificios a la lista principal
    /// </summary>
    public void ScanRoads()
    {
        for (int y = 0; y < gc.Height; y++)
        {
            for (int x = 0; x < gc.Width; x++)
            {
                if (roadTilemap.HasTile(new Vector3Int(x, y, 0)))
                {
                    int index = Utils.TilePosToIndex(x, y, gc.Width);
                    gc.ActiveBuildings[index] = new Building(gc.Buildings[3], index);
                }
            }
        }
    }

    /// <summary>
    /// Apaga o prende las luces de los edificios 
    /// </summary>
    /// <param name="state"></param>
    /// <param name="cell"></param>
    /// <param name="data"></param>
    public void SetBuildingElectricVisibility(bool state, Vector3Int cell, BuildingData data)
    {
        TileBase tb = null;
        tb = state ? data.tile : data.tileOff;
        buildTilemap.SetTile(cell, tb);
    }

    /// <summary>
    /// Dibuja el gizmo de celda seleccionada
    /// </summary>
    /// <param name="cell"></param>
    public void DrawSelectOverlay(Vector3Int cell)
    {
        overlayTilemap.SetTile(cell, highlightTile);
        overlayTilemap.RefreshTile(cell);
    }
    /// <summary>
    /// quita el gizmo de celda seleccionada
    /// </summary>
    /// <param name="cell"></param>
    public void ClearSelectOverlay(Vector3Int cell)
    {
        overlayTilemap.SetTile(cell, null);
        overlayTilemap.RefreshTile(cell);
    }

    public Vector3 CellToWorldPosition(Vector3Int cell)
    {
        return groundTilemap.CellToWorld(cell);
    }

    public Vector3Int WorldPosToCellPos(Vector3 worldPos)
    {
        return overlayTilemap.WorldToCell(worldPos);
    }

    public void SquishTile(Vector3Int cell)
    {
        if (cell.x < 0 || cell.x >= gc.Width || cell.y < 0 || cell.y >= gc.Height)
        {
            Debug.Log("Cell outside of bounds");
            return;
        }
        squishing = true;
        Debug.Log("Squishing " + cell);
        GameObject animBuilding = Instantiate(buildingAnimationPrefab, buildTilemap.CellToWorld(cell), Quaternion.identity); // Spawn anim prefab
        animBuilding.transform.name = cell + "_anim";
        SpriteRenderer sr = animBuilding.transform.GetChild(0).GetComponent<SpriteRenderer>();

        Building actBuilding = gc.ActiveBuildings[Utils.TilePosToIndex(cell.x, cell.y, gc.Width)];
        BuildingData buildingData = gc.Buildings[actBuilding.ListIndex];
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
            int index = Utils.TilePosToIndex(cell.x, cell.y, gc.Width);
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
}
