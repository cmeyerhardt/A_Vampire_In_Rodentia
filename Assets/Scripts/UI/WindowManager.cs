using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    //todo -- make this a Stack?
    [SerializeField] List<GameObject> openWindows = new List<GameObject>();
    [SerializeField] GameObject escapeKeyMenu = null;

    private void Awake()
    {
        //foreach(Transform child in transform)
        //{
        //    openWindows.Add(child.gameObject);
        //}
    }

    void Start()
    {
        //exitConfirmation = transform.Find("ExitConfirmation").gameObject;
        //CloseAllWindows();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(openWindows.Count > 0)
            {
                CloseWindow(openWindows[openWindows.Count - 1]);
            }
            else
            {
                OpenWindow(escapeKeyMenu);
            }
        }
    }

    public void OpenWindow(GameObject toOpen)
    {
        print(toOpen);
        if (toOpen == escapeKeyMenu)
        {
            CloseAllWindows();
        }

        if (!openWindows.Contains(toOpen))
        {
            openWindows.Add(toOpen);
        }

        toOpen.SetActive(true);
    }

    public void CloseWindow(GameObject toClose)
    {
        if (openWindows.Contains(toClose))
        {
            openWindows.Remove(toClose);
        }
        else
        {
            Debug.LogFormat("{0} window attempted to close while not contained in openWindows.", toClose);
        }
        toClose.SetActive(false);
    }

    public void CloseAllWindows()
    {
        int i = openWindows.Count;
        while(i > 0)
        {
            if(openWindows[i-1] != null)
            {
                CloseWindow(openWindows[i-1]);
            }
            i--;
        }
    }
}
