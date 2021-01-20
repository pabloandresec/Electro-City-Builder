using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Building
{
    int totalPowerConsumption;
    public List<BuildingComponent> components;

    public void CalculateTotalPowerComsumption(BuildingData data)
    {

    }
}

public class BuildingComponent
{
    List<int> devices;
}
