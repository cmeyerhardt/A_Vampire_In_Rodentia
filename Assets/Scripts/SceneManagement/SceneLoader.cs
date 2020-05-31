using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public UnityEvent doneLoadingEvent;
    [SerializeField] CanvasFade loadingScreen = null;
    [SerializeField] int sceneToLoadOnPlay = 1;
    [SerializeField] float loadWaitTime = 2f;

    public void ExitGame()
    {
        Application.Quit();
    }

    public void Play()
    {
        StartCoroutine(LoadSceneByIndex(sceneToLoadOnPlay));
    }

    public void ReturnToMenu()
    {
        StartCoroutine(LoadSceneByIndex(0));
    }
    
    public IEnumerator LoadSceneByIndex(int buildIndex)
    {
        loadingScreen.SetCanvasState(CanvasState.FadeIn);
        while (loadingScreen.canvasState != CanvasState.Idle)
        {
            yield return null;
        }
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;

        yield return SceneManager.LoadSceneAsync(buildIndex);

        //loading stuff

        //SceneManager.UnloadSceneAsync(currentBuildIndex);
        yield return SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(buildIndex));

        //yield return new WaitForSecondsRealtime(loadWaitTime);

        loadingScreen.SetCanvasState(CanvasState.FadeOut);
        while(loadingScreen.canvasState != CanvasState.Idle)
        {
            yield return null;
        }
        doneLoadingEvent.Invoke();
        LoadSplashMenu();
        //print(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        //todo -- load correct scene with correct save data
        Play();
    }

    public void LoadSplashMenu()
    {
        FindObjectOfType<LevelManager>().PauseGame();
        FindObjectOfType<WindowManager>().OpenSplashScreen();
    }

}
