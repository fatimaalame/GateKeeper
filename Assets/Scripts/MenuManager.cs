using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void EnterTheMaze()
    {
        SceneManager.LoadScene("AdaptiveMazeScene");
    }

    public void OpenHowToPlay()
    {
        SceneManager.LoadScene("HowToPlay");
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

     public void OpenLoveLetter()
    {
        SceneManager.LoadScene("JTM");
    }
}