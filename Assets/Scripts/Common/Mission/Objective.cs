using System;

public abstract class Objective
{
    protected string id; // Id
    protected string uiEntry; //nombre del objetivo
    protected Stage stage; //Etapa a la que este objectivo pertenece
    protected bool completed; //flag de objectivo completo
    protected Action onComplete; //Accion a ejecutar cuando se completa un objetivo

    public bool Completed { get => completed; }
    public string Id { get => id;}
    public string UiEntry { get => uiEntry; }
    public Stage Stage { get => stage; set => stage = value; }
    public Action OnComplete { get => onComplete; }
}
