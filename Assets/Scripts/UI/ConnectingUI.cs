using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectingUI : MonoBehaviour
{
    public GameObject connectingScreen;
    public TMP_Text msgText;
    public GameObject failScreen;

    void Start()
    {
        DeadweightNetworkManager.instance.OnFailedToJoinGame += DeadweightNetworkManager_OnFailedToJoinGame;
        Hide(connectingScreen);
        Hide(failScreen);
    }

    private void DeadweightNetworkManager_OnFailedToJoinGame(object sender, EventArgs e)
    {
        Hide(connectingScreen);
        Show(failScreen);
        msgText.text = NetworkManager.Singleton.DisconnectReason;

        if (msgText.text.Equals(""))
        {
            msgText.text = "Failed to connect";
        }
    }

    public void Show(GameObject obj)
    {
        obj.SetActive(true);
    }

    public void Hide(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroy()
    {
        DeadweightNetworkManager.instance.OnFailedToJoinGame -= DeadweightNetworkManager_OnFailedToJoinGame;
    }
}
