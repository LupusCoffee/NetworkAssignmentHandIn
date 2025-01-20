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
        health = maxHealth;
    }

    /*private void Awake()
    {
        Spawn();
    }

    public void Spawn()
    {
        gameObject.SetActive(true);
        health = maxHealth;

        print("spawn!");
    }*/

    public void SetHealth(int value)
    {
        health = value;
    }
    public int GetHealth() { return health; }

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
