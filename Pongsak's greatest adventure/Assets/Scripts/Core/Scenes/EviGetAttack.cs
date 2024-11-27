using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EviGetAttack : MonoBehaviour , IDamageable
{
    #region Public Variables (Serialized Fields)
    [Header("Enemy Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private ParticleSystem deathParticles; 
    #endregion

    #region Private Variables
    private int currentHealth;  
    private bool isDead = false; 
    #endregion

    #region Unity Callbacks
    private void Start()
    {
       
        currentHealth = maxHealth;
    }
    #endregion

    #region Damage Handling
    public void TakeDamage(int damage)
    {
        if (isDead) return; 

        
        currentHealth -= damage;

        Debug.Log(gameObject.name + " took " + damage + " damage!");

        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void DropGold()
    {
        
        int randomChance = Random.Range(1, 101);

        int goldToDrop = 0;

        if (randomChance <= 60)
        {
            goldToDrop = 0;
        }
        else if (randomChance > 60 && randomChance <= 90)
        {
            goldToDrop = 1;
        }
        else if (randomChance > 90)
        {
            goldToDrop = 3;
        }

        if (goldToDrop > 0)
        {
            
            CurrencyManager.Instance.AddGold(goldToDrop);
            Debug.Log($"Dropped {goldToDrop} Gold!");
        }
        else
        {
            Debug.Log("No Gold Dropped.");
        }
    }
    private void Die()
    {
        isDead = true;

        if (deathParticles == null)
        {
            Debug.LogError("Death particles are not assigned!");
        }
        else
        {
            ParticleSystem particles = Instantiate(deathParticles, transform.position, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, particles.main.duration);
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("CurrencyManager instance is null!");
        }
        else if (CurrencyManager.Instance.canDropGold)
        {
            DropGold();
        }

        Destroy(gameObject);
        Debug.Log(gameObject.name + " has died.");
    }
    #endregion
}