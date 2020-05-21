using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Window : MonoBehaviour
{
    public UnityEvent openWindowEvent;
    public UnityEvent closeWindowEvent;

    public void WindowEvent(bool open)
    {
        if(open)
        {
            openWindowEvent.Invoke();
        }
        else
        {
            closeWindowEvent.Invoke();
        }
    }
}
