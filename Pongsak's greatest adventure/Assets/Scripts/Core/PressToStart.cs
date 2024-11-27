using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PressToStart : MonoBehaviour
{
    public string mainSceneName = "Lobby";

    void Update()
    {
        if (Input.anyKeyDown)
        {
            LoadMainScene();
        }
    }

    private void LoadMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }
}