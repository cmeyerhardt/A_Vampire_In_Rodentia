using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    //todo -- use a Stack?
    [SerializeField] List<Window> openWindows = new List<Window>();
    [SerializeField] Window escapeKeyMenu = null;

    private void Awake()
    {

    }

    void Start()
    {

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

    public void OpenWindow(Window toOpen)
    {
        //print(toOpen);
        if(toOpen == null) { return; }
        if (toOpen == escapeKeyMenu)
        {
            CloseAllWindows();
        }

        if (!openWindows.Contains(toOpen))
        {
            openWindows.Add(toOpen);
        }

        toOpen.gameObject.SetActive(true);
        toOpen.openWindowEvent.Invoke();
    }

    public void CloseWindow(Window toClose)
    {
        if (toClose == null) { return; }
        if (openWindows.Contains(toClose))
        {
            openWindows.Remove(toClose);
        }
        else
        {
            Debug.LogFormat("{0} window attempted to close while not contained in openWindows.", toClose);
        }
        toClose.gameObject.SetActive(false);
        toClose.closeWindowEvent.Invoke();
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
