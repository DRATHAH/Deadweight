using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HostDisconnectUI : MonoBehaviour
{
    public Button returnButton;

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += ShowMenu;
        Hide();

        returnButton.onClick.AddListener(() =>
        {
            SceneChangeManager.instance.Load(SceneChangeManager.Scene.MainMenu);
        });
    }

    private void ShowMenu(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Show();
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
}
