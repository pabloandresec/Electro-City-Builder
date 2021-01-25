using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class Building
{
    int listIndex; //indica el indice de donde se encuentra en la lista de datos de edificios
    int positionIndex; //indica la posicion en el mapa
    public List<BuildingComponent> components; //componentes que posee este edificio
    public List<ComponentLimit> currentCompAmounts;
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
        totalPowerConsumption = 0;
        if(components != null && components.Count > 0)
        {
            for (int i = 0; i < components.Count; i++)
            {
                totalPowerConsumption += referenceData[components[i].index].powerConsumption;
                components[i].life--;

                if(components[i].life <= 0)
                {
                    RemoveBuildingComponent(i, referenceData);
                }
            }
        }
    }

    public void RemoveBuildingComponent(int i, List<ComponentData> referenceData)
    {
        currentCompAmounts.Find(val => val.category == referenceData[components[i].index].category).val--;
        components.RemoveAt(i);
    }

    public void AddBuildingComponent(ComponentData componentData)
    {
        BuildingComponent bc = new BuildingComponent(componentData);
        components.Add(bc);
        Debug.Log("Added " + componentData.name + " to " + ListIndex + " at " + PositionIndex);
        UpdateComponentCounts(componentData.category);
    }

    private void UpdateComponentCounts(ComponentCategory _category)
    {
        if(currentCompAmounts == null)
        {
            currentCompAmounts = new List<ComponentLimit>();
            currentCompAmounts.Add(new ComponentLimit(_category, 1));
        }
        else
        {
            bool found = false;
            for (int i = 0; i < currentCompAmounts.Count; i++)
            {
                if(currentCompAmounts[i].category == _category)
                {
                    currentCompAmounts[i].val += 1;
                    found = true;
                    break;
                }
            }
            if(!found)
            {
                currentCompAmounts.Add(new ComponentLimit(_category, 1));
            }
        }
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
