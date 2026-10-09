using System;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady instance {  get; private set; }
    public Button cpuEnableButton;
    public Button cpuDisableButton;

    public event EventHandler OnReadyChanged;
    Dictionary<ulong, bool> playersReady;

    private void Awake()
    {
        instance = this;
        playersReady = new Dictionary<ulong, bool>();

        cpuEnableButton.onClick.AddListener(() =>
        {
            DeadweightNetworkManager.instance.EnableCpus(true);
        });
        cpuDisableButton.onClick.AddListener(() =>
        {
            DeadweightNetworkManager.instance.EnableCpus(false);
        });

        cpuEnableButton.gameObject.SetActive(false);
        cpuDisableButton.gameObject.SetActive(false);
    }

    void Start()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += SetLobbyButtons;
        StartCoroutine(CpuButton());
    }

    private void SetLobbyButtons(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        SetLobbyButtonsServerRpc();
    }

    IEnumerator CpuButton()
    {
        yield return new WaitForSeconds(1);
        SetLobbyButtonsServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SetLobbyButtonsServerRpc()
    {
        if (NetworkManager.Singleton != null && IsHost)
        {
            cpuEnableButton.gameObject.SetActive(true);
        }
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [Rpc(SendTo.Server,InvokePermission = RpcInvokePermission.Everyone)]
    void SetPlayerReadyServerRpc(RpcParams rpcParams = default)
    {
        SetPlayerReadyClientRpc(rpcParams.Receive.SenderClientId);
        playersReady[rpcParams.Receive.SenderClientId] = true;
        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playersReady.ContainsKey(clientId) || !playersReady[clientId])
            {
                // This player is not ready
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            DeadweightLobby.instance.DeleteLobby();
            SceneChangeManager.instance.LoadScene(SceneChangeManager.Scene.LevelOnePoC);
        }
    }

    [Rpc(SendTo.ClientsAndHost,InvokePermission =RpcInvokePermission.Server)]
    void SetPlayerReadyClientRpc(ulong clientId)
    {
        playersReady[clientId] = true;
        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playersReady.ContainsKey(clientId) && playersReady[clientId];
    }

    public override void OnDestroy()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= SetLobbyButtons;
    }
}
