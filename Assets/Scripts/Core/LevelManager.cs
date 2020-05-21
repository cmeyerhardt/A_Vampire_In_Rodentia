using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [SerializeField] Window loseMenu = null;
    [SerializeField] Window winMenu = null;
    //[SerializeField] Window pauseMenu = null;

    [SerializeField] WindowManager windowManager = null;

    PlayerController player = null;
    SceneLoader sceneLoader = null;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        sceneLoader = FindObjectOfType<SceneLoader>();
    }

    void Update()
    {
        if(player.isDead)
        {
            GameOver();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void UnPauseGame()
    {
        Time.timeScale = 1f;
    }

    public void GameTimeDone()
    {
        if ((player.isHidden || player.playerState == PlayerState.Hiding) && !player.isDead && player.stamina.GetStaminaValue() > 0f)
        {
            Win();
        }
        else
        {
            GameOver();
        }
    }

    private void Win()
    {
        windowManager.OpenWindow(winMenu);
    }

    private void GameOver()
    {
        windowManager.OpenWindow(loseMenu);
    }

    public void QuitLevel()
    {
        UnPauseGame();
        sceneLoader.ReturnToMenu();
    }

    public void NextLevel()
    {
        UnPauseGame();
        sceneLoader.NextLevel();
    }
}
