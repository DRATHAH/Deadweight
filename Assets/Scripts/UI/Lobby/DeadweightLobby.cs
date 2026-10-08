using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class DeadweightLobby : MonoBehaviour
{
    #region Singleton

    public static DeadweightLobby instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("More than one instance of Deadweight Lobby found!");
            return;
        }

        instance = this;
        DontDestroyOnLoad(instance);
    }

    #endregion

    Lobby joinedLobby;

    async void InitializeAuthethication()
    {
        if(UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions options = new InitializationOptions();
            options.SetProfile(Random.Range(0f,10000f).ToString());
            await UnityServices.InitializeAsync(options);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    async void CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, DeadweightNetworkManager.instance.maxPlayers, new CreateLobbyOptions
            {
                IsPrivate = isPrivate
            });

            DeadweightNetworkManager.instance.StartHost();
            SceneChangeManager.instance.LoadScene(SceneChangeManager.Scene.GameSelectScene);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    async public void QuickJoin()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            DeadweightNetworkManager.instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
