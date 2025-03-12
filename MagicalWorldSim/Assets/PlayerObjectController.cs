using UnityEngine;
using Mirror;
using Steamworks;

public class PlayerObjectController : NetworkBehaviour
{
    // SyncVars with private backing fields
    [SyncVar(hook = nameof(OnConnectionIdChanged))]
    private int connectionId;
    [SyncVar(hook = nameof(OnPlayerIdNumberChanged))]
    private int playerIdNumber;
    [SyncVar(hook = nameof(OnPlayerSteamIdChanged))]
    private ulong playerSteamId;
    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    private string playerName;
    [SyncVar(hook = nameof(OnPlayerReadyChanged))]
    private bool ready;

    // Public accessors
    public int ConnectionId => connectionId;
    public int PlayerIdNumber => playerIdNumber;
    public ulong PlayerSteamId => playerSteamId;
    public string PlayerName => playerName;
    public bool Ready => ready;

    private customnetworkmanager manager;

    private customnetworkmanager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = customnetworkmanager.singleton as customnetworkmanager;
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName());
        gameObject.name = "LocalGamePlayer";
        LobbyController.instance.FindLocalPlayer();
        LobbyController.instance.UpdateLobbyName();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (isServer)
        {
            Manager.GamePlayers.Add(this);
        }
        LobbyController.instance.UpdatePlayerList();
    }

    [ClientRpc]
    public void RpcUpdateUIForAllClients()
    {
        if (LobbyController.instance != null)
        {
            LobbyController.instance.UpdatePlayerList();
        }
        else
        {
            Debug.LogWarning("LobbyController instance is not available.");
        }
    }

    public override void OnStopClient()
    {
        if (isServer)
        {
            Manager.GamePlayers.Remove(this);
        }
        LobbyController.instance.UpdatePlayerList();
    }

    [Command]
    public void CmdSetPlayerReady()
    {
        ready = !ready;
    }

    public void ChangeReady()
    {
        if (isOwned)
        {
            CmdSetPlayerReady();
        }
    }

    [Command]
    private void CmdSetPlayerName(string playerName)
    {
        this.playerName = playerName;
    }

    // Method to set values on the server
    [Server]
    public void SetPlayerInfo(int connId, int playerId, ulong steamId)
    {
        connectionId = connId;
        playerIdNumber = playerId;
        playerSteamId = steamId;
    }

    // Hooks for SyncVars to update the UI
    private void OnConnectionIdChanged(int oldValue, int newValue) => UpdatePlayerInfo();
    private void OnPlayerIdNumberChanged(int oldValue, int newValue) => UpdatePlayerInfo();
    private void OnPlayerSteamIdChanged(ulong oldValue, ulong newValue) => UpdatePlayerInfo();
    private void OnPlayerNameChanged(string oldValue, string newValue) => UpdatePlayerInfo();
    private void OnPlayerReadyChanged(bool oldValue, bool newValue) => UpdatePlayerInfo();

    private void UpdatePlayerInfo()
    {
        if (isClient)
        {
            LobbyController.instance.UpdatePlayerList();
        }
    }
}
