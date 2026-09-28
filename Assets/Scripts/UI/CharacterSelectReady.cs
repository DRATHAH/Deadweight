using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady instance {  get; private set; }

    Dictionary<ulong, bool> playersReady;

    private void Awake()
    {
        instance = this;
        playersReady = new Dictionary<ulong, bool>();
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [Rpc(SendTo.Server,InvokePermission = RpcInvokePermission.Everyone)]
    void SetPlayerReadyServerRpc(RpcParams rpcParams = default)
    {
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
            SceneChangeManager.instance.LoadScene(SceneChangeManager.Scene.BattleScene);
        }
    }
}
