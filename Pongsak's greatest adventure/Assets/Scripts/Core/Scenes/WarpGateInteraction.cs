using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpGateInteraction : MonoBehaviour
{
    private bool playerIsNear = false;
    private StageManager stageManager;

    void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
    }

    void Update()
    {
        if (playerIsNear && (Input.GetKeyDown(KeyCode.Joystick1Button5) || Input.GetKeyDown(KeyCode.F)))
        {
            stageManager.WarpToNextStage();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
        }
    }
}