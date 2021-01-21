using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class Building
{
    int listIndex; //indica el indice de donde se encuentra en la lista de datos de edificios
    int positionIndex; //indica la posicion en el mapa
    public List<BuildingComponent> components; //componentes que posee este edificio
    public int totalPowerConsumption; //Poder que consumio este edificio

    public int ListIndex { get => listIndex; set => listIndex = value; }
    public int PositionIndex { get => positionIndex; set => positionIndex = value; }

    #region Constructors

    public Building(BuildingData data, int posInd)
    {
        this.listIndex = data.Index;
        this.positionIndex = posInd; 
        this.components = new List<BuildingComponent>();
    }

    #endregion

    public void UpdateComponentsLife(List<ComponentData> referenceData)
    {
        if(components != null && components.Count > 0)
        {
            foreach (BuildingComponent bc in components)
            {
                totalPowerConsumption += referenceData[bc.index].powerConsumption;
                bc.life = bc.life - 1;
            }
        }
    }

    public void AddBuildingComponent(ComponentData componentData, int quant)
    {
        BuildingComponent bc = new BuildingComponent(componentData);
        for (int i = 0; i < quant; i++)
        {
            components.Add(bc);
        }
        Debug.Log("Added " + quant + " " + componentData.name + " to " + listIndex);
    }
}
[Serializable]
public class BuildingComponent
{
    public int index;
    public int life;

    public BuildingComponent(ComponentData componentData)
    {
        this.index = componentData.Index;
        this.life = componentData.durability;
    }
}
