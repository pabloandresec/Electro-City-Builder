using System;
using UnityEngine;

public class BuildingConstructedObjective : Objective
{
    BuildingData buildingObjective;

    public BuildingConstructedObjective(string _uiEntry ,BuildingData _bd, Action _OnComplete)
    {
        uiEntry = _uiEntry;
        buildingObjective = _bd;
        id = "Objective-Build-" + Utils.GenerateRandomString(5);
        completed = false;
        GameController.BuildingContructed += CheckForBuildingConstructed;
        onComplete = _OnComplete;
    }

    private void CheckForBuildingConstructed(BuildActionInfo obj)
    {
        if(buildingObjective == null)
        {
            if(obj.Builded)
            {
                CompleteObjective();
            }
        }
        else
        {
            if (obj.BuildingDataRef == buildingObjective)
            {
                CompleteObjective();
            }
        }
    }

    private void CompleteObjective()
    {
        OnComplete?.Invoke();
        completed = true;
        GameController.BuildingContructed -= CheckForBuildingConstructed;
        GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>().PlaySFX(3);
        stage.CheckIfStageIsCompleted();
    }
}