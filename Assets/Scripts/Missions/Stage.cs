using System.Collections.Generic;
using UnityEngine;

public class Stage
{
    #region Variables
    private string id; // Id
    private string uiEntry; // UI entry
    private bool stageCompleted = false; //Esta etapa esta completa?
    private Mission mission; //Mision a la pertenece esta etapa
    private List<Objective> objectives; //Los objetivos de esta etapa

    public string Id { get => id; }
    public string UiEntry { get => uiEntry; set => uiEntry = value; }
    public bool StageCompleted { get => stageCompleted; set => stageCompleted = value; }
    public Mission Mission { get => mission; }
    public List<Objective> Objectives { get => objectives; set => objectives = value; } 
    #endregion

    public Stage(string uiEntry)
    {
        this.id = "Stage-"+Utils.GenerateRandomString(5);
        this.uiEntry = uiEntry;
    }

    /// <summary>
    /// Se fija si todos los objeticos de la etapa estan completos y alerta al padre
    /// </summary>
    /// <returns></returns>
    public bool CheckIfStageIsCompleted()
    {
        bool completed = true;
        foreach(Objective o in objectives)
        {
            if(!o.Completed)
            {
                completed = false;
                break;
            }
        }
        stageCompleted = completed;
        if(completed)
        {
            mission.StageCompleted(this);
        }
        return completed;
    }

    /// <summary>
    /// Añade un objetico a esta etapa
    /// </summary>
    /// <param name="o"></param>
    public void AddObjective(Objective o)
    {
        if(objectives == null)
        {
            objectives = new List<Objective>();
        }
        o.Stage = this;
        objectives.Add(o);
        Debug.Log(o.Id + " añadido a esta etapa(" + id + ")");
    } //Añade objetivos a esta etapa

    /// <summary>
    /// Setea la mission padre de referencia
    /// </summary>
    /// <param name="m"></param>
    public void SetMission(Mission m)
    {
        mission = m;
        Debug.Log("Mission set");
    }
}
