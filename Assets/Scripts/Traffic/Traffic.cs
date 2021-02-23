using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class Traffic : MonoBehaviour
{
    [Header("AI")]
    [SerializeField] protected Vector2Int dest;
    [SerializeField] protected float speed = 0;
    [SerializeField] private float offset;

    private Vector2[] path;
    private Vector3[] correctPath;
    private bool hasPath = false;
    private bool calculatingPath = false;
    private int currentIndex = -1;

    protected void RequestPath()
    {
        dest = GameObject.FindGameObjectWithTag("GameController").GetComponent<PGrid>().RequestRandomRoadCellPos();
        path = Pathfinding.RequestPathToTile(transform.position, dest);
        OffsetPath();
        calculatingPath = false;
        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if(correctPath == null || currentIndex >= correctPath.Length)
        {
            ClearPath();
        }
        else
        {
            int nextIndx = currentIndex + 1;
            
            if(nextIndx >= correctPath.Length)
            {
                ClearPath();
            }
            else
            {
                RefreshSprites(correctPath[nextIndx] - transform.position);
                LeanTween.move(gameObject, correctPath[nextIndx], 1 / speed).setOnComplete(() =>
                {
                    currentIndex = nextIndx;
                    MoveAlongPath();
                });
            }
        }
    }

    protected virtual void RefreshSprites(Vector2 direction) {}

    private void ClearPath()
    {
        //Debug.Log("Path complete");
        currentIndex = -1;
        calculatingPath = false;
        hasPath = false;
        path = null;
        correctPath = null;
        RequestPath();
    }

    private void OffsetPath()
    {
        if(path == null || path.Length == 0)
        {
            return;
        }
        correctPath = new Vector3[path.Length];
        Vector2[] directions = new Vector2[path.Length];
        correctPath[0] = path[0];
        Vector2[] ortDirections = new Vector2[]
        {
            new Vector2(1,1),//NE
            new Vector2(1,-1),//SE
            new Vector2(-1,-1),//SW
            new Vector2(-1,1) //NW
        };
        for (int i = 1; i < path.Length; i++)
        {
            Vector2 dir = path[i] - path[i - 1];
            dir.x = dir.x > 0 ? Mathf.CeilToInt(dir.x) : Mathf.FloorToInt(dir.x);
            dir.y = dir.y > 0 ? Mathf.CeilToInt(dir.y) : Mathf.FloorToInt(dir.y);
            directions[i-1] = new Vector2(dir.x, dir.y);
            
            for (int indx = 0; indx < ortDirections.Length; indx++)
            {
                if(dir == ortDirections[indx])
                {
                    Vector2 newPos = (Vector2)Vector3.Cross(dir, Vector3.forward) * offset + path[i];
                    correctPath[i] = newPos;
                }
            }
        }

        for (int i = 1; i < correctPath.Length; i++)
        {
            if(directions[i-1] != directions[i])
            {
                if(directions[i] == ortDirections[0]) //NE
                {
                    correctPath[i] += new Vector3(offset, -offset);
                }
                else if (directions[i] == ortDirections[1]) //SE
                {
                    correctPath[i] += new Vector3(-offset, -offset);
                }
                else if (directions[i] == ortDirections[2]) //SW
                {
                    correctPath[i] += new Vector3(-offset, offset);
                }
                else if (directions[i] == ortDirections[3]) //NW
                {
                    correctPath[i] += new Vector3(offset, offset);
                }
            }
        }
        correctPath[0] = (Vector2)Vector3.Cross(directions[0], Vector3.forward) * offset + path[0];
        hasPath = true;
    }

    private void OnDrawGizmosSelected()
    {
        if(path != null && path.Length > 0)
        {
            for (int i = 1; i < path.Length; i++)
            {
                Gizmos.DrawLine(path[i - 1], path[i]);
            }
        }
        Gizmos.color = Color.magenta;
        if (correctPath != null && correctPath.Length > 0)
        {
            for (int i = 1; i < path.Length; i++)
            {
                Gizmos.DrawLine(correctPath[i - 1], correctPath[i]);
            }
        }
    }
}
