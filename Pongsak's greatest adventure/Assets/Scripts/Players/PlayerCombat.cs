using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCombat : Singleton<PlayerCombat>
{
    [Header("Projectile Settings")]
    public SpellProjectile spellPrefab;
    public Transform projectileAttackPoint;
    public float projectileSpeed = 10f;
    public int maxProjectiles = 1;
    private int remainingProjectiles;
    private bool canFireProjectile = true;

    [Header("Melee Settings")]
    public Transform meleeAttackPoint;
    public float attackRange = 1.0f;

    [Header("Attack Settings")]
    public int baseLightAttackDamage = 20;
    public int baseHeavyAttackDamage = 50;

    [Header("UI Reference")]
    public ProjectileUIManager projectileUIManager;

    [Header("Cooldowns")]
    public float lightAttackCooldown = 0.5f;
    public float heavyAttackCooldown = 1.5f;

    [Header("Audio Settings")]
    public AudioSource audioSource; 
    public AudioClip fireProjectileClip;
    public AudioClip lightAttackClip;
    public AudioClip heavyAttackClip;

    [Header("Layers and References")]
    public LayerMask attackLayers;
    public SpriteRenderer playerSprite;
    public Rigidbody rb;
    public PlayerController playerController;
    public Animator animator;

    private bool canLightAttack = true;
    private bool canHeavyAttack = true;
    private bool isAimingProjectile = false;
    private bool slowEffectEnabled = false;
    private bool aoeEnabled = false;
    private bool stunEffectEnabled = false;

    public float meleeAttackPointDistance = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (SaveManager.Instance.onContinue)
        {
            //Load Savegems
            baseLightAttackDamage = SaveManager.Instance.saveData.baseLightAttackDamage;
            baseHeavyAttackDamage = SaveManager.Instance.saveData.baseHeavyAttackDamage;
        }



        remainingProjectiles = maxProjectiles;
        meleeAttackPoint.localPosition = new Vector3(0.4f, 0, 0);

        projectileUIManager.UpdateProjectileUI(remainingProjectiles);
    }

    void Update()
    {
        if (PlayerController.isUIActive) return;

        HandleAttacks();
        UpdateAttackPointPosition();
        HandleProjectileFire();
    }
    public void EnableAOEProjectiles()
    {
        aoeEnabled = true;
        Debug.Log("Bomb purchased: AOE projectiles enabled!");
    }

    public void EnableStunEffect()
    {
        stunEffectEnabled = true;
        Debug.Log("Stun effect enabled for future attacks.");
    }

    private void HandleAttacks()
    {
        if (PlayerController.isUIActive) return;

        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Joystick1Button2)) && canLightAttack)
        {
            StartCoroutine(PerformLightAttack());
        }
        if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Joystick1Button3)) && canHeavyAttack)
        {
            StartCoroutine(PerformHeavyAttack());
        }
    }

    private IEnumerator PerformLightAttack()
    {
        canLightAttack = false;
        playerController.canMove = false;
        animator.SetTrigger("LightAttack");

        if (audioSource != null && lightAttackClip != null)
        {
            audioSource.PlayOneShot(lightAttackClip);
        }
        else
        {
            Debug.LogWarning("AudioSource or AudioClip for light attack is missing!");
        }

        Collider[] hitTargets = Physics.OverlapSphere(meleeAttackPoint.position, attackRange, attackLayers);
        foreach (Collider target in hitTargets)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(baseLightAttackDamage);
                Debug.Log("Hit " + target.name + " with Light Attack for " + baseLightAttackDamage + " damage.");
            }
        }

        yield return new WaitForSeconds(0.4f);
        playerController.canMove = true;
        yield return new WaitForSeconds(lightAttackCooldown - 0.4f);

        canLightAttack = true;
    }

    private IEnumerator PerformHeavyAttack()
    {
        canHeavyAttack = false;
        playerController.canMove = false;
        animator.SetTrigger("HeavyAttack");

        if (audioSource != null && heavyAttackClip != null)
        {
            audioSource.PlayOneShot(heavyAttackClip);
        }
        else
        {
            Debug.LogWarning("AudioSource or AudioClip for heavy attack is missing!");
        }

        Collider[] hitTargets = Physics.OverlapSphere(meleeAttackPoint.position, attackRange, attackLayers);
        foreach (Collider target in hitTargets)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(baseHeavyAttackDamage);
                Debug.Log("Hit " + target.name + " with Heavy Attack for " + baseHeavyAttackDamage + " damage.");
            }
        }

        yield return new WaitForSeconds(0.5f);
        playerController.canMove = true;
        yield return new WaitForSeconds(heavyAttackCooldown - 0.5f);

        canHeavyAttack = true;
    }
    private void HandleProjectileFire()
    {
        if (canFireProjectile && (Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.Q)))
        {
            StartAimingProjectile();
        }

        if (isAimingProjectile && (Input.GetKeyUp(KeyCode.JoystickButton1) || Input.GetKeyUp(KeyCode.Q)))
        {
            StartCoroutine(CastSpellAndFireProjectile());
            StopAimingProjectile();
        }
    }

    private void StartAimingProjectile()
    {
        isAimingProjectile = true;
        playerController.canMove = false;
    }

    private void StopAimingProjectile()
    {
        isAimingProjectile = false;
        new WaitForSeconds(0.2f);
        playerController.canMove = true;
    }

    private IEnumerator CastSpellAndFireProjectile()
    {
        animator.SetTrigger("CastSpell");
        yield return new WaitForSeconds(0.2f);
        FireProjectile();
    }

    private void FireProjectile()
    {
        if (remainingProjectiles > 0 && canFireProjectile)
        {
            Vector3 fireDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

            if (fireDirection.magnitude < 0.1f)
            {
                fireDirection = playerSprite.flipX ? Vector3.left : Vector3.right;
            }

            SpellProjectile spell = Instantiate(spellPrefab, projectileAttackPoint.position, Quaternion.identity);
            spell.Fire(fireDirection * projectileSpeed);

            if (slowEffectEnabled)
            {
                spell.EnableSlowEffect();
            }

            if (stunEffectEnabled)
            {
                spell.EnableStun();
            }

            if (audioSource != null && fireProjectileClip != null)
            {
                audioSource.PlayOneShot(fireProjectileClip);
            }
            else
            {
                Debug.LogWarning("AudioSource or AudioClip for firing projectile is missing!");
            }

            remainingProjectiles--;
            canFireProjectile = remainingProjectiles > 0;

            projectileUIManager.UpdateProjectileUI(remainingProjectiles);
        }
    }

    public void EnableSlowEffect()
    {
        slowEffectEnabled = true;
        Debug.Log("Slow Effect enabled for future projectiles.");
    }

    public void IncreaseMaxProjectiles()
    {
        maxProjectiles++;
        remainingProjectiles = maxProjectiles;
        Debug.Log("Player's max projectiles increased to " + maxProjectiles);
    }


    public void ReloadProjectiles()
    {
        if (remainingProjectiles < maxProjectiles)
        {
            remainingProjectiles++;
            canFireProjectile = true;

            projectileUIManager.UpdateProjectileUI(remainingProjectiles);
        }
        else
        {
            Debug.Log("Projectiles are already at maximum capacity.");
        }
    }
    private void UpdateAttackPointPosition()
    {
        Vector3 joystickDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;


        if (joystickDirection.magnitude > 0.1f)
        {
            meleeAttackPoint.localPosition = joystickDirection * 0.4f;
        }
        else
        {
            meleeAttackPoint.localPosition = new Vector3(playerSprite.flipX ? -0.4f : 0.4f, 0, 0);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (meleeAttackPoint == null || projectileAttackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleeAttackPoint.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(projectileAttackPoint.position, 0.1f);
    }

    public void ResetCombatState()
    {
        remainingProjectiles = maxProjectiles;
        canFireProjectile = true;
        canLightAttack = true;
        canHeavyAttack = true;
        isAimingProjectile = false;
        animator.Play("Idle");
    }
}