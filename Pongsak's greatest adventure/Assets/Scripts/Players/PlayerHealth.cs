using MyInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Cinemachine;

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
        public PlayerCombat playerCombat;
        public PlayerHealth playerHealth;
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
            playerCombat = PlayerCombat.Instance;
            playerController = PlayerController.Instance;
            playerHealth = this;
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
                SoundManager.Instance.PlayEffect(SoundManager.Instance.hurtClip);
                StartCoroutine(HurtAnimation());
                StartCoroutine(BlinkEffect());
            }
        }
    }
    public void KnockBack()
        {
            StartCoroutine(DelayKnockBack());
        }

        IEnumerator DelayKnockBack()
        {
            yield return new WaitForSeconds(0.2f);
            playerController.rb.Sleep();
            playerController.rb.velocity = Vector3.zero;
        }
    private void Die()
    {
        if (isDead) return;

        Debug.Log("Player has died.");
        isDead = true;

        SoundManager.Instance.PlayEffect(SoundManager.Instance.dieClip);

        animator.SetTrigger("Die");
        playerController.enabled = false;
        ResetAllPurchasedEffects();
        StageManager.Instance.ResetToFirstStage();

        CurrencyManager currencyManager = FindObjectOfType<CurrencyManager>();
        if (currencyManager != null)
        {
            currencyManager.ResetGold();
        }
        else
        {
            Debug.LogError("CurrencyManager not found in the scene. Please ensure it is added.");
        }
        PlayerCombat playerCombat = PlayerCombat.Instance;
        if (playerCombat != null)
        {
            playerCombat.canAttack = false; 
        }
        // Show Restart Panel
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowRestartPanel();
        }
    }

    private void ResetAllPurchasedEffects()
        {
            Debug.Log("Resetting all purchased item effects.");

            PlayerController.Instance.DecreaseMaxDashes();
            playerCombat.DecreaseMaxProjectiles();
            playerCombat.DisableSlowEffect();
            CurrencyManager.Instance.DisableGoldDrop();
            PlayerController.Instance.DecreaseDashSpeed(5f);
            playerCombat.DisableAOEProjectiles();
            playerCombat.DisableStunEffect();
            PlayerController.Instance.SetMoveSpeed(2f);
            CinemachineVirtualCamera camera = FindObjectOfType<CinemachineVirtualCamera>();
            if (camera != null)
            {
                camera.m_Lens.FieldOfView = 15f;
            }

            Debug.Log("All purchased item effects reset successfully.");
        }

        private IEnumerator ReviveWithCarti()
        {
            Debug.Log("Reviving player with Carti item...");
            yield return new WaitForSeconds(3f);
            playerHealth.RestoreToMaxHealth();
            isDead = false;
            playerController.enabled = true;
            playerController.ResetControllerState();
            playerCombat.ResetCombatState();

            Debug.Log("Player revived successfully with Carti item!");
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