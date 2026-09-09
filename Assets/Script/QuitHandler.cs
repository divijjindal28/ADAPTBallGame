using UnityEngine;

public class QuitHandler : MonoBehaviour
{
    // Call this function via a UI Button OnClick() event
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Stops play mode if you are running inside the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Closes the actual built application (Windows, Mac, Linux, Android)
            Application.Quit();
#endif
    }

    // Optional: Triggers the quit function if the player presses the Escape key
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }
}