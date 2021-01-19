using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "ComponentData", menuName = "City Builder Data/Component")]
public class ComponentData : ScriptableObject
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private string description;
    [SerializeField] private int cost;
    [SerializeField] private int powerConsumption;
}
