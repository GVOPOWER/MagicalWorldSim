using UnityEngine;
using UnityEngine.SceneManagement;
#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.UI;
#endif

public class SceneLoader : MonoBehaviour
{
#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
    public QuickMatchLobbyControl lobbyControl;
    
    private void Update()
    {
        if (lobbyControl != null && lobbyControl.HasLobby && lobbyControl.AllPlayersReady)
        {
            LoadNextScene();
        }
    }
#endif

    private void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
            if (lobbyControl != null && lobbyControl.HasLobby)
            {
                lobbyControl.SetGameServer();
            }
#endif

            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more scenes to load!");
        }
    }
}
