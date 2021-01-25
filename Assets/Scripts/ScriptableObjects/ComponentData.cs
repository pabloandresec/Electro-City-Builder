using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "ComponentData", menuName = "City Builder Data/Component")]
public class ComponentData : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public ComponentCategory category;
    public string description;
    public int cost;
    public int durability;
    public int powerConsumption;
    public bool premium;
    public Color color;
    private int index;

    public int Index { get => index; }

    public void SetIndex(int i)
    {
        this.index = i;
    }
}

public enum ComponentCategory
{
    ILUMINACION = 0,
    CABLEADO = 1,
    TOMAS_DE_ELECTRICIDAD = 2,
    TERMICAS = 3
}