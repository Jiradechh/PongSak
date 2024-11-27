using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCombat.Instance.ReloadProjectiles();
            Destroy(gameObject);
        }
    }
}