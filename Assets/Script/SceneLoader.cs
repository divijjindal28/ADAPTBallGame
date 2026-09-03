using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    public int sceneNumber;

    // Load the scene using the Inspector scene number
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneNumber);
    }

    // Optional: Load any scene by passing a number
    public void LoadSceneByNumber(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        Time.timeScale = 1f;

    }
}