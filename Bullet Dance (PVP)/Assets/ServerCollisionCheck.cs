using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ServerCollisionCheck : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField] private int damageAmount = 1;

    private Color playerColor = Color.white;
    private InputActions actions;

    public delegate void PlayerDeath();
    public static event PlayerDeath onPlayerDeath;

    private Player player;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;

        player = GetComponent<Player>();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (collision.transform.CompareTag("Bullet"))
        {
            TakeDamage(damageAmount);
            collision.gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
    /*private void DestroyBullet(NetworkBehaviourReference bulletReference)
    {
        if (!bulletReference.TryGet<Bullet>(out Bullet bullet)) return;
        bullet.GetComponent<NetworkObject>().Despawn();
    }*/

    private void TakeDamage(int damage)
    {
        player.SetHealth(player.GetHealth() - damage);
        Debug.Log("Health: " + player.GetHealth());
        if (player.GetHealth() <= 0) DeactivatePlayerClientRpc();
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void DeactivatePlayerClientRpc()
    {
        print(gameObject.name + " died!");
        gameObject.SetActive(false);
        onPlayerDeath?.Invoke();
    }
}
