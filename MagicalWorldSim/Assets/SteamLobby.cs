using UnityEngine;
using Steamworks;
using Mirror;
using System.Collections;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby instance;
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    public ulong CurrentLobbyID;
    private const string HostAddressKey = "HostAddress";
    private customnetworkmanager manager;

    private void Start()
    {
        if (instance == null) { instance = this; }

        if (!SteamAPI.Init())
        {
            Debug.LogError("Steam API initialization failed. Make sure Steam is running.");
            return;
        }

        manager = GetComponent<customnetworkmanager>();

        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, manager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Failed to create Steam lobby.");
            return;
        }
        Debug.Log("Lobby created successfully.");

        manager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName() + "'S LOBBY");
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request to join lobby.");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        Debug.Log("Joined Steam Lobby successfully.");

        CurrentLobbyID = callback.m_ulSteamIDLobby;

        if (NetworkServer.active)
        {
            LobbyController.instance?.UpdatePlayerList();
            return;
        }

        StartCoroutine(DelayedStartClient(callback.m_ulSteamIDLobby));
    }

    private IEnumerator DelayedStartClient(ulong lobbyId)
    {
        yield return new WaitForSeconds(1.0f); // Allow time for data retrieval

        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(lobbyId), HostAddressKey);
        if (string.IsNullOrEmpty(hostAddress))
        {
            Debug.LogError("Failed to get Host Address from Steam Lobby.");
            yield break;
        }

        manager.networkAddress = hostAddress;
        manager.StartClient();

        // Ensure the player list is updated for the new client
        LobbyController.instance?.UpdatePlayerList();
    }
}
