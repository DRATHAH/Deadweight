using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public Button mainMenuButton;
    public Button createLobbyButton;
    public Button joinGameButton;
    public LobbyCreateUI createLobbyUI;
    public Button joinCodeButton;
    public TMP_InputField jointCodeField;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            DeadweightLobby.instance.LeaveLobby();
            SceneChangeManager.instance.Load(SceneChangeManager.Scene.MainMenu);
        });

        createLobbyButton.onClick.AddListener(() =>
        {
            createLobbyUI.Show();
        });

        joinGameButton.onClick.AddListener(() =>
        {
            DeadweightLobby.instance.QuickJoin();
        });

        joinCodeButton.onClick.AddListener(() =>
        {
            DeadweightLobby.instance.JoinWithCode(jointCodeField.text);
        });
    }
}
