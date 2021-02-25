using System;
using UnityEngine;

[Serializable]
public class SelectionInfo
{
    [SerializeField] private Vector3Int cellPosition;
    [SerializeField] private Building building;

    public Vector3Int CellPosition { get => cellPosition; }
    public Building Building { get => building; }

    public SelectionInfo(Vector3Int cellPosition, Building building)
    {
        this.cellPosition = cellPosition;
        this.building = building;
    }

    
}
