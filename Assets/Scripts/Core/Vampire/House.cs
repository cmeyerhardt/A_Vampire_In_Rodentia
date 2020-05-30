using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    StringEvent mouseDeathEvent;
    [SerializeField] Villager[] miceThatLiveHere = null;

    Dictionary<string, bool> miceStatus = new Dictionary<string, bool>();
    
    private void Awake()
    {
        for (int i = 0; i < miceThatLiveHere.Length; i++)
        {
            miceStatus.Add(miceThatLiveHere[i].GetComponent<SavableObject>().GetUniqueIdentifier(), true);
            miceThatLiveHere[i].GetComponent<Health>().deathEvent.AddListener(MouseDied);
        }
    }

    public void MouseDied(string uuid)
    {
        miceStatus[uuid] = false;
    }

    public int GetNumAliveMice()
    {
        int _num = 0;
        foreach(KeyValuePair<string, bool> mouse in miceStatus)
        {
            if(mouse.Value)
            {
                _num++;
            }
        }
        return _num;
    }
}
