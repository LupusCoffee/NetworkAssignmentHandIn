using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Player;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(1)]
public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    [Header("Stats")]
    [SerializeField] private List<Color> playerColors;
    [SerializeField] private int maxPlayers = 4;

    [Header("Reference")]
    [SerializeField] private List<GameObject> players;
    [SerializeField] private GameObject BulletSpawner;
    [SerializeField] private GameObject StartAreas;

    GameObject bulletSpawnerInst;
    private int occupiedStartAreas;
    private int livingPlayers;
    private int playerCountLoseCondition;
    private bool isMatchOngoing;

    //shortcuts
    NetworkManager NETWORK_MANAGER;
    IReadOnlyList<NetworkClient> PLAYERS;


    private void Awake()
    {
        instance = this;
    }


    public override void OnNetworkSpawn()
    {
        NETWORK_MANAGER = NetworkManager.Singleton;
        PLAYERS = NETWORK_MANAGER.ConnectedClientsList;

        if (!IsServer) return;

        NETWORK_MANAGER.OnClientConnectedCallback += PlayerJoined;
        NETWORK_MANAGER.OnClientDisconnectCallback += PlayerLeft;
        StartArea.onStartAreaOccupied += PlayerEnteredStartArea;
        StartArea.onStartAreaNotOccupied += PlayerExitedStartArea;
        Player.onPlayerDeath += PlayerDeath;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        NETWORK_MANAGER.OnClientConnectedCallback -= PlayerJoined;
        NETWORK_MANAGER.OnClientDisconnectCallback -= PlayerLeft;
        StartArea.onStartAreaOccupied -= PlayerEnteredStartArea;
        StartArea.onStartAreaNotOccupied -= PlayerExitedStartArea;
        Player.onPlayerDeath -= PlayerDeath;
    }

    private void PlayerJoined(ulong id)
    {
        print(id + " joined.");

        if (PLAYERS.Count > maxPlayers || isMatchOngoing)
        {
            //somehow kick player (hmmm)
            return;
        }

        GameObject playerObj = PLAYERS[0].PlayerObject.gameObject;
        for (int i = 0; i < PLAYERS.Count; i++)
        {
            ulong idCheck = PLAYERS[i].ClientId;
            playerObj = PLAYERS[i].PlayerObject.gameObject;
            ChangePlayerNameClientRpc(playerObj.GetComponent<Player>(), idCheck);
            ChangePlayerColorClientRpc(playerObj.GetComponent<Player>(), idCheck);
        }
    }
    [Rpc(SendTo.ClientsAndHost)] //not important against cheating, thus no proper server authority (also no time and lower priority)
    private void ChangePlayerNameClientRpc(NetworkBehaviourReference playerReference, ulong playerId)
    {
        if (playerReference.TryGet<Player>(out Player player))
            player.gameObject.name = "Player " + playerId;
        else print("didnt get player");
    }
    [Rpc(SendTo.ClientsAndHost)] //not important against cheating, thus no proper server authority (also no time and lower priority)
    private void ChangePlayerColorClientRpc(NetworkBehaviourReference playerReference, ulong idCheck)
    {
        if (!playerReference.TryGet<Player>(out Player player))
        {
            Debug.LogError("PLAYER DIDNT GET COLOR LOL");
            return;
        }
        Player clientPlayer = player.GetComponent<Player>();

        int randomPlayerColorIndex = Random.Range(0, playerColors.Count);
        bool colorFound = false;
        while (!colorFound)
        {
            randomPlayerColorIndex = Random.Range(0, playerColors.Count);
            Color myClientColor = playerColors[randomPlayerColorIndex];
            colorFound = true;

            for (int i = 0; i < PLAYERS.Count; i++)
            {
                Color otherClientColor = PLAYERS[i].PlayerObject.gameObject.GetComponent<Player>().GetColor();
                if (myClientColor == otherClientColor) colorFound = false;
            }
        }
        clientPlayer.SetColor(playerColors[randomPlayerColorIndex]);
    }
    private void PlayerLeft(ulong id)
    {
        print(id + " left.");

        GameObject playerObj = PLAYERS[0].PlayerObject.gameObject;
        for (int i = 0; i < PLAYERS.Count; i++)
        {
            if (PLAYERS[i].ClientId == id)
                playerObj = PLAYERS[i].PlayerObject.gameObject;
        }
    }

    private void PlayerEnteredStartArea()
    {
        occupiedStartAreas++;
        if(occupiedStartAreas == PLAYERS.Count) StartGameClientRpc();
    }
    private void PlayerExitedStartArea()
    {
        occupiedStartAreas--;
        if (occupiedStartAreas == PLAYERS.Count) StartGameClientRpc();
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void StartGameClientRpc()
    {
        StartAreas.SetActive(false);

        if (!IsServer) return;

        livingPlayers = PLAYERS.Count;

        if(livingPlayers > 1) playerCountLoseCondition = 1;
        else playerCountLoseCondition = 0;

        bulletSpawnerInst = Instantiate(BulletSpawner, new Vector3(0,0,0), Quaternion.identity);
    }
    private void PlayerDeath()
    {
        livingPlayers--;
        if(livingPlayers <= playerCountLoseCondition) EndGameClientRpc();
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void EndGameClientRpc()
    {
        StartAreas.SetActive(true);

        for (int i = 0; i < PLAYERS.Count; i++)
            PLAYERS[i].PlayerObject.gameObject.GetComponent<Player>().Spawn(); //this should have been taken care of via a network variable for proper server authority - sadly, no time

        //NO WORKIE :((((
        //for (int i = 0; i < bulletSpawnerInst.GetComponent<ServerBulletSpawner>().currentBullets.Count; i++)
        //{
        //    bulletSpawnerInst.GetComponent<ServerBulletSpawner>().currentBullets.RemoveAt(i);
       //     Destroy(bulletSpawnerInst.GetComponent<ServerBulletSpawner>().currentBullets[i]);
       // }

        if (!IsServer) return;

        Destroy(bulletSpawnerInst);
    }
}
