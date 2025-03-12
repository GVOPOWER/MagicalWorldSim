using UnityEngine;
using Mirror;
using Steamworks;

public class PlayerObjectController : NetworkBehaviour
{
    // Player Data
    [SyncVar(hook = nameof(OnConnectionIdChanged))] public int ConnectionId;
    [SyncVar(hook = nameof(OnPlayerIdNumberChanged))] public int PlayerIdNumber;
    [SyncVar(hook = nameof(OnPlayerSteamIdChanged))] public ulong PlayerSteamId;
    [SyncVar(hook = nameof(OnPlayerNameChanged))] public string PlayerName;
    [SyncVar(hook = nameof(OnPlayerReadyChanged))] public bool Ready;

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
        Ready = !Ready;
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
        PlayerName = playerName;
    }

    // Hooks for SyncVars to update UI
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
