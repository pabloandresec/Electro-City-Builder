using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "ComponentData", menuName = "City Builder Data/Component")]
public class ComponentData : ScriptableObject
{
    [SerializeField] private Sprite icon;
    [SerializeField] private string description;
    [SerializeField] private ComponentLevel[] levels;
}
[Serializable]
public struct ComponentLevel
{
    public string name;
    public Sprite icon;
    public string description;
    public int cost;
    public string durability;
    public string powerConsumption;
}
