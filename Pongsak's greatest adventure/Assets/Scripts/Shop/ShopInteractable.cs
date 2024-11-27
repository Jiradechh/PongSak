using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteractable : MonoBehaviour
{
    public ShopSystem shopSystem;
    private bool playerIsNear = false;

    private void Update()
    {
        
        if (playerIsNear && Input.GetKeyDown(KeyCode.Joystick1Button7))
        {
            
            shopSystem.ToggleShop(!shopSystem.shopUI.activeSelf);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
            shopSystem.ToggleShop(false);
        }
    }
}