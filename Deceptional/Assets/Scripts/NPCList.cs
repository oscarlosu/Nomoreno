using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
*   Singleton class that holds a list of the NPCs present in the scene
*
*/
public class NPCList : IEnumerable
{
    private static NPCList instance;
    public static NPCList Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new NPCList();
            }
            return instance;
        }
    }
    private List<NPC> list;

    public NPCList()
    {
        list = new List<NPC>();
    }
    public NPC Get(int index)
    {
        return list[index];
    }
    public void Add(NPC npc)
    {
        list.Add(npc);
    }

    public IEnumerator GetEnumerator() {
        return ((IEnumerable)list).GetEnumerator();
    }

    public int Count
    {
        get
        {
            return list.Count;
        }
    }
}
