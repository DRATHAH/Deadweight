using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyTemplateUI : MonoBehaviour
{
    public TMP_Text lobbyName;
    Lobby lobby;
    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyName.text = lobby.Name;

        GetComponent<Button>().onClick.AddListener(() =>
        {
            DeadweightLobby.instance.JoinWithId(lobby.Id);
        });
    }
}
