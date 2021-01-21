using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class PGrid : MonoBehaviour {

    public bool diagonals = true;
	public bool displayGridGizmos;
    public GameController gameController;

    public int roadIndex;
    public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;

    public Grid tileGrid;
    public Vector3 gridOffset;
    Node[,] grid;
    bool gridInit = false;

	float nodeDiameter;

	void Awake()
    {
        if(tileGrid == null)
        {
            tileGrid = GetComponent<Grid>();
        }
        if(gameController == null)
        {
            gameController = GetComponent<GameController>();
        }
	}

    public void InitGrid()
    {
        Debug.Log("start pGrid");
        grid = new Node[gameController.Width, gameController.Height];
        ScanRoadTiles();
    }

    private void ScanRoadTiles()
    {
        for (int x = 0; x < gameController.Width; x++)
        {
            for (int y = 0; y < gameController.Height; y++)
            {
                Vector2 worldPoint = tileGrid.CellToWorld(new Vector3Int(x, y, 0)) + gridOffset;
                bool walkable = false;//gameController.Map[x, y] == roadIndex ? true : false;
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

        if(diagonals)
        {
            for (int x = -depth; x <= depth; x++)
            {
                for (int y = -depth; y <= depth; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if (checkX >= 0 && checkX < gameController.Width && checkY >= 0 && checkY < gameController.Height)
                    {
                        neighbours.Add(grid[checkX, checkY]);
                    }
                }
            }
        }
        else
        {
            int checkX, checkY;
            //Check Top
            checkX = node.gridX;
            checkY = node.gridY + 1;

            if (checkX >= 0 && checkX < gameController.Width && checkY >= 0 && checkY < gameController.Height)
            {
                neighbours.Add(grid[checkX, checkY]);
            }
            //Check Bottom
            checkX = node.gridX;
            checkY = node.gridY - 1;

            if (checkX >= 0 && checkX < gameController.Width && checkY >= 0 && checkY < gameController.Height)
            {
                neighbours.Add(grid[checkX, checkY]);
            }
            //Check left
            checkX = node.gridX - 1;
            checkY = node.gridY;

            if (checkX >= 0 && checkX < gameController.Width && checkY >= 0 && checkY < gameController.Height)
            {
                neighbours.Add(grid[checkX, checkY]);
            }
            //Check right
            checkX = node.gridX + 1;
            checkY = node.gridY;

            if (checkX >= 0 && checkX < gameController.Width && checkY >= 0 && checkY < gameController.Height)
            {
                neighbours.Add(grid[checkX, checkY]);
            }
        }
		

		return neighbours;
	}
	
    public Node NodeFromWorldPoint(Vector3 _worldPosition)
    {
        Vector3Int pos = tileGrid.WorldToCell(_worldPosition);
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

}