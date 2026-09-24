using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public TMP_Text timerText;
    public float time = 60;

    bool gameStarted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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

    [ClientRpc]
    void UpdateTimerClientRpc(float t)
    {
        TimeSpan timeFormat = TimeSpan.FromSeconds(t);
        timerText.text = timeFormat.ToString(@"m\:ss");
    }

    public void StartGame()
    {
        gameStarted = true;
    }
}
