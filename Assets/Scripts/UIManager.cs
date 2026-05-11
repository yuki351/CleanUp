using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // Start Game button
    public void StartGame()
    {
        SceneManager.LoadScene("MainGame");
    }

    // Exit Game button
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game Closed");
    }

    // How To Play button
    public void HowToPlay()
    {
        SceneManager.LoadScene("Instructions");
    }
}