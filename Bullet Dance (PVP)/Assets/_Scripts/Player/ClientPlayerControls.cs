using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientPlayerControls : NetworkBehaviour
{
    PlayerInput playerInput;
    PlayerBatAiming playerBatAiming;
    Player player;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerBatAiming = GetComponent<PlayerBatAiming>();
        player = GetComponent<Player>();

        if (playerInput == null || player == null) return;
        playerInput.enabled = false;
        playerBatAiming.enabled = false;
        player.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enabled = IsClient; //enabled if client;
        
        if(!IsOwner)
        {
            enabled = false;
            playerInput.enabled = false;
            playerBatAiming.enabled = false;
            player.enabled = false;
            return;
        }

        playerInput.enabled = true;
        playerBatAiming.enabled = true;
        player.enabled = true;
    }
}
