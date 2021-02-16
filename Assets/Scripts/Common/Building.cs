using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class Building
{
    int listIndex;                                  //indica el indice de donde se encuentra en la lista de datos de edificios
    int positionIndex;                              //indica la posicion en el mapa
    public List<BuildingComponent> components;      //componentes que posee este edificio
    public List<ComponentLimit> currentCompAmounts; //montos categorizados de los componentes
    public int totalPowerConsumption;               //Poder que consumio este edificio

    public int ListIndex { get => listIndex; set => listIndex = value; }
    public int PositionIndex { get => positionIndex; set => positionIndex = value; }

    #region Constructors

    public Building(BuildingData data, int posInd)
    {
        this.listIndex = data.Index;
        this.positionIndex = posInd; 
        this.components = new List<BuildingComponent>();
        this.currentCompAmounts = new List<ComponentLimit>();
    }

    #endregion

    public void UpdateComponentsLife(List<ComponentData> referenceData, float timePassed)
    {
        CalculateOnlyFirstComponentByCategory(referenceData, timePassed);
        CheckForActiveComponents();
    }

    private void CheckForActiveComponents()
    {
        if (components == null)
        {
            return;
        }
        GameController gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        if (components.Count == 0)
        {
            gc.SetBuildingElectricVisibility(false, Utils.IndexToTilePos(positionIndex,GameController.currentWidth), gc.Buildings[listIndex]);
        }
        else
        {
            gc.SetBuildingElectricVisibility(true, Utils.IndexToTilePos(positionIndex, GameController.currentWidth), gc.Buildings[listIndex]);
        }
    }

    public void PayRent()
    {
        int rent = 0;
        GameController gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        BuildingData bd = gc.Buildings[listIndex];
        if(currentCompAmounts == null || currentCompAmounts.Count <= 0)
        {
            Debug.Log("Building has no sevices");
            return;
        }

        foreach(ComponentLimit cl in currentCompAmounts)
        {
            if(cl.val > 0)
            {
                rent += bd.rent;
            }
        }
        Debug.Log("paying " + rent);
        gc.AddMoney(rent);
    }

    private void CalculateOnlyFirstComponentByCategory(List<ComponentData> referenceData, float timePassed)
    {
        totalPowerConsumption = 0;
        if (currentCompAmounts != null && components != null && currentCompAmounts.Count > 0 && components.Count > 0)
        {
            for (int i = 0; i < currentCompAmounts.Count; i++)
            {
                BuildingComponent bc = components.First(val => referenceData[val.index].category == currentCompAmounts[i].category);
                if (bc != null)
                {
                    bc.life -= (int)timePassed;
                    if (bc.life <= 0)
                    {
                        RemoveBuildingComponent(bc, referenceData);
                        GameObject.FindGameObjectWithTag("UI").GetComponent<UIController>().AddAttentionBubble(positionIndex.ToString(), Utils.IndexToTilePos(positionIndex, GameController.currentWidth));
                    }
                    else
                    {
                        totalPowerConsumption += referenceData[bc.index].powerConsumption;
                    }
                }
            }
        }
    }

    /// <summary>
    /// End Desuso calculat todos los componentes al mismo tiempo
    /// </summary>
    /// <param name="referenceData"></param>
    /// <param name="timePassed"></param>
    private void CalculateAllComponentsAtTheSameTime(List<ComponentData> referenceData, float timePassed)
    {
        totalPowerConsumption = 0;
        for (int i = 0; i < components.Count; i++)
        {
            totalPowerConsumption += referenceData[components[i].index].powerConsumption;
            components[i].life -= (int)timePassed;
            if (components[i].life <= 0)
            {
                RemoveBuildingComponent(i, referenceData);
                GameObject.FindGameObjectWithTag("UI").GetComponent<UIController>().AddAttentionBubble(positionIndex.ToString(), Utils.IndexToTilePos(positionIndex, GameController.currentWidth));
            }
        }
    }

    /// <summary>
    /// Busca la categoria correspondiente y actualiza los valores al remover el componente
    /// </summary>
    /// <param name="i"></param>
    /// <param name="referenceData"></param>
    public void RemoveBuildingComponent(int i, List<ComponentData> referenceData)
    {
        ComponentLimit cl = currentCompAmounts.Find(val => val.category == referenceData[components[i].index].category);
        cl.val--;
        if(cl.val <= 0)
        {
            currentCompAmounts.Remove(cl);
        }
        components.RemoveAt(i);
    }

    /// <summary>
    /// Busca la categoria correspondiente y actualiza los valores al remover el componente
    /// </summary>
    /// <param name="i"></param>
    /// <param name="referenceData"></param>
    public void RemoveBuildingComponent(BuildingComponent bd, List<ComponentData> referenceData)
    {
        ComponentLimit cl = currentCompAmounts.Find(val => val.category == referenceData[bd.index].category); //Busca en la lista "cantidaded de componentes" la categoria correspondiente y actualiza el monto --
        cl.val--;
        if (cl.val <= 0)
        {
            currentCompAmounts.Remove(cl);
        }
        components.Remove(bd); //Quita el componenete de la lista prioncipal
    }

    /// <summary>
    /// Añade un componente a este edificio
    /// </summary>
    /// <param name="i"></param>
    /// <param name="referenceData"></param>
    public void AddBuildingComponent(ComponentData componentData)
    {
        if(components == null)
        {
            components = new List<BuildingComponent>();
        }
        GameObject.FindGameObjectWithTag("UI").GetComponent<UIController>().AddAddedBubble(positionIndex.ToString(), Utils.IndexToTilePos(positionIndex, GameController.currentWidth), 0.5f);
        BuildingComponent bc = new BuildingComponent(componentData); //Con el componente de referencia(componentData) se crea este componente activo
        components.Add(bc); //Se añade a la lista de componentes
        UpdateComponentCounts(componentData.category); //Actualizamos los montos x categoria correspondientes
        Debug.Log("Added " + componentData.name + " to " + ListIndex + " at pos" + Utils.IndexToTilePos(positionIndex, GameController.currentWidth));
    }


    /// <summary>
    /// Actualiza los montos x categoria
    /// </summary>
    /// <param name="_category"></param>
    private void UpdateComponentCounts(ComponentCategory _category)
    {
        if(currentCompAmounts == null) //Esta la lista de montos categorizados inicializada?
        {
            currentCompAmounts = new List<ComponentLimit>();
            currentCompAmounts.Add(new ComponentLimit(_category, 1));
        }
        else //Si la lista de montos categorizados existe
        {
            bool found = false;
            for (int i = 0; i < currentCompAmounts.Count; i++) //Buscamos la categoria correspondiente
            {
                if(currentCompAmounts[i].category == _category) //Si la encontramos añadimos el valor nuevo
                {
                    currentCompAmounts[i].val += 1;
                    found = true;
                    break;
                }
            }
            if(!found) //NO existe esa categoria el el edifico... la vamos a añadir
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
