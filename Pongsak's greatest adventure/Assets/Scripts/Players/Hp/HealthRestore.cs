using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthRestore : MonoBehaviour
{
    private PlayerHealth playerHealth;

    private void Start()
    {
       
        playerHealth = FindObjectOfType<PlayerHealth>();
    }

    
    public void RestoreHealthToMax()
    {
        if (playerHealth != null)
        {
            playerHealth.RestoreToMaxHealth();
            Debug.Log("Player's health has been fully restored.");
        }
        else
        {
            Debug.LogWarning("PlayerHealth script not found!");
        }
    }
}