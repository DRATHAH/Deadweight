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
    public TMP_Text timerText;
    public float time = 60;
    public List<Transform> spawns;
    public GameObject CPUPrefab;
    public List<EnemyAI> CPUs;
    public List<GameObject> gamePlayers = new List<GameObject>();

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

        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            gamePlayers.Add(NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject);
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
            if (NetworkManager.Singleton)
            {
                UpdateTimerClientRpc(time);
                UpdateCamera();
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
        gamePlayers.Add(enemy);

        OnSpawnCPUs?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateTimerClientRpc(float t)
    {
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

        foreach(float x in xValues)
        {
            foreach(float y in yValues)
            {
                foreach(float z in zValues)
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

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateCameraClientRpc(Vector3 camTarget)
    {
        mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, camTarget, cameraSmooth);
    }
}
