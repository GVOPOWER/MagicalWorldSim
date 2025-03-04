using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using System.Linq;
using UnityEngine.UI;
using Edgegap;

public class LobbyController : MonoBehaviour
{
    public static LobbyController instance;

    public Text LobbyNameText;
    public GameObject PlayerListViewContent;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;

    public ulong CurrentLobbyId;
    public bool PlayerItemCreated = false;
    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();
    public PlayerObjectController LocalplayerController;

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

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void UpdateLobbyName()
    {
        CurrentLobbyId = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyId), "name");
    }

    public void UpdatePlayerList()
    {
        // Ensure new players are added correctly
        if (!PlayerItemCreated)
        {
            CreateHostPlayerItem();
        }

        if (playerListItems.Count < Manager.GamePlayers.Count)
        {
            CreateClientPlayerItem();
        }

        if (playerListItems.Count > Manager.GamePlayers.Count)
        {
            RemovePlayerItem();
        }

        UpdatePlayerItem();
    }

    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalplayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

    public void CreateHostPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab);
            PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();
            NewPlayerItemScript.PlayerName = player.PlayerName;
            NewPlayerItemScript.ConnectionID = player.ConnectionId;
            NewPlayerItemScript.PlayerSteamID = player.PlayerSteamId;
            NewPlayerItemScript.SetPlayerValues();

            NewPlayerItem.transform.SetParent(PlayerListViewContent.transform, false);

            playerListItems.Add(NewPlayerItemScript);
        }
        PlayerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (!playerListItems.Any(b => b.ConnectionID == player.ConnectionId))
            {
                GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab);
                PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();
                NewPlayerItemScript.PlayerName = player.PlayerName;
                NewPlayerItemScript.ConnectionID = player.ConnectionId;
                NewPlayerItemScript.PlayerSteamID = player.PlayerSteamId;
                NewPlayerItemScript.SetPlayerValues();

                NewPlayerItem.transform.SetParent(PlayerListViewContent.transform, false);

                playerListItems.Add(NewPlayerItemScript);
            }
        }

        UpdatePlayerItem(); // Ensure UI is updated after adding new items
    }


    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            foreach (PlayerListItem PlayerListItemScript in playerListItems)
            {
                if (PlayerListItemScript.ConnectionID == player.ConnectionId)
                {
                    PlayerListItemScript.PlayerName = player.PlayerName;
                    PlayerListItemScript.SetPlayerValues();
                }
            }
        }
    }

    public void RemovePlayerItem()
    {
        List<PlayerListItem> playerListItemToRemove = new List<PlayerListItem>();
        foreach (PlayerListItem PlayerListItem in playerListItems)
        {
            if (!Manager.GamePlayers.Any(b => b.ConnectionId == PlayerListItem.ConnectionID))
            {
                playerListItemToRemove.Add(PlayerListItem);
            }
        }

        foreach (PlayerListItem playerlistItemToRemove in playerListItemToRemove)
        {
            playerListItems.Remove(playerlistItemToRemove);
            Destroy(playerlistItemToRemove.gameObject);
        }
    }
}
