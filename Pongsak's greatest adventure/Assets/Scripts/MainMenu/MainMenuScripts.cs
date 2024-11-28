using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScripts : MonoBehaviour
{
    public static MainMenuScripts Instance;

    [Header("Main Menu Elements")]
    public Button continueButton;
    public Button startButton;
    public Button exitButton;

    private List<Button> menuButtons;
    private int selectedButtonIndex = 0;

    private float inputDelay = 0.3f;
    private float lastInputTime = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        bool hasSaveGame = PlayerPrefs.GetInt("MaxHealth", 0) > 0;
        SetContinueButtonState(hasSaveGame);

        continueButton.onClick.AddListener(ContinueGame);
        startButton.onClick.AddListener(() => LoadScene("Lobby"));
        exitButton.onClick.AddListener(ExitGame);

        menuButtons = new List<Button> { continueButton, startButton, exitButton };
        HighlightButton(selectedButtonIndex);
    }

    private void Update()
    {
        if (Time.time - lastInputTime > inputDelay)
        {
            HandleMenuNavigation();
        }
    }

    public void SetContinueButtonState(bool isActive)
    {
        if (continueButton == null)
        {
            Debug.LogError("Continue Button is not assigned in MainMenuScripts!");
            return;
        }

        continueButton.gameObject.SetActive(isActive);
    }

    private void HandleMenuNavigation()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetAxis("Vertical") > 0.5f)
        {
            Navigate(-1);
            lastInputTime = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || Input.GetAxis("Vertical") < -0.5f)
        {
            Navigate(1);
            lastInputTime = Time.time;
        }

        if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Return))
        {
            var selectedButton = menuButtons[selectedButtonIndex];
            if (selectedButton.gameObject.activeSelf)
            {
                selectedButton.onClick.Invoke();
                lastInputTime = Time.time;
            }
        }
    }

    private void Navigate(int direction)
    {
        selectedButtonIndex = (selectedButtonIndex + direction + menuButtons.Count) % menuButtons.Count;
        HighlightButton(selectedButtonIndex);
    }

    private void HighlightButton(int index)
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            var colors = menuButtons[i].colors;
            colors.normalColor = i == index ? Color.red : Color.white;
            menuButtons[i].colors = colors;
        }
    }

    private void ContinueGame()
    {
        SaveManager.Instance.onContinue = true;
        SceneManager.LoadScene("Lobby");
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