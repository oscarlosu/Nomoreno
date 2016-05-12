using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class NPCPart
{
    public enum NPCPartType {
        None,
        Pants,
        Shirt,
        Hat,
        Item
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

    public string GetFileName() {
        string fileName = "";
        string[] words = Description.Split(' ');
        fileName += words[0];
        for(int i = 1; i < words.Length; ++i) {
            fileName += "_";
            fileName += words[i];
        }
        return fileName;
    }
}
