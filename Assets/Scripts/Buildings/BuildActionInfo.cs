using UnityEngine;
using System.Collections;

public class BuildActionInfo
{
    private Vector3 worldPos;
    private Vector3Int cellPos;
    private BuildingData buildingDataRef;
    private bool builded;

    public BuildActionInfo(Vector3 worldPos, Vector3Int cellPos, BuildingData buildingDataRef, bool builded)
    {
        this.worldPos = worldPos;
        this.cellPos = cellPos;
        this.buildingDataRef = buildingDataRef;
        this.builded = builded;
    }

    public Vector3 WorldPos { get => worldPos; set => worldPos = value; }
    public Vector3Int CellPos { get => cellPos; set => cellPos = value; }
    public BuildingData BuildingDataRef { get => buildingDataRef; set => buildingDataRef = value; }
    public bool Builded { get => builded; set => builded = value; }
}
