using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;


//Esta clase 
public class PGrid : MonoBehaviour {

	[SerializeField] private bool displayGridGizmos;
    [SerializeField] private GameController gameController;

    [SerializeField] private Tilemap roadTilemap;
    [SerializeField] private int roadIndex;
    [SerializeField] private Vector2 gridWorldSize;
    [SerializeField] private float nodeRadius;

    [SerializeField] private Vector3 gridOffset;
    [SerializeField] private GameObject trafficPrefab;
    [Min(0)]
    [SerializeField] private int trafficAmount;

    private List<Vector2Int> roads;
    private Node[,] grid;
    private bool gridInit = false;

	float nodeDiameter;

	void Awake()
    {
        roads = new List<Vector2Int>();
        CheckReferences();
    }

    private void CheckReferences()
    {
        if (roadTilemap == null)
        {
            roadTilemap = GetComponent<Tilemap>();
        }
        if (gameController == null)
        {
            gameController = GetComponent<GameController>();
        }
    }

    public void InitGrid()
    {
        Debug.Log("start pGrid");
        grid = new Node[gameController.Width, gameController.Height];
        ScanRoadTiles();
        for (int i = 0; i < trafficAmount; i++)
        {
            Vector3 r = RequestRandomRoadWorldPos();
            GameObject g = Instantiate(trafficPrefab, new Vector3(r.x, r.y, 0), Quaternion.identity) as GameObject;
            g.name = "Traffico " + i.ToString();
        }
    }



    private void ScanRoadTiles()
    {
        for (int x = 0; x < gameController.Width; x++)
        {
            for (int y = 0; y < gameController.Height; y++)
            {
                Vector2 worldPoint = roadTilemap.CellToWorld(new Vector3Int(x, y, 0)) + gridOffset;
                bool walkable = gameController.ActiveBuildings[gameController.TilePosToIndex(x, y)].ListIndex == 3;
                if (walkable) roads.Add(new Vector2Int(x, y));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
        gridInit = true;
        Debug.Log("pgrid Init!!");
    }

    public int MaxSize
    {
		get { return gameController.Width * gameController.Height; }
	}

	public List<Node> GetNeighbours(Node node, int depth = 1) {
		List<Node> neighbours = new List<Node>();
        foreach (Vector2Int dir in Utils.cardinalDirections)
        {
            Vector2Int checkTile = new Vector2Int(node.gridX, node.gridY) + dir;
            if(InBounds(checkTile.x,checkTile.y))
            {
                neighbours.Add(grid[checkTile.x, checkTile.y]);
            }
        }
        return neighbours;
        #region OldFunction
        /*
        int checkX, checkY;
        //Check Top
        checkX = node.gridX;
        checkY = node.gridY + 1;
        
        if (InBounds(checkX,checkY))
        {
            neighbours.Add(grid[checkX, checkY]);
        }
        //Check Bottom
        checkX = node.gridX;
        checkY = node.gridY - 1;

        if (InBounds(checkX, checkY))
        {
            neighbours.Add(grid[checkX, checkY]);
        }
        //Check left
        checkX = node.gridX - 1;
        checkY = node.gridY;

        if (InBounds(checkX, checkY))
        {
            neighbours.Add(grid[checkX, checkY]);
        }
        //Check right
        checkX = node.gridX + 1;
        checkY = node.gridY;

        if (InBounds(checkX, checkY))
        {
            neighbours.Add(grid[checkX, checkY]);
        }

        return neighbours;
        */ 
        #endregion
    }
	
    public Node NodeFromWorldPoint(Vector3 _worldPosition)
    {
        Vector3Int pos = roadTilemap.WorldToCell(_worldPosition);
        pos.x = Mathf.Clamp(pos.x, 0, gameController.Width);
        pos.y = Mathf.Clamp(pos.y, 0, gameController.Height);
        return grid[pos.x, pos.y];
    }


    public Node ClosestWalkableNode(Node node) {
		int maxRadius = Mathf.Max (gameController.Width, gameController.Height) / 2;
		for (int i = 1; i < maxRadius; i++) {
			Node n = FindWalkableInRadius (node.gridX, node.gridY, i);
			if (n != null) {
				return n;
			}
		}
		return null;
	}
	Node FindWalkableInRadius(int centreX, int centreY, int radius) {

		for (int i = -radius; i <= radius; i ++) {
			int verticalSearchX = i + centreX;
			int horizontalSearchY = i + centreY;

			// top
			if (InBounds(verticalSearchX, centreY + radius)) {
				if (grid[verticalSearchX, centreY + radius].walkable) {
					return grid [verticalSearchX, centreY + radius];
				}
			}

			// bottom
			if (InBounds(verticalSearchX, centreY - radius)) {
				if (grid[verticalSearchX, centreY - radius].walkable) {
					return grid [verticalSearchX, centreY - radius];
				}
			}
			// right
			if (InBounds(centreY + radius, horizontalSearchY)) {
				if (grid[centreX + radius, horizontalSearchY].walkable) {
					return grid [centreX + radius, horizontalSearchY];
				}
			}

			// left
			if (InBounds(centreY - radius, horizontalSearchY)) {
				if (grid[centreX - radius, horizontalSearchY].walkable) {
					return grid [centreX - radius, horizontalSearchY];
				}
			}

		}

		return null;

	}

	private bool InBounds(int x, int y)
    {
		return x >= 0 && x < gameController.Width && y >= 0 && y< gameController.Height;
	}

    private void OnDrawGizmos() {
		if (gridInit && displayGridGizmos)
        {
            for (int x = 0; x < gameController.Width; x++)
            {
                for (int y = 0; y < gameController.Height; y++)
                {
                    Gizmos.color = Color.red;
                    if(grid[x, y].walkable == true)
                    {
                        Gizmos.color = Color.white;
                    }
                    Gizmos.DrawWireCube(grid[x, y].worldPosition,new Vector3(0.25f, 0.25f, 0.25f));
                }
            }
		}
	}

    public Vector3 RequestRandomRoadWorldPos()
    {
        return roadTilemap.CellToWorld((Vector3Int)roads[Random.Range(0, roads.Count)]);
    }
    public Vector2Int RequestRandomRoadCellPos()
    {
        return roads[Random.Range(0, roads.Count)];
    }
}