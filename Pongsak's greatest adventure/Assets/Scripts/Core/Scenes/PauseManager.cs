using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu; // Reference to the Pause Menu Panel

    private bool isPaused = false;

    void Update()
    {
        // Toggle pause menu with the Escape key or Start button on the controller
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true); // Show pause menu
        Time.timeScale = 0f; // Freeze game time
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false); // Hide pause menu
        Time.timeScale = 1f; // Resume game time
        isPaused = false;
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Ensure time is running normally before leaving the game
        SceneManager.LoadScene("MainMenu"); // Replace with the actual name of your main menu scene
    }
}