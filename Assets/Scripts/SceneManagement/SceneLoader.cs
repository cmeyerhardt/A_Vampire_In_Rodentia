using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] CanvasFade loadingScreen = null;
    [SerializeField] int sceneToLoadOnPlay = 1;
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Play();
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void Play()
    {
        SceneManager.LoadScene(sceneToLoadOnPlay);
    }
    
    public IEnumerator LoadScene(int buildIndex)
    {
        loadingScreen.SetCanvasState(CanvasState.FadeIn);

        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;

        yield return SceneManager.LoadSceneAsync(buildIndex);

        //loading stuff

        yield return SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(buildIndex));

        SceneManager.UnloadSceneAsync(currentBuildIndex);

        loadingScreen.SetCanvasState(CanvasState.FadeOut);
        while(loadingScreen.canvasState != CanvasState.Idle)
        {
            yield return null;
        }
        print("Loading Complete");
    }

}
