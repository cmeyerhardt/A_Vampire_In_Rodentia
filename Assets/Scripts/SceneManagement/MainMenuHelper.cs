using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuHelper : MonoBehaviour
{
    public void Play()
    {
        FindObjectOfType<SceneLoader>().Play();
    }

    public void QuitGame()
    {
        FindObjectOfType<SceneLoader>().ExitGame();
    }
}
