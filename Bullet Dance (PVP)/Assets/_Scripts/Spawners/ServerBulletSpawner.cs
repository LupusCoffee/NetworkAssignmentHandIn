using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class ServerBulletSpawner : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] private Vector2 spawnArea;
    [SerializeField] private float spawnFrequency = 10;

    [Header("References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private LayerMask unspawnableMask;

    public List<GameObject> currentBullets;

    //Private shit
    private float time = 0;
    float radius;
    RaycastHit2D hit;

    public override void OnNetworkSpawn()
    {
        if(!IsServer)
        {
            enabled = false;
            return;
        }
        base.OnNetworkSpawn();
    }


    private void Start()
    {
        float playerRadius = playerPrefab.GetComponent<CircleCollider2D>().radius;
        float bulletRadius = bulletPrefab.GetComponent<CircleCollider2D>().radius;
        if (playerRadius > bulletRadius) radius = playerRadius;
        else radius = bulletRadius;
    }

    private void Update()
    {
        if (time > 0) time -= Time.deltaTime;
        else
        {
            Spawn(bulletPrefab);
            time = spawnFrequency;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, (Vector3)spawnArea);
    }


    private void Spawn(GameObject prefab)
    {
        //this spawnPosition somehow needs to be synced across the network
        Vector2 spawnPosition = new Vector2(Random.Range(-spawnArea.x / 2, spawnArea.x / 2), Random.Range(-spawnArea.y / 2, spawnArea.y / 2));

        hit = Physics2D.CircleCast(spawnPosition, radius, transform.right, 0, unspawnableMask);
        if (hit) return;

        GameObject bullet = Instantiate(prefab, spawnPosition, Quaternion.identity);
        currentBullets.Add(bullet);
        NetworkObject bulletNetworkObj = bullet.GetComponent<NetworkObject>();
        bulletNetworkObj.Spawn();
    }
}