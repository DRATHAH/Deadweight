using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    #region Singleton

    public static GameManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of Game Manager found!");
            return;
        }
        instance = this;
    }

    #endregion

    public event EventHandler OnSpawnCPUs;

    public Transform playerPrefab; 
    public TMP_Text timerText;
    public float time = 60;
    public List<Transform> spawns;
    public GameObject CPUPrefab;
    public List<EnemyAI> CPUs;

    bool gameStarted = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        for(int i = 0; i < spawns.Count; i++)
        {
            if (i < NetworkManager.Singleton.ConnectedClientsIds.Count)
            {
                Transform playerTransform = Instantiate(playerPrefab, spawns[i].position, Quaternion.identity);
                playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsIds[i], true);
            }
            else if (DeadweightNetworkManager.instance.GetCpuState())
            {
                SpawnCPUServerRpc(i);
            }
        }

        gameStarted = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (gameStarted)
        {
            time -= Time.deltaTime;
            // Format string in mm:ss format
            if (NetworkManager.Singleton)
            {
                UpdateTimerClientRpc(time);
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Server)]
    void SpawnCPUServerRpc(int spawnId)
    {
        GameObject enemy = Instantiate(CPUPrefab, spawns[spawnId].position, Quaternion.identity);
        NetworkObject enemyObj = enemy.GetComponent<NetworkObject>();
        enemyObj.Spawn(true);
        CPUs.Add(enemy.GetComponent<EnemyAI>());

        OnSpawnCPUs?.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    void UpdateTimerClientRpc(float t)
    {
        TimeSpan timeFormat = TimeSpan.FromSeconds(t);
        timerText.text = timeFormat.ToString(@"m\:ss");
    }
}
