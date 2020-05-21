using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] CanvasFade loadingScreen = null;
    [SerializeField] int sceneToLoadOnPlay = 1;


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

        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;

        yield return SceneManager.LoadSceneAsync(buildIndex);

        //loading stuff

        //SceneManager.UnloadSceneAsync(currentBuildIndex);
        yield return SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(buildIndex));
        
        loadingScreen.SetCanvasState(CanvasState.FadeOut);
        while(loadingScreen.canvasState != CanvasState.Idle)
        {
            yield return null;
        }
        print(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        //todo -- load correct scene with correct save data
        Play();
    }

}
