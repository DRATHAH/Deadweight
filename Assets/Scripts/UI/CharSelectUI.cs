using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class CharSelectUI : MonoBehaviour
{
    public Button mainMenuButton;
    public Button readyButton;
    public TMP_Text lobbyName;
    public TMP_Text lobbyCode;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            DeadweightLobby.instance.LeaveLobby();
            SceneChangeManager.instance.Load(SceneChangeManager.Scene.MainMenu);
        });

        readyButton.onClick.AddListener(() =>
        {
            CharacterSelectReady.instance.SetPlayerReady();
        });
    }

    private void Start()
    {
        Lobby lobby = DeadweightLobby.instance.GetLobby();

        lobbyName.text = "Lobby Name: " + lobby.Name;
        lobbyCode.text = "Lobby Code: " + lobby.LobbyCode;
    }
}
