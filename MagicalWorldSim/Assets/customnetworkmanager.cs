using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;

// Ensure this script is attached to a GameObject in your scene.
public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    public void RpcUpdatePlayerLists()
    {
        if (LobbyController.instance != null)
        {
            LobbyController.instance.UpdatePlayerList();
        }
        else
        {
            Debug.LogWarning("LobbyController instance not found on client.");
        }
    }
}

public class customnetworkmanager : NetworkManager
{
    [SerializeField] private PlayerObjectController gamePlayerPrefab;
    public List<PlayerObjectController> GamePlayers { get; } = new List<PlayerObjectController>();

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("NetworkServer is not active! Player cannot be added.");
            return;
        }

        if (SceneManager.GetActiveScene().name == "Hosting")
        {
            var gamePlayerInstance = Instantiate(gamePlayerPrefab);
            gamePlayerInstance.ConnectionId = conn.connectionId;
            gamePlayerInstance.PlayerIdNumber = GamePlayers.Count + 1;

            if (SteamLobby.instance != null && SteamLobby.instance.CurrentLobbyID != 0)
            {
                int playerIndex = GamePlayers.Count;
                gamePlayerInstance.PlayerSteamId = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.instance.CurrentLobbyID, playerIndex);
            }
            else
            {
                Debug.LogError("Steam Lobby is not initialized properly.");
            }

            NetworkServer.AddPlayerForConnection(conn, gamePlayerInstance.gameObject);
            GamePlayers.Add(gamePlayerInstance);

            LobbyController.instance?.UpdatePlayerList();
            LobbyManager.instance?.RpcUpdatePlayerLists();
        }
    }
}
