using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [SerializeField]PlayerVFX playerVFX;

    public void StartGame()
    {
        StartCoroutine(LoadLevel1());
    }

    private void Start()
    {
        ScriptLoadSequencer.LoadScripts();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator LoadLevel1()
    {
        playerVFX.BeginFadeScreen();
        yield return null;
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        while (!playerVFX.isFaded)
        {
            yield return null;
        }

        var loadingNextScene = SceneManager.LoadSceneAsync(nextIndex);
        while (!loadingNextScene.isDone)
        {
            print("loading scene");
            yield return null;
        }

    }

    public void StartSceneByName(string name)
    {
        StartCoroutine(LoadLevelByName((name)));
    }

    public void StartSceneByIndex(int index)
    {
        StartCoroutine(LoadLevelByBuildIndex(index));
    }
    IEnumerator LoadLevelByName(string name)
    {
        print(name);
        playerVFX.BeginFadeScreen();
        yield return null;
        int nextIndex = SceneManager.GetSceneByName(name).buildIndex;

        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        while (!playerVFX.isFaded)
        {
            yield return null;
        }

        var loadingNextScene = SceneManager.LoadSceneAsync(nextIndex);
        while (!loadingNextScene.isDone)
        {
            print("loading scene");
            yield return null;
        }

    }

    IEnumerator LoadLevelByBuildIndex(int buildIndex)
    {
        print(name);
        playerVFX.BeginFadeScreen();
        yield return null;

        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        while (!playerVFX.isFaded)
        {
            yield return null;
        }

        var loadingNextScene = SceneManager.LoadSceneAsync(buildIndex);
        while (!loadingNextScene.isDone)
        {
            print("loading scene");
            yield return null;
        }
    }

}
