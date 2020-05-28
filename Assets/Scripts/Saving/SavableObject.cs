using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

[ExecuteAlways]
public class SavableObject : MonoBehaviour
{
    [SerializeField] string uniqueIdentifier = "";
    static Dictionary<string, SavableObject> globalUUID = new Dictionary<string, SavableObject>(); //exists throughout all scenes


    public string GetUniqueIdentifier()
    {
        return uniqueIdentifier;
    }

    //public object SaveData()
    //{
    //    Dictionary<string, object> state = new Dictionary<string, object>();
    //    foreach (ISave saveable in GetComponents<ISave>())
    //    {
    //        state[saveable.GetType().ToString()] = saveable.Save();
    //    }
    //    return state;

    //}

    //public void LoadData(object state)
    //{
    //    Dictionary<string, object> stateDict = (Dictionary<string, object>)state;

    //    foreach (ISave saveable in GetComponents<ISave>())
    //    {
    //        string typeString = saveable.GetType().ToString();
    //        if (stateDict.ContainsKey(typeString))
    //        {
    //            saveable.Load(stateDict[typeString]);
    //        }
    //    }

    //}

#if UNITY_EDITOR
    private void Update()
    {
        if (Application.IsPlaying(gameObject)) { return; } // don't execute while playing
        if (string.IsNullOrEmpty(gameObject.scene.path)) { return; } // don't execute if editing a prefab

        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty property = serializedObject.FindProperty("uniqueIdentifier");

        if (string.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue)) // if no guid exists, create one
        {
            property.stringValue = Guid.NewGuid().ToString();
            serializedObject.ApplyModifiedProperties();
        }
        globalUUID[property.stringValue] = this;
    }
#endif

    private bool IsUnique(string candidate)
    {
        if (!globalUUID.ContainsKey(candidate))
        {
            return true;
        }
        if (globalUUID[candidate] == this)
        {
            return true;
        }
        if (globalUUID[candidate] == null)
        {
            globalUUID.Remove(candidate);
            return true;
        }

        if (globalUUID[candidate].GetUniqueIdentifier() != candidate)
        {
            globalUUID.Remove(candidate);
            return true;
        }

        return false;
    }
}
