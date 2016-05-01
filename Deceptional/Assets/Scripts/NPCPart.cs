using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class NPCPart
{
    public enum NPCPartType {
        None,
        Pants,
        Shirt,
        Hat
    }
    public enum NPCPartDescription
    {
        Red,
        Blue,
        Green,
        Yellow,
        Black,
        White
    }

    public NPCPartType Type { get; set; }
    //public NPCPartDescription Description { get; set; }
    public string Description { get; set; }

    //public NPCPart(NPCPartType type, NPCPartDescription description)
    public NPCPart(NPCPartType type, int descIdx)
    {
        Type = type;
        var posParts = Assets.Scripts.IO.FileLoader.GetParts(type);
        descIdx = descIdx % posParts.Count;
        Description = posParts[descIdx];
    }

    /*
    private Mesh model;
    public Mesh Model { get; set; }

    private Material material;
    public Material Material { get; set; }

    public NPCPart(Mesh model, Material material, NPCPartType type, NPCPartDescription description)
    {
        this.model = model;
        this.material = material;
        this.type = type;
        this.description = description;
    }
    */
}
