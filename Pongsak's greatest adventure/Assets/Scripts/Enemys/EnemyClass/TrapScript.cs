using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapScript : MonoBehaviour
{
    public float initialDelayTime = 0.4f;
    public float activeDuration = 5f;
    public float damageInterval = 0.5f;
    public int damageAmount = 25;
    public Animator trapAnimator;

    private bool isTriggered = false;
    private bool playerInTrap = false;
    private List<IDamageable> enemiesInTrap = new List<IDamageable>();
    private Coroutine trapCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            playerInTrap = true;
            trapCoroutine = StartCoroutine(TriggerTrap(other));
        }
        else if (other.CompareTag("Enemy") && isTriggered)
        {
            IDamageable enemy = other.GetComponent<IDamageable>();
            if (enemy != null)
            {
                enemiesInTrap.Add(enemy);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrap = false;
        }
        else if (other.CompareTag("Enemy"))
        {
            IDamageable enemy = other.GetComponent<IDamageable>();
            if (enemy != null)
            {
                enemiesInTrap.Remove(enemy);
            }
        }
    }

    IEnumerator TriggerTrap(Collider player)
    {
        isTriggered = true;

        yield return new WaitForSeconds(initialDelayTime);

        if (trapAnimator != null)
        {
            trapAnimator.SetTrigger("Activate");
        }

        float elapsedTime = 0f;
        while (elapsedTime < activeDuration)
        {
            if (playerInTrap)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageAmount);
                }
            }

            foreach (var enemy in enemiesInTrap)
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(damageAmount);
                }
            }

            yield return new WaitForSeconds(damageInterval);
            elapsedTime += damageInterval;
        }

        if (trapAnimator != null)
        {
            trapAnimator.SetTrigger("Deactivate");
        }

        isTriggered = false;
        enemiesInTrap.Clear();
    }
}