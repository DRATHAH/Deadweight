using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyMessageUI : MonoBehaviour
{
    public TMP_Text messageText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DeadweightNetworkManager.instance.OnFailedToJoinGame += DeadweightNetworkManager_OnFailedToJoinGame;
        DeadweightLobby.instance.OnCreateLobbyStarted += DeadweightLobby_OnCreateLobbyStarted;
        DeadweightLobby.instance.OnCreateLobbyFailed += DeadweightLobby_OnCreateLobbyFailed;
        DeadweightLobby.instance.OnJoinStarted += DeadweightLobby_OnJoinStarted;
        DeadweightLobby.instance.OnJoinFailed += DeadweightLobby_OnJoinFailed;
        DeadweightLobby.instance.OnCodeJoinFailed += DeadweightLobby_OnCodeJoinFailed;

        gameObject.SetActive(false);
    }

    private void DeadweightLobby_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Creating lobby...");
    }

    private void DeadweightLobby_OnCreateLobbyFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to create lobby!");
    }

    private void DeadweightLobby_OnJoinStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Joining lobby...");
    }

    private void DeadweightLobby_OnJoinFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Could not find a lobby to quick join!");
    }

    private void DeadweightLobby_OnCodeJoinFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to join lobby!");
    }

    void ShowMessage(string msg)
    {
        gameObject.SetActive(true);
        messageText.text = msg;
    }

    private void DeadweightNetworkManager_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        gameObject.SetActive(true);
        if (NetworkManager.Singleton.DisconnectReason.Equals(""))
        {
            ShowMessage("Failed to connect");
        }
        else
        {
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
        }
    }

    private void OnDestroy()
    {
        DeadweightNetworkManager.instance.OnFailedToJoinGame -= DeadweightNetworkManager_OnFailedToJoinGame;
        DeadweightLobby.instance.OnCreateLobbyStarted -= DeadweightLobby_OnCreateLobbyStarted;
        DeadweightLobby.instance.OnCreateLobbyFailed -= DeadweightLobby_OnCreateLobbyFailed;
        DeadweightLobby.instance.OnJoinStarted -= DeadweightLobby_OnJoinStarted;
        DeadweightLobby.instance.OnJoinFailed -= DeadweightLobby_OnJoinFailed;
        DeadweightLobby.instance.OnCodeJoinFailed -= DeadweightLobby_OnCodeJoinFailed;
    }
}
