using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Edgegap;

public class LobbyController : MonoBehaviour
{
    public static LobbyController instance;

    public TMP_Text LobbyNameText;
    public GameObject PlayerListViewContent;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;
    public Button LeaveButton;
    public Button StartGameButton;
    public TMP_Text ReadyButtonText;

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

    private void Start()
    {
        LeaveButton.onClick.AddListener(LeaveLobby);
    }

    public void UpdateLobbyName()
    {
        CurrentLobbyId = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyId), "name");
    }

    public void ReadyPlayer()
    {
        LocalplayerController.ChangeReady();
    }

    public void UpdateButton()
    {
        ReadyButtonText.text = LocalplayerController.Ready ? "Unready" : "Ready";
    }

    public void CheckIfAllReady()
    {
        bool allReady = Manager.GamePlayers.All(player => player.Ready);
        StartGameButton.interactable = allReady && LocalplayerController.PlayerIdNumber == 1;
    }

    public void StartGame()
    {
        if (LocalplayerController.PlayerIdNumber == 1)
        {
            Manager.ServerChangeScene("gme"); // Change to your scene name
        }
    }

    public void UpdatePlayerList()
    {
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

    public void LeaveLobby()
    {
        if (!Steamworks.SteamAPI.IsSteamRunning())
        {
            SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyId));
        }
        Manager.StopHost();
    }

    public void CreateHostPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            CreatePlayerItem(player);
        }
        PlayerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (!playerListItems.Any(b => b.ConnectionID == player.ConnectionId))
            {
                CreatePlayerItem(player);
            }
        }
        UpdatePlayerItem();
    }

    private void CreatePlayerItem(PlayerObjectController player)
    {
        GameObject newPlayerItem = Instantiate(PlayerListItemPrefab);
        PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();
        newPlayerItemScript.PlayerName = player.PlayerName;
        newPlayerItemScript.ConnectionID = player.ConnectionId;
        newPlayerItemScript.PlayerSteamID = player.PlayerSteamId;
        newPlayerItemScript.Ready = player.Ready;
        newPlayerItemScript.SetPlayerValues();

        newPlayerItem.transform.SetParent(PlayerListViewContent.transform, false);
        playerListItems.Add(newPlayerItemScript);
    }

    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            foreach (PlayerListItem playerListItemScript in playerListItems)
            {
                if (playerListItemScript.ConnectionID == player.ConnectionId)
                {
                    playerListItemScript.PlayerName = player.PlayerName;
                    playerListItemScript.Ready = player.Ready;
                    playerListItemScript.SetPlayerValues();
                    if (player == LocalplayerController)
                    {
                        UpdateButton();
                    }
                }
            }
        }
        CheckIfAllReady();
    }

    public void RemovePlayerItem()
    {
        List<PlayerListItem> playerListItemToRemove = playerListItems.Where(
            item => !Manager.GamePlayers.Any(player => player.ConnectionId == item.ConnectionID)).ToList();

        foreach (PlayerListItem playerlistItemToRemove in playerListItemToRemove)
        {
            playerListItems.Remove(playerlistItemToRemove);
            Destroy(playerlistItemToRemove.gameObject);
        }
    }
}
