using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Unity.Netcode;
using System;

public class SceneChangeManager : MonoBehaviour
{
    #region Singleton

    public static SceneChangeManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of Scene Change Manager found!");
        }
        instance = this;
    }

    #endregion

    public enum Scene
    {
        LobbyScene,
        BattleScene,
        GameSelectScene,
        ResultsScene,
        MainMenu,
        LevelOnePoC
    }

    Scene targetScene;

    public void LoadScene(Scene scene)
    {
        targetScene = scene;
        StartCoroutine(LoadSceneOperation(true));
    }

    public void LoadScene(string scene)
    {
        if (Enum.TryParse<Scene>(scene, out Scene sceneEnum))
        {
            targetScene = sceneEnum;
            StartCoroutine(LoadSceneOperation(true));
        }
        else
        {
            Debug.LogWarning("Scene name not found");
        }
    }

    public void Load(Scene scene)
    {
        targetScene = scene;
        StartCoroutine(LoadSceneOperation(false));
    }

    IEnumerator LoadSceneOperation(bool multiplayer)
    {
        // Start transition animation

        // Wait for transition to finish
        yield return new WaitForSeconds(1f);
        if (multiplayer && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
        }
        else if (NetworkManager.Singleton.SceneManager != null)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene.ToString(), LoadSceneMode.Single);
            while (!operation.isDone)
            {
                // Progress bar here
                yield return null;
            }
        }
    }
}
