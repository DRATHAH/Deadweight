using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public Button createGameButton;
    public Button joinGameButton;

    private void Awake()
    {
        createGameButton.onClick.AddListener(() =>
        {
            DeadweightNetworkManager.instance.StartHost();
            SceneChangeManager.instance.LoadScene(SceneChangeManager.Scene.GameSelectScene);
        });

        joinGameButton.onClick.AddListener(() =>
        {
            DeadweightNetworkManager.instance.StartClient();
        });
    }
}
