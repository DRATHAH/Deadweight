using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Unity.Netcode;

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
        GameSelectScene
    }

    Scene targetScene;

    public void LoadScene(Scene scene)
    {
        targetScene = scene;
        StartCoroutine(LoadSceneOperation());
    }

    IEnumerator LoadSceneOperation()
    {
        // Start transition animation

        // Wait for transition to finish
        yield return new WaitForSeconds(.5f);

        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single); 
    }
}
