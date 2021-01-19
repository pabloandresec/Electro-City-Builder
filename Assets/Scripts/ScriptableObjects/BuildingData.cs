using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData",menuName = "City Builder Data/Building")]
public class BuildingData : ScriptableObject
{
    [Header("Sprites")]
    [SerializeField] private Sprite sprite;
    [TextArea()]
    [SerializeField] private string description;
    [Header("Build Settings")]
    [SerializeField] private int buildingCost;
    [Header("Upgrade Settings")]
    [SerializeField] private int upgradeCost;
    [SerializeField] private string upgradeBuildingName;
}
