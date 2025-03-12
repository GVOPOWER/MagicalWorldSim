using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

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
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (LeaveButton != null)
        {
            LeaveButton.onClick.AddListener(LeaveLobby);
        }
        else
        {
            Debug.LogError("LeaveButton is not assigned.");
        }

        if (StartGameButton == null)
        {
            Debug.LogError("StartGameButton is not assigned.");
        }

        FindLocalPlayer();
        if (LocalplayerController == null)
        {
            Debug.LogError("LocalplayerController is not assigned.");
        }

        UpdateLobbyName();
        StartCoroutine(InitializeLocalPlayer());

    }

    public void UpdateLobbyName()
    {
        if (Manager != null && Manager.GetComponent<SteamLobby>() != null)
        {
            CurrentLobbyId = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
            if (LobbyNameText != null)
            {
                LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyId), "name");
            }
            else
            {
                Debug.LogError("LobbyNameText is not assigned.");
            }
        }
        else
        {
            Debug.LogError("SteamLobby component is missing or Manager is null.");
        }
    }

    public void ReadyPlayer()
    {
        if (LocalplayerController != null)
        {
            LocalplayerController.ChangeReady();
        }
        else
        {
            Debug.LogError("LocalplayerController is not assigned.");
        }
    }

    public void UpdateButton()
    {
        if (ReadyButtonText != null && LocalplayerController != null)
        {
            ReadyButtonText.text = LocalplayerController.Ready ? "Unready" : "Ready";
        }
        else
        {
            Debug.LogError("ReadyButtonText or LocalplayerController is null.");
        }
    }

    public void CheckIfAllReady()
    {
        if (Manager != null && Manager.GamePlayers != null)
        {
            bool allReady = Manager.GamePlayers.All(player => player.Ready);
            if (StartGameButton != null && LocalplayerController != null)
            {
                StartGameButton.interactable = allReady && LocalplayerController.PlayerIdNumber == 1;
            }
            else
            {
                Debug.LogError("StartGameButton or LocalplayerController is null.");
            }
        }
    }

    public void StartGame()
    {
        if (LocalplayerController != null && LocalplayerController.PlayerIdNumber == 1)
        {
            if (Manager != null)
            {
                Manager.ServerChangeScene("gme"); // Change "gme" to your actual game scene name
            }
            else
            {
                Debug.LogError("Manager is null.");
            }
        }
    }

    public void UpdatePlayerList()
    {
        if (Manager != null && Manager.GamePlayers != null)
        {
            // Clear the existing list
            foreach (Transform child in PlayerListViewContent.transform)
            {
                Destroy(child.gameObject);
            }

            // Recreate the player items list
            foreach (PlayerObjectController player in Manager.GamePlayers)
            {
                CreatePlayerItem(player);
            }
        }
        else
        {
            Debug.LogError("Manager or GamePlayers is null.");
        }
    }


    internal void FindLocalPlayer()
    {
        if (NetworkClient.localPlayer != null)
        {
            var localPlayerController = NetworkClient.localPlayer.GetComponent<PlayerObjectController>();
            if (localPlayerController != null)
            {
                LocalplayerController = localPlayerController; // Set the LocalplayerController
                Debug.Log("Local player object found.");
            }
            else
            {
                Debug.LogError("PlayerObjectController component not found on local player.");
            }
        }
        else
        {
            Debug.LogError("Local player NetworkIdentity not found.");
        }
    }

    private IEnumerator InitializeLocalPlayer()
    {
        yield return new WaitUntil(() => NetworkClient.localPlayer != null);
        FindLocalPlayer();
        if (LocalplayerController == null)
        {
            Debug.LogError("LocalplayerController is not assigned after initialization.");
        }
    }
    public void LeaveLobby()
    {
        if (SteamAPI.IsSteamRunning())
        {
            SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyId));
        }
        if (Manager != null)
        {
            Manager.StopHost();
        }
        else
        {
            Debug.LogError("Manager is null.");
        }
    }

    public void CreateHostPlayerItem()
    {
        if (Manager != null && Manager.GamePlayers != null)
        {
            foreach (PlayerObjectController player in Manager.GamePlayers)
            {
                CreatePlayerItem(player);
            }
            PlayerItemCreated = true;
        }
    }

    public void CreateClientPlayerItem()
    {
        if (Manager != null && Manager.GamePlayers != null)
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
    }

    private void CreatePlayerItem(PlayerObjectController player)
    {
        if (PlayerListItemPrefab != null && PlayerListViewContent != null)
        {
            GameObject newPlayerItem = Instantiate(PlayerListItemPrefab);
            PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();
            if (newPlayerItemScript != null)
            {
                newPlayerItemScript.PlayerName = player.PlayerName;
                newPlayerItemScript.ConnectionID = player.ConnectionId;
                newPlayerItemScript.PlayerSteamID = player.PlayerSteamId;
                newPlayerItemScript.Ready = player.Ready;
                newPlayerItemScript.SetPlayerValues();

                newPlayerItem.transform.SetParent(PlayerListViewContent.transform, false);
                playerListItems.Add(newPlayerItemScript);
            }
            else
            {
                Debug.LogError("PlayerListItem component is missing on prefab.");
            }
        }
        else
        {
            Debug.LogError("PlayerListItemPrefab or PlayerListViewContent is null.");
        }
    }

    public void UpdatePlayerItem()
    {
        if (Manager != null && Manager.GamePlayers != null)
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
    }

    public void RemovePlayerItem()
    {
        if (Manager != null && Manager.GamePlayers != null)
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
}
