using Unity.Netcode;
using UnityEngine;

public class JoinServer : MonoBehaviour
{
    [SerializeField] private GameObject menu;

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        menu.SetActive(false);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        menu.SetActive(false);
    }
}
