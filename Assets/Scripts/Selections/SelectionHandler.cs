using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectionHandler : MonoBehaviour
{
    [SerializeField] private SelectionInfo selection = null;
    [SerializeField] private bool selected = false;
    public SelectionInfo Selection { get => selection; }

    public void Select(Vector3Int cellPos, Building building)
    {
        SelectionInfo newSelection = new SelectionInfo(cellPos, building);
        selection = newSelection;
        selected = true;
        Debug.Log("Selected " + cellPos);
    }

    public void Deselect()
    {
        selection = null;
        selected = false;
        Debug.Log("Deselected");
    }

    public bool HasSelection()
    {
        return selected;
    }
}