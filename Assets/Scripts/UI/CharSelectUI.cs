using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharSelectUI : MonoBehaviour
{
    public Button mainMenuButton;
    public Button readyButton;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            SceneChangeManager.instance.Load(SceneChangeManager.Scene.MainMenu);
        });

        readyButton.onClick.AddListener(() =>
        {
            CharacterSelectReady.instance.SetPlayerReady();
        });
    }
}
