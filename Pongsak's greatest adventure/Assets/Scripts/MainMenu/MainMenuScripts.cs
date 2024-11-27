using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScripts : MonoBehaviour
{
    [Header("Main Menu Elements")]
    public Button startButton;
    public Button exitButton;

    private Button[] menuButtons;
    private int selectedIndex = 0;

    private void Start()
    {
        startButton.onClick.AddListener(() => LoadScene("Lobby"));
        exitButton.onClick.AddListener(ExitGame);

        // Assign menu buttons
        menuButtons = new Button[] { startButton, exitButton };
        HighlightButton(selectedIndex); // Highlight the first button
    }

    private void Update()
    {
        HandleMenuNavigation();
    }

    private void HandleMenuNavigation()
    {
        float verticalInput = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || verticalInput > 0.5f)
            Navigate(-1); // Up

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || verticalInput < -0.5f)
            Navigate(1); // Down

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Submit"))
        {
            menuButtons[selectedIndex].onClick.Invoke();
        }
    }

    private void Navigate(int direction)
    {
        selectedIndex = (selectedIndex + direction + menuButtons.Length) % menuButtons.Length;
        HighlightButton(selectedIndex);
    }

    private void HighlightButton(int index)
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            var colors = menuButtons[i].colors;
            colors.normalColor = i == index ? Color.yellow : Color.white;
            menuButtons[i].colors = colors;
        }
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    private void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exiting game...");
    }
}