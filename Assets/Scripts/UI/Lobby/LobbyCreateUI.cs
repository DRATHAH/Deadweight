using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    public Button closeButton;
    public Button createPublic;
    public Button createPrivate;
    public TMP_InputField lobbyNameField;

    private void Awake()
    {
        createPublic.onClick.AddListener(() =>
        {
            DeadweightLobby.instance.CreateLobby(lobbyNameField.text, false);
            SceneChangeManager.instance.LoadScene(SceneChangeManager.Scene.GameSelectScene);
        });
        createPrivate.onClick.AddListener(() =>
        {
            DeadweightLobby.instance.CreateLobby(lobbyNameField.text, true);
            SceneChangeManager.instance.LoadScene(SceneChangeManager.Scene.GameSelectScene);
        });
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }
}
