using TMPro;
using UnityEngine;

public class CharSelectPlayer : MonoBehaviour
{
    public int playerIndex;
    public GameObject readyText;

    private void Start()
    {
        DeadweightNetworkManager.instance.OnPlayerDataNetworkListChanged += DeadweightNetworkManager_OnPlayerDataNetworkListChanged;
        CharacterSelectReady.instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;
        UpdatePlayer();
    }

    private void CharacterSelectReady_OnReadyChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void DeadweightNetworkManager_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    void UpdatePlayer()
    {
        if (DeadweightNetworkManager.instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();
            PlayerData data = DeadweightNetworkManager.instance.GetPlayerDataFromPlayerIndex(playerIndex);
            readyText.SetActive(CharacterSelectReady.instance.IsPlayerReady(data.clientId));
        }
        else
        {
            Hide();
        }
    }

    void Show()
    {
        gameObject.SetActive(true);
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        DeadweightNetworkManager.instance.OnPlayerDataNetworkListChanged -= DeadweightNetworkManager_OnPlayerDataNetworkListChanged;
        CharacterSelectReady.instance.OnReadyChanged -= CharacterSelectReady_OnReadyChanged;
    }
}
