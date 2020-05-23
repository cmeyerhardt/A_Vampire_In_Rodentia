using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum CursorType { None, UI, Victim, Guard, Hide, PlayerDead, PickUp };

[System.Serializable]
public class CursorMapping
{
    public CursorType type = CursorType.None;
    public Texture2D texture = null;
    [HideInInspector] public Vector2 hotspot = Vector2.zero;
}

public class CursorControl : MonoBehaviour
{


    [Header("Cursor")]
    [SerializeField] CursorMapping[] cursorMappings = null;

    //Cache
    PlayerController player = null;

    private void Awake()
    {
        SetCursor(CursorType.None);
    }

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        ProcessRaycast();
    }

    /***********************
    * RAYCASTING
    ***********************/

    private void ProcessRaycast()
    {
        if (InteractWithUI()) { return; }
        if(player != null)
        {
            if (player.isDead)
            {
                SetCursor(CursorType.PlayerDead);
                return;
            }
            if (InteractWithComponent()) { return; }
        }

        SetCursor(CursorType.None);
    }

    private bool InteractWithUI()
    {
        if (EventSystem.current.IsPointerOverGameObject()) //is the cursor over UI?
        {
            SetCursor(CursorType.UI);
            return true;
        }
        return false;
    }

    private bool InteractWithComponent()
    {
        RaycastHit[] hits = SortRaycasts();

        foreach (RaycastHit hit in hits)
        {
            IRaycast[] raycastables = hit.transform.GetComponents<IRaycast>();
            foreach (IRaycast raycastable in raycastables)
            {
                if (raycastable.HandleRaycast(player))
                {
                    SetCursor(raycastable.GetCursorType());
                    return true;
                }
            }
        }
        return false;
    }

    RaycastHit[] SortRaycasts()
    {
        //get all hits
        RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());

        float[] distances = new float[hits.Length];
        for (int i = 0; i < hits.Length; i++)
        {
            distances[i] = hits[i].distance;
        }
        Array.Sort(distances, hits);

        //sort array of hits
        //return
        return Physics.RaycastAll(GetMouseRay());
    }

    private static Ray GetMouseRay()
    {
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }

    /***********************
    * CURSOR
    ***********************/

    private void SetCursor(CursorType type)
    {
        CursorMapping mapping = GetCursorMapping(type);
        Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
    }

    private CursorMapping GetCursorMapping(CursorType type)
    {
        foreach (CursorMapping mapping in cursorMappings)
        {
            if (mapping.type == type)
            {
                return mapping;
            }
        }
        return cursorMappings[0];
    }
}
