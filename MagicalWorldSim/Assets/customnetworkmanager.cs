using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;

public class customnetworkmanager : NetworkManager
{
    [SerializeField] private PlayerObjectController GamePlayerPrefab;
    public List<PlayerObjectController> GamePlayers { get; } = new List<PlayerObjectController>();

    // Example usage in customnetworkmanager
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("NetworkServer is not active! Player cannot be added.");
            return;
        }

        PlayerObjectController gamePlayerInstance = Instantiate(GamePlayerPrefab);
        int connId = conn.connectionId;
        int playerIdNumber = GamePlayers.Count + 1;
        ulong steamId = 0;

        int playerIndex = GamePlayers.Count;
        if (SteamLobby.instance != null && SteamLobby.instance.CurrentLobbyID != 0)
        {
            steamId = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.instance.CurrentLobbyID, playerIndex);
        }
        else
        {
            Debug.LogError("Steam Lobby is not initialized properly.");
        }

        gamePlayerInstance.SetPlayerInfo(connId, playerIdNumber, steamId);
        NetworkServer.AddPlayerForConnection(conn, gamePlayerInstance.gameObject);
        GamePlayers.Add(gamePlayerInstance);

        // Call the RPC from the player instance
        gamePlayerInstance.RpcUpdateUIForAllClients();
    }




    // RPC to update UI for all clients
 

}