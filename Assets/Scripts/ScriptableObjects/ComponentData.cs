using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "ComponentData", menuName = "City Builder Data/Component")]
public class ComponentData : ScriptableObject
{
    [Tooltip("Nombre que saldra en el menu")]
    public string displayName;
    [Tooltip("Icono que saldra en el menu")]
    public Sprite icon;
    [Tooltip("Categoria a la que pertenece")]
    public ComponentCategory category;
    [Tooltip("Descripcion")]
    [TextArea()]
    public string description;
    [Tooltip("Cuanto cuesta")]
    [Min(0)]
    public int cost;
    [Tooltip("Duracion en segundos reales")]
    [Min(0)]
    public int durability;
    [Tooltip("Duracion en segundos")]
    [Min(0)]
    public int powerConsumption;
    [Tooltip("Es un objeto premium?")]
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
    CUBIERTAS = 1,
    INTERRUPTORES = 2,
    TOMACORRIENTES = 3,
    SENSORES = 4,
    TERMICAS = 5
}