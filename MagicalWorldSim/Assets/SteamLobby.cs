using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using HeathenEngineering.SteamworksIntegration;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby instance;
    //callbacks
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;


    // variables
    public ulong CurrentLobbyID;
    private const string HostAddressKey = "HostAddress";
    private customnetworkmanager manager;


    private void Start()
    {
        if (instance == null) { instance = this; }

        if (!Steamworks.SteamAPI.IsSteamRunning())
        {
            Debug.LogError("Steam is not running! Make sure Steam is open before launching the game.");
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
        if (callback.m_eResult != EResult.k_EResultOK) { return; }
        Debug.Log("lobbyCreated succes");

        manager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'S LOBBY");
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("request to join lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        Debug.Log("Joined Steam Lobby Successfully!");

        CurrentLobbyID = callback.m_ulSteamIDLobby;

        if (NetworkServer.active)
        {
            // Ensure LobbyController.instance is not null
            if (LobbyController.instance != null)
            {
                LobbyController.instance.UpdatePlayerList();
            }
            return;
        }

        StartCoroutine(DelayedStartClient(callback.m_ulSteamIDLobby));
    }

    private IEnumerator DelayedStartClient(ulong lobbyId)
    {
        yield return new WaitForSeconds(1.0f); // Small delay to ensure Steam fetches data

        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(lobbyId), HostAddressKey);
        if (string.IsNullOrEmpty(hostAddress))
        {
            Debug.LogError("Failed to get Host Address from Steam Lobby.");
            yield break;
        }

        manager.networkAddress = hostAddress;
        manager.StartClient();

        // Ensure the player list is updated for the new client
        LobbyController.instance.UpdatePlayerList();
    }


}