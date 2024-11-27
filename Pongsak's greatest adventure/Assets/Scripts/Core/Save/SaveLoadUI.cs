using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SaveLoadUI : MonoBehaviour
{
    public Button loadButton;
    private void Start()
    {
        loadButton.onClick.AddListener(LoadGame);
    }

    public void LoadGame()
    {
        SaveManager.Instance.onContinue = true;

        SceneManager.LoadScene("Lobby");
    }
}