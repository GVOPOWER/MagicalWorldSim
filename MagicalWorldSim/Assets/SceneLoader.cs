using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadGameScene(string gme)
    {
        if (!string.IsNullOrEmpty(gme))
        {
            SceneManager.LoadScene(gme);
        }
        else
        {
            Debug.LogError("Scene name is empty! Set a valid scene name.");
        }
    }
}
