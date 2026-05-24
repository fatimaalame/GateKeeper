using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private void Start()
    {
        // Start the menu music when this menu scene opens.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }
    }

    public void StartGame()
    {
        // Play a small click sound when the player presses the menu button.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        SceneManager.LoadScene("MainScene");
    }

    public void EnterTheMaze()
    {
        // Play a small click sound when the player enters the maze.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        SceneManager.LoadScene("AdaptiveMazeScene");
    }

    public void OpenHowToPlay()
    {
        // Play a small click sound when the player opens the instructions.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        SceneManager.LoadScene("HowToPlay");
    }

    public void ReturnToMainMenu()
    {
        // Play a small click sound when returning to the main menu.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void OpenLoveLetter()
    {
        // Play a small click sound when opening this hidden scene.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        SceneManager.LoadScene("JTM");
    }
}