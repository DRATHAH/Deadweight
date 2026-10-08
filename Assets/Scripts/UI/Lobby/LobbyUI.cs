using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public Button mainMenuButton;
    public Button createLobbyButton;
    public Button joinGameButton;
    public LobbyCreateUI createLobbyUI;
    public Button joinCodeButton;
    public TMP_InputField jointCodeField;
    public Transform lobbyContainer;
    public GameObject lobbyTemplate;

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

    private void Start()
    {
        DeadweightLobby.instance.OnLobbyListChanged += DeadweightLobby_OnLobbyListChanged;
    }

    private void DeadweightLobby_OnLobbyListChanged(object sender, DeadweightLobby.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach(Transform child in lobbyContainer)
        {
            Destroy(child.gameObject);
        }

        foreach(Lobby lobby in lobbyList)
        {
            GameObject lobbyTemp = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTemp.GetComponent<LobbyTemplateUI>().SetLobby(lobby);
        }
    }

    private void OnDestroy()
    {
        DeadweightLobby.instance.OnLobbyListChanged -= DeadweightLobby_OnLobbyListChanged;
    }
}
