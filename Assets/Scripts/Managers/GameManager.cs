using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
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

    [Header("Camera")]
    public Camera mainCam;
    [Tooltip("How quickly the camera adjusts its view")]
    public float cameraSmooth = 0.01f;
    [Tooltip("Clear space between players and edge of screen")]
    public float padding = 2f;
    [Tooltip("The most the camera can zoom")]
    public float minDistance = 10f;

    [Header("Player and Game States")]
    public Transform playerPrefab;
    public GameObject chain;
    public int chainAmount = 2;
    public TMP_Text timerText;
    public float time = 60;
    public List<Transform> spawns;
    public GameObject CPUPrefab;
    public List<EnemyAI> CPUs;
    public List<GameObject> gamePlayers = new List<GameObject>();

    bool gameStarted = false;
    ConnectConfigJoints prevChain;
    bool gameOver = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        for (int i = 0; i < spawns.Count; i++)
        {
            if (i < NetworkManager.Singleton.ConnectedClientsIds.Count)
            {
                Transform playerTransform = Instantiate(playerPrefab, spawns[i].position, spawns[i].rotation);
                playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsIds[i], true);
            }
            else if (DeadweightNetworkManager.instance.GetCpuState())
            {
                GameObject enemy = Instantiate(CPUPrefab, spawns[i].position, spawns[i].rotation);
                NetworkObject enemyObj = enemy.GetComponent<NetworkObject>();
                enemyObj.Spawn(true);
                CPUs.Add(enemy.GetComponent<EnemyAI>());
                gamePlayers.Add(enemy);
            }
        }

        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            gamePlayers.Add(NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject);
        }
        foreach(GameObject player in gamePlayers)
        {
            UpdatePlayerListClientRpc(player.GetComponent<NetworkObject>().NetworkObjectId);
        }

        for (int i = 0; i < gamePlayers.Count; i++)
        {
            if (i + 1 < gamePlayers.Count)
            {
                gamePlayers[i].GetComponent<ChainLink>().InitializeChain(gamePlayers[i + 1].transform);
            }
            else if (gamePlayers.Count > 1)
            {
                gamePlayers[i].GetComponent<ChainLink>().InitializeChain(gamePlayers[0].transform);
            }
        }

        if (gamePlayers.Count > 1)
        {
            for (int i = 0; i < gamePlayers.Count; i++)
            {
                Transform playerA = gamePlayers[i].transform.GetComponent<ChainLink>().anchors[1].transform;
                Transform playerB;

                if (i + 1 >= gamePlayers.Count)
                {
                    playerB = gamePlayers[0].transform.GetComponent<ChainLink>().anchors[0].transform;
                }
                else
                {
                    playerB = gamePlayers[i + 1].transform.GetComponent<ChainLink>().anchors[0].transform;
                }
                float distBetweenSpawns = (playerB.position - playerA.position).magnitude;
                float chainNum = (distBetweenSpawns / 1.8f) + .5f;
                UpdateChainClientRpc(distBetweenSpawns, i);
                for (int c = 0; c < (int)chainNum; c++)
                {
                    Vector3 direction = (playerB.transform.position - playerA.transform.position).normalized;
                    Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90f, 0f, 0f);
                    Vector3 spawnPos = Vector3.zero;
                    if (prevChain == null)
                    {
                        spawnPos = playerA.position;
                    }
                    else
                    {
                        spawnPos = prevChain.chainEnd.position;
                    }
                    
                    GameObject newChain = Instantiate(chain, spawnPos, rotation);
                    playerA.parent.GetComponent<ChainLink>().connectedChains.Add(newChain);
                    playerB.parent.GetComponent<ChainLink>().connectedChains.Add(newChain);
                    ConnectConfigJoints chainJoints = newChain.GetComponent<ConnectConfigJoints>();

                    if (prevChain == null)
                    {
                        chainJoints.InitializeChain(playerA.GetComponent<Rigidbody>());
                    }
                    else
                    {
                        chainJoints.InitializeChain(prevChain.chainEnd);
                    }

                    prevChain = chainJoints;
                    newChain.GetComponent<NetworkObject>().Spawn();
                }
                playerB.GetComponent<ConfigurableJoint>().connectedBody = prevChain.chainEnd;
                prevChain = null;
            }
        }

        OnSpawnCPUs?.Invoke(this, EventArgs.Empty);
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
            if (NetworkManager.Singleton && !gameOver)
            {
                UpdateTimerClientRpc(time);
                UpdateCamera();
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateTimerClientRpc(float t)
    {
        if (t <= 0)
        {
            gameOver = true;
            EndGame();
        }
        // Format string in mm:ss format
        TimeSpan timeFormat = TimeSpan.FromSeconds(t);
        timerText.text = timeFormat.ToString(@"m\:ss");
    }

    void UpdateCamera()
    {
        Vector3 camTarget = Vector3.zero;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        int playersLeft = 0;
        float averageDepth = 0;

        foreach (GameObject player in gamePlayers)
        {
            if (player && player.GetComponent<DamageableCharacter>().Targetable)
            {
                playersLeft++;

                // Converts player position to screen space
                Vector3 localPos = mainCam.transform.InverseTransformPoint(player.transform.position);
                // Sets maximum/minimum X and Y coordinates needed to keep all players on screen
                minX = Mathf.Min(minX, localPos.x);
                maxX = Mathf.Max(maxX, localPos.x);
                minY = Mathf.Min(minY, localPos.y);
                maxY = Mathf.Max(maxY, localPos.y);
                minZ = Mathf.Min(minZ, localPos.z);
                maxZ = Mathf.Max(maxZ, localPos.z);

                camTarget += player.transform.position;
            }
        }
        playersLeft = Mathf.Clamp(playersLeft, 1, gamePlayers.Count);

        // Center between players
        averageDepth /= playersLeft;
        float centerX = (minX + maxX) * .5f;
        float centerY = (minY + maxY) * .5f;
        float centerZ = (minZ + maxZ) * .5f;
        Vector3 worldCenter = mainCam.transform.TransformPoint(new Vector3(centerX, centerY, centerZ));

        float halfWidth = (maxX - minX) * .5f;
        float halfHeight = (maxY - minY) * .5f;
        float halfDepth = (maxZ - minZ) * .5f;

        // Calculate horizontal FOV from vertical FOV + aspect ratio
        float verticalFov = mainCam.fieldOfView * Mathf.Deg2Rad;
        float horizontalFov = 2f * Mathf.Atan(Mathf.Tan(verticalFov / 2f) * mainCam.aspect);
        float verticalTan = Mathf.Tan(verticalFov / 2f);
        float horizontalTan = Mathf.Tan(horizontalFov / 2f);

        // Distance needed to fit width and height
        float requiredDistance = 0;
        float[] xValues = { -halfWidth, halfWidth };
        float[] yValues = { -halfHeight, halfHeight };
        float[] zValues = { -halfDepth, halfDepth };

        foreach (float x in xValues)
        {
            foreach (float y in yValues)
            {
                foreach (float z in zValues)
                {
                    float horizontalDistance = Mathf.Abs(x) / horizontalTan - z;
                    float verticalDistance = Mathf.Abs(y) / verticalTan - z;
                    requiredDistance = Mathf.Max(requiredDistance, horizontalDistance, verticalDistance);
                }
            }
        }
        requiredDistance = Mathf.Max(requiredDistance + padding, minDistance);

        // Zoom camera out if needed
        Vector3 desiredPos = worldCenter + (-mainCam.transform.forward * requiredDistance);

        UpdateCameraClientRpc(desiredPos);
    }

    public void EndGame()
    {
        SceneChangeManager.instance.LoadScene(SceneChangeManager.Scene.ResultsScene);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateCameraClientRpc(Vector3 camTarget)
    {
        mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, camTarget, cameraSmooth);
    }

    [Rpc(SendTo.NotServer)]
    void UpdatePlayerListClientRpc(ulong playerId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject networkObject))
        {
            gamePlayers.Add(networkObject.gameObject);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateChainClientRpc(float chainLength, int playerId)
    {
        gamePlayers[playerId].transform.GetComponent<ChainLink>().SetDistance(chainAmount);
    }
}
