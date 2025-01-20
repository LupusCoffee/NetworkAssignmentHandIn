using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int damageAmount = 1;

    [Header("References")]
    [SerializeField] private SpriteRenderer edgeColorRenderer;
    [SerializeField] private SpriteRenderer edgeColorRenderer2;

    private int health;
    private Color playerColor = Color.white;
    private InputActions actions;

    public delegate void PlayerDeath();
    public static event PlayerDeath onPlayerDeath;


    private void Awake()
    {
        Spawn();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Bullet"))
        {
            TakeDamageClientRpc(damageAmount);
            DestroyBulletServerRpc(collision.gameObject.GetComponent<Bullet>());
        }
    }
    [Rpc(SendTo.Server)]
    private void DestroyBulletServerRpc(NetworkBehaviourReference bulletReference)
    {
        if (!bulletReference.TryGet<Bullet>(out Bullet bullet)) return;
        bullet.GetComponent<NetworkObject>().Despawn();
    }

    public void Spawn()
    {
        gameObject.SetActive(true);
        health = maxHealth;
    }

    [Rpc(SendTo.ClientsAndHost)] //Should be server side!! i.e. send to server
    private void TakeDamageClientRpc(int damage)
    {
        health -= damage;
        if (health <= 0) DeactivatePlayer();
    }
    private void DeactivatePlayer()
    {
        print(gameObject.name + " died!");
        gameObject.SetActive(false);
        onPlayerDeath?.Invoke();
    }

    public void SetColor(Color newVal)
    {
        playerColor = newVal;
        edgeColorRenderer.color = playerColor;
        edgeColorRenderer2.color = playerColor;
    }
    public Color GetColor()
    {
        return playerColor;
    }
}
