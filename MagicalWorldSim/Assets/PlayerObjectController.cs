using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class PlayerObjectController : NetworkBehaviour
{
    // Player Data
    [SyncVar] public int ConnectionId;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamId;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool Ready;

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
    private void PlayerReadyUpdate(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            this.Ready = newValue;
        }
        if (isClient)
        {
            LobbyController.instance.UpdatePlayerList();
        }
    }

    [Command]
    private void CMdSetPlayerReady()
    {
        this.PlayerReadyUpdate(this.Ready, !this.Ready);

    }

    public void ChangeReady()
    {
        if (isOwned)
        {
            CMdSetPlayerReady();
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

        CmdAddPlayerToList(); // Make sure the server adds the player to the list

        if (isClient)
        {
            LobbyController.instance.UpdatePlayerList();
        }
        LobbyController.instance.UpdateLobbyName();
        LobbyController.instance.UpdatePlayerList();
    }

    [Command]
    private void CmdAddPlayerToList()
    {
        Manager.GamePlayers.Add(this);
    }


    public override void OnStopClient()
    {
        Manager.GamePlayers.Remove(this);
        LobbyController.instance.UpdatePlayerList();
    }

    [Command]
    private void CmdSetPlayerName(string playerName)
    {
        PlayerName = playerName; // Correctly setting the SyncVar
    }

    public void PlayerNameUpdate(string oldValue, string newValue)
    {
        if (isServer)
        {
            PlayerName = newValue;
        }

        if (isClient)
        {
            LobbyController.instance.UpdatePlayerList();
        }
    }

}