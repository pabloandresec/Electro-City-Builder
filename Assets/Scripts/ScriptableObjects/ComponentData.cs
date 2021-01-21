using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "ComponentData", menuName = "City Builder Data/Component")]
public class ComponentData : ScriptableObject
{
    [SerializeField] string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private ComponentCategory category;
    [SerializeField] private string description;
    [SerializeField] public int cost;
    [SerializeField] public int durability;
    [SerializeField] public int powerConsumption;
    private int index;

    public int Index { get => index; }

    public void SetIndex(int i)
    {
        this.index = i;
    }
}

public enum ComponentCategory
{
    ILUMINACION,
    CABLEADO,
    TOMAS_DE_ELECTRICIDAD
}