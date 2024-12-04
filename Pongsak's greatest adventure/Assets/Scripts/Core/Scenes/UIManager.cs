using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public GameObject restartPanel; // The panel with buttons
    public GameObject defaultSelectedButton; // The button selected by default when the panel is shown

    public void ShowRestartPanel()
    {
        if (restartPanel == null)
        {
            Debug.LogError("Restart Panel is not assigned in the Inspector.");
            return;
        }

        restartPanel.SetActive(true);

        if (defaultSelectedButton == null)
        {
            Debug.LogError("Default Selected Button is not assigned in the Inspector.");
            return;
        }

        if (EventSystem.current == null)
        {
            Debug.LogError("Event System is missing from the scene.");
            return;
        }

        // Set default button focus for joystick input
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
    }
    public void HideRestartPanel()
    {
        restartPanel.SetActive(false);
    }

    public void RestartGame()
    {
        PlayerRespawnManager.Instance.RespawnPlayer();
        HideRestartPanel();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Replace with your actual main menu scene name
    }
}