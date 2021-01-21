using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "BuildingData",menuName = "City Builder Data/Building")]
public class BuildingData : ScriptableObject
{
    [Header("Sprites")]
    public Sprite icon;
    public Tile tile;
    [TextArea()]
    public string description;
    [Header("Build Settings")]
    public int buildingCost;
    [Header("Upgrade Settings")]
    public int upgradeCost;
    public string upgradeBuildingName;
    private int index;

    public int Index { get => index; }

    public void SetIndex(int i)
    {
        this.index = i;
    }
}
