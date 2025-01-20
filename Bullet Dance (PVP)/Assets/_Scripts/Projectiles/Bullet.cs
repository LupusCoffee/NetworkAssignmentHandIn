using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float baseSpeed;
    [SerializeField] private float speedIncrease;

    private float currentSpeed;

    Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        currentSpeed = baseSpeed;
    }

    public void Deflect(Vector2 direction)
    {
        body.linearVelocity = direction * currentSpeed;
        currentSpeed += speedIncrease;
    }
}
