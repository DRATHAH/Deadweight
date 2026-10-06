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
        ResultsScene
    }

    Scene targetScene;

    public void LoadScene(Scene scene)
    {
        targetScene = scene;
        StartCoroutine(LoadSceneOperation());
    }

    public void LoadScene(string scene)
    {
        if (Enum.TryParse<Scene>(scene, out Scene sceneEnum))
        {
            targetScene = sceneEnum;
            StartCoroutine(LoadSceneOperation());
        }
        else
        {
            Debug.LogWarning("Scene name not found");
        }
    }

    IEnumerator LoadSceneOperation()
    {
        // Start transition animation

        // Wait for transition to finish
        yield return new WaitForSeconds(.5f);

        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single); 
    }
}
