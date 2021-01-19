using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData",menuName = "City Builder Data/Building")]
public class BuildingData : ScriptableObject
{
    [Header("Sprites")]
    [SerializeField] private Sprite north;
    [SerializeField] private Sprite east;
    [SerializeField] private Sprite south;
    [SerializeField] private Sprite west;
    [Space(10)]
    [SerializeField] private Sprite interior;
    [SerializeField] private string description;
    [Header("Build Settings")]
    [SerializeField] private int buildingCost;
    [SerializeField] private float buildingTime;
    [Header("Upgrade Settings")]
    [SerializeField] private int upgradeCost;
    [SerializeField] private float upgradeTime;
    [SerializeField] private string upgradeBuildingName;
}
