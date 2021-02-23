using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class Utils
{
    public static Vector2Int[] cardinalDirections =
    {
            new Vector2Int(0,1),  //North
            new Vector2Int(1,0),  //East
            new Vector2Int(0,-1), //South
            new Vector2Int(-1,0), //West
        };

    public static Color GenerateRandomColor()
    {
        Color randColor = new Color
            (
              Random.Range(0f, 1f),
              Random.Range(0f, 1f),
              Random.Range(0f, 1f),
              1
            );
        return randColor;
    }

    public static void LoadLevelAsync(int sceneIndex)
    {
        SceneManager.LoadSceneAsync(sceneIndex);
    }

    public static string GenerateRandomString(int lenght)
    {
        string abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        string temp = string.Empty;
        for (int i = 0; i < lenght; i++)
        {
            temp += abc[Random.Range(0, abc.Length)];
        }
        return temp;
    }

    public static Vector2Int GetRandomMapPos(int width, int height)
    {
        return new Vector2Int(Random.Range(0, width), Random.Range(0, height));
    }

    public static bool IsInMapBounds(int[,] map, Vector2Int pos)
    {
        return pos.x < map.GetLength(0) && pos.x >= 0 && pos.y < map.GetLength(1) && pos.y >= 0;
    }

    public static Vector2Int GetRandomMapPosExcludingBounds(Bounds bounds, Vector2Int offset, int w, int h)
    {
        RectInt[] areas = new RectInt[]
        {
                new RectInt(new Vector2Int(((int)bounds.min.x - offset.x), (int)bounds.min.y), new Vector2Int(offset.x, (int)(bounds.max.y - bounds.min.y))),
                new RectInt(new Vector2Int((int)bounds.min.x - offset.x, (int)bounds.min.y - offset.y), new Vector2Int((int)bounds.size.x + (offset.x * 2), offset.y)),
                new RectInt(new Vector2Int((int)bounds.max.x, (int)bounds.min.y), new Vector2Int(offset.x, (int)(bounds.max.y - bounds.min.y))),
                new RectInt(new Vector2Int((int)bounds.min.x - offset.x, (int)bounds.max.y), new Vector2Int((int)bounds.size.x + (offset.x * 2),offset.y))
        };

        int areaIndx = Random.Range(0, 4);
        int randX = Random.Range(areas[areaIndx].position.x, areas[areaIndx].max.x);
        int randY = Random.Range(areas[areaIndx].position.y, areas[areaIndx].max.y);
        return new Vector2Int(randX, randY);
    }

    public static Vector2 GetRandomBoundedPos(Bounds bounds)
    {
        int randX = Random.Range((int)bounds.min.x, (int)bounds.max.x);
        int randY = Random.Range((int)bounds.min.y, (int)bounds.max.y);
        return new Vector2(randX, randY);
    }

    public static int TilePosToIndex(int x, int y, int width)
    {
        int index = x + y * width;
        return index;
    }
    public static Vector3Int IndexToTilePos(int indx, int width)
    {
        int y = (int)(indx / width);
        int x = indx - (y * width);
        return new Vector3Int(x, y, 0);
    }
}
[Serializable]
public struct IntRange
{
    public int min;
    public int max;

    public IntRange(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
}
[Serializable]
public struct FloatRange
{
    public float min;
    public float max;

    public FloatRange(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}

public enum Orientation
{
    HORIZONTAL,
    VERTICAL
}

public enum PlayerState
{
    GAME = 0,
    MENUS = 1
}



