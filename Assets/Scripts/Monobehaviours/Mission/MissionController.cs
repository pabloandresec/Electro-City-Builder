using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MissionController : MonoBehaviour
{
    [Header("Missions")]
    [SerializeField] private List<Mission> missions;
    [SerializeField] private List<Mission> activeMissions;
    [SerializeField] private List<Mission> completedMissions;
    [Header("References")]
    [SerializeField] private GameController game;
    [SerializeField] private UIController ui;

    private void Start()
    {
        CheckForReferences();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            PrintMissions();
        }
    }

    private void CheckForReferences()
    {
        if (ui == null)
        {
            ui = GameObject.FindGameObjectWithTag("UI").GetComponent<UIController>();
        }
        if (game == null)
        {
            game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        }
    }

    public void PrintMissions()
    {
        string s = "";
        foreach (Mission m in missions)
        {
            s = "-" + m.UiEntry + " -> "+ m.MissionCompleted +"\n";
            foreach (Stage sta in m.Stages)
            {
                s += "  *" + sta.UiEntry + "\n";
                foreach (Objective o in sta.Objectives)
                {
                    s += "      >" + o.UiEntry + " -> ";
                    s += o.Completed ? "Completed" : "InProgress";
                }
            }
        }
        Debug.Log(s+ "\n\n\n\n");
    }

    public void MissionFinished(string id)
    {
        Mission m = missions.FirstOrDefault(mis => mis.Id == id);
        if(m != null)
        {
            if(completedMissions != null)
            {
                completedMissions = new List<Mission>();
            }
            completedMissions.Add(m);
            missions.Remove(m);
        }
    }

    public void CreateCameraMission()
    {
        //Se Crean los objetivos (pueden ser varios...)
        CameraMovementObjective cameraMovementObj = new CameraMovementObjective("Mueve la camara en todas las direcciones", () => {
            ui.GetComponent<CharController>().ShowDialog(1);
            game.SwitchState(1); //lock all Input
        });
        //Se crea la etapa que contendra los objetivos
        Stage cameraMovement = new Stage("Aprende a mover la camara");
        //Añadimos los objetivos a la etapa
        cameraMovement.AddObjective(cameraMovementObj);
        //Creamos la mision con las etapas de parametro...
        Mission cameraMovementTutorial = new Mission("Tutorial: ", new Stage[] { cameraMovement }, MissionFinished);
        //Añadimos la mision a la lista de misiones
        if (missions == null)
        {
            missions = new List<Mission>();
        }
        missions.Add(cameraMovementTutorial);
    }

    private void CreateTestMission()
    {
        //Se Crean los objetivos (pueden ser varios...)
        BuildingConstructedObjective constructApartmentsE = new BuildingConstructedObjective("Pulsa un casilla vacia y presiona construir... luego elige la opcion " + game.Buildings[6].name, game.Buildings[6], null);
        //Se crea la etapa que contendra los objetivos
        Stage constructStage = new Stage("Aprende a construir edificios");
        //Añadimos los objetivos a la etapa
        constructStage.AddObjective(constructApartmentsE);
        //Creamos la mision con las etapas de parametro...
        Mission tutorial = new Mission("Tutorial: ", new Stage[1] { constructStage }, MissionFinished);
        //Añadimos la mision a la lista de misiones
        if (missions == null)
        {
            missions = new List<Mission>();
        }
        missions.Add(tutorial);
    }

    public void CreateZoomMission()
    {
        //Se Crean los objetivos (pueden ser varios...)
        CameraZoomObjective cameraZoomObj = new CameraZoomObjective("Haz zoom con la camara", 1f, 2.5f, () => {
            ui.GetComponent<CharController>().ShowDialog(2);
            game.SwitchState(1); //lock all Input
        });
        //Se crea la etapa que contendra los objetivos
        Stage cameraZoom = new Stage("Aprende a hace zoom con la camara");
        //Añadimos los objetivos a la etapa
        cameraZoom.AddObjective(cameraZoomObj);
        //Creamos la mision con las etapas de parametro...
        Mission cameraZoomTutorial = new Mission("Tutorial: ", new Stage[] { cameraZoom }, MissionFinished);
        //Añadimos la mision a la lista de misiones
        if (missions == null)
        {
            missions = new List<Mission>();
        }
        missions.Add(cameraZoomTutorial);
    }

    public void CreateAnyBuildingMission()
    {
        //Se Crean los objetivos (pueden ser varios...)
        BuildingConstructedObjective constructApartmentsE = new BuildingConstructedObjective("Construye algo" , null, ()=> {
            ui.GetComponent<CharController>().ShowDialog(13);
        });
        //Se crea la etapa que contendra los objetivos
        Stage constructStage = new Stage("Aprende a construir edificios");
        //Añadimos los objetivos a la etapa
        constructStage.AddObjective(constructApartmentsE);
        //Creamos la mision con las etapas de parametro...
        Mission tutorial = new Mission("Tutorial: ", new Stage[1] { constructStage }, MissionFinished);
        //Añadimos la mision a la lista de misiones
        if (missions == null)
        {
            missions = new List<Mission>();
        }
        missions.Add(tutorial);
    }
}
