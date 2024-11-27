using MyInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerHealth : Singleton<PlayerHealth> , TakeDamage
{
    #region Public Variables
    public int maxHealth = 100;
    public int currentHealth;
    public SpriteRenderer spriteRenderer;
    public float blinkDuration = 3f;
    public PlayerController playerController;
    public float vibrationStrength = 0.5f;
    public float vibrationDuration = 0.5f;
    public int damageTestValue = 25;
    public bool isDead = false;

    public HeartUI heartUI;

    [Header("Animation Settings")]
    public Animator animator;

    public bool hasCarti = false;
    #endregion

    #region Private Variables
    public bool isInvincible = false;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    #region Unity Callbacks
    void Start()
    {
        /*if (SaveManager.Instance.onContinue)
        {
            //Load Savegems
            maxHealth = SaveManager.Instance.saveData.maxHealth;
        }*/

        currentHealth = maxHealth;
        heartUI.UpdateHearts(currentHealth, maxHealth);
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Test Damage Triggered");
            TakeDamage(damageTestValue);
        }
    }
    #endregion

    #region Health Logic
    public void TakeDamage(int damage)
    {
        if (!isInvincible && !isDead)
        {
            currentHealth -= damage;
            heartUI.UpdateHearts(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
            else
            {
                StartCoroutine(HurtAnimation());
                StartCoroutine(BlinkEffect());
            }
        }
    }

    private void Die()
    {
        if (isDead) return;

        if (hasCarti)
        {
            Debug.Log("Carti item consumed: Player respawns without returning to the lobby!");
            StartCoroutine(ReviveWithCarti());
            return;
        }

        Debug.Log("Player has died.");
        isDead = true;

        animator.SetTrigger("Die");
        playerController.enabled = false;

        StageManager.Instance.ResetToFirstStage();

       

        PlayerRespawnManager respawnManager = FindObjectOfType<PlayerRespawnManager>();
        if (respawnManager != null)
        {
            respawnManager.RespawnPlayer();
        }
        else
        {
            Debug.LogError("PlayerRespawnManager not found in the scene. Please ensure it is added.");
        }
    }

    
    private IEnumerator ReviveWithCarti()
    {
        hasCarti = false;
        Debug.Log("Reviving player in 2 seconds...");

        yield return new WaitForSeconds(2f);

        RestoreToMaxHealth();
        isDead = false;
        animator.SetTrigger("Revive");
        playerController.enabled = true;
        Debug.Log("Player has been revived!");
    }
    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth;
        heartUI.UpdateHearts(currentHealth, maxHealth);
        Debug.Log($"Max Health increased by {amount}. New Max Health: {maxHealth}");
    }

    public void RestoreToMaxHealth()
    {
        currentHealth = maxHealth;
        heartUI.UpdateHearts(currentHealth, maxHealth);
        Debug.Log("Player's health restored to max HP.");
    }

    public void ActivateAvoidNextHit()
    {
        isInvincible = true;
        Debug.Log("Player will avoid the next hit.");
    }
    #endregion

    #region Hurt Animation and Effects
    private IEnumerator HurtAnimation()
    {
        animator.SetTrigger("Hurt");
        playerController.enabled = true;

        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(vibrationStrength, vibrationStrength);
            yield return new WaitForSeconds(vibrationDuration);
            Gamepad.current.SetMotorSpeeds(0, 0);
        }

        yield return new WaitForSeconds(1f);
        playerController.enabled = true;
    }

    private IEnumerator BlinkEffect()
    {
        isInvincible = true;
        float elapsedTime = 0f;
        bool isVisible = true;

        while (elapsedTime < blinkDuration)
        {
            isVisible = !isVisible;
            spriteRenderer.enabled = isVisible;
            elapsedTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
    }
    #endregion
}