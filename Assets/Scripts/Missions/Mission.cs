using UnityEngine;
using System.Collections;
using System;
using System.Linq;

[Serializable]
public class Mission
{
    [SerializeField] private string id; //nombre de la mission
    [SerializeField] private bool missionCompleted; //mission Completed state
    [SerializeField] private string uiEntry; //nombre de la mission
    [SerializeField] private int currentStage = 0; //etapa actual de la mission
    private Action<string> onMissionFinished; //etapa actual de la mission
    private Stage[] stages; //todas las etapas que pertenecen a la mission

    public string Id { get => id; }
    public string UiEntry { get => uiEntry; }
    public int CurrentStage { get => currentStage; }
    public Stage[] Stages { get => stages; }
    public bool MissionCompleted { get => missionCompleted; }
    public Action<string> OnMissionFinished { get => onMissionFinished; }

    public Mission(string _uiEntry, Stage[] _stage, Action<string> _onMissionFinished)
    {
        this.id = "Mission-" + Utils.GenerateRandomString(5);
        this.uiEntry = _uiEntry;
        this.stages = _stage;
        for (int i = 0; i < stages.Length; i++)
        {
            stages[i].SetMission(this);
        }
        onMissionFinished = _onMissionFinished;
    }

    /// <summary>
    /// Se llama cuando una etapa se completa... actualiza el estado correspondiente y se fija si las etapas se completaron
    /// </summary>
    /// <param name="completedStage"></param>
    public void StageCompleted(Stage completedStage)
    {
        Stage first = stages.FirstOrDefault(s => s == completedStage);
        if (first == stages[currentStage])
        {
            currentStage++;
        }
        first.StageCompleted = true;
        CheckIfMissionCompleted();
    }

    /// <summary>
    /// Se fija si todas las rtapas estan completas y regresa y actualiza el estado
    /// </summary>
    /// <returns></returns>
    private bool CheckIfMissionCompleted()
    {
        bool com = true;
        foreach (Stage s in stages)
        {
            if (!s.StageCompleted)
            {
                com = false;
                break;
            }
        }
        missionCompleted = com;
        if(com)
        {
            onMissionFinished?.Invoke(id);
        }
        return com;
    }
}
