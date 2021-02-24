using System;

[Serializable]
public class BuildingComponent
{
    public int index; //Indica que componente es en la lista inicial
    public int life; //Cuanta vida tiene el componente

    public BuildingComponent(ComponentData componentData)
    {
        this.index = componentData.Index;
        this.life = componentData.durability;
    }
}
