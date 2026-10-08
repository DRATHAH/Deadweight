using Unity.Netcode;
using UnityEngine;

public class ManagerCleanup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }

        if (DeadweightNetworkManager.instance != null)
        {
            Destroy(DeadweightNetworkManager.instance.gameObject);
        }

        if (DeadweightLobby.instance != null)
        {
            Destroy (DeadweightLobby.instance.gameObject);
        }
    }
}
