using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PunyaEnemy : MonoBehaviour, IDamageable
{
    NavMeshAgent navMash;

    [Header("Status")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Attack")]
    public int damage = 10;
    public float attackRange = 0.5f;
    public float attackCooldown = 5f;
    public float projectileSpeed = 10f;
    public float shootDelay = 1f;
    private bool onAttack = false;

    [Header("Detection")]
    public float detectionRadius = 5f;

    [Header("Move")]
    public float walkSpeed = 2f;
    private bool canMove = true;

    public Transform player;
    public Animator animator;
    public LayerMask playerLayer;
    public Rigidbody rb;
    public bool isDead = false;
    public SpriteRenderer spriteRenderer;

    public GameObject projectilePrefab;
    public Transform firePoint;
    public GameObject reloadItemPrefab;

    private int currentDamageBuff = 25;
    private float originalWalkSpeed;
    private bool isBuffed = false;
    private bool isStunned = false;

    private bool isAttacking = false;
    private bool isHurt = false;
    private bool hasDetectedPlayer = false;
    private bool wasHitByProjectile = false;
    private float lastShotTime = -5f;

    #region Unity Callbacks
    void Start()
    {
        navMash = GetComponent<NavMeshAgent>();
        navMash.updateRotation = false;
        currentHealth = maxHealth;

        originalWalkSpeed = walkSpeed;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("Player not found! Make sure the Player object has the 'Player' tag.");
            }
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
    public void SetWasHitByProjectile(bool value)
    {
        wasHitByProjectile = value;
    }
    void Update()
    {
        if (isDead || isHurt || isStunned) return;

        DetectPlayer();

        if (hasDetectedPlayer)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
            {
                StopWalking();
                if (!onAttack && Time.time >= lastShotTime + attackCooldown)
                {
                    StartCoroutine(AttackWithDelay());
                }
            }
            else if (canMove)
            {
                WalkTowardsPlayer();
            }
        }
    }
    #endregion

    #region Detection Logic
    void DetectPlayer()
    {
        if (!hasDetectedPlayer)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRadius)
            {
                hasDetectedPlayer = true;
            }
        }
    }
    #endregion

    #region Attack Logic
    IEnumerator AttackWithDelay()
    {
        onAttack = true;
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(shootDelay);
        ShootProjectile();
        lastShotTime = Time.time;
        onAttack = false;
    }

    void ShootProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        SpriteRenderer projectileSprite = projectile.GetComponent<SpriteRenderer>();

        Vector3 shootDirection = (player.position - firePoint.position).normalized;

        projectileRb.velocity = new Vector3(shootDirection.x, 0, shootDirection.z) * projectileSpeed;

        if (projectileSprite != null)
        {
            if (shootDirection.x < 0)
            {
                projectileSprite.flipX = true;
            }
            else if (shootDirection.x > 0)
            {
                projectileSprite.flipX = false;
            }
        }
        Debug.Log($"Projectile shot with direction: {shootDirection}, velocity: {projectileRb.velocity}, flipX: {projectileSprite?.flipX}");
    }
    #endregion

    #region Movement Logic
    private void WalkTowardsPlayer()
    {
        if (isAttacking) return;

        animator.SetBool("isWalking", true);
        navMash.speed = walkSpeed;
        navMash.destination = player.position;

        Vector3 direction = player.position - transform.position;
        if (direction.x < 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (direction.x > 0)
        {
            spriteRenderer.flipX = true;
        }

        Debug.Log($"{gameObject.name} is walking towards the player. Flipped: {spriteRenderer.flipX}");
    }
    private void StopWalking()
    {
        animator.SetBool("isWalking", false);
        navMash.speed = 0;

        Vector3 direction = player.position - transform.position;
        if (direction.x < 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (direction.x > 0)
        {
            spriteRenderer.flipX = true;
        }
    }
    #endregion

    #region Hurt, Buff, and Status Effect Logic
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        int totalDamage = damage;
        if (isBuffed)
        {
            totalDamage += currentDamageBuff;
            Debug.Log($"{gameObject.name} is buffed, taking additional damage: {currentDamageBuff}");
        }

        currentHealth -= totalDamage;
        Debug.Log($"{gameObject.name} took {totalDamage} damage. Remaining health: {currentHealth}");

        animator.SetTrigger("Hurt");
        isHurt = true;
        SoundManager.Instance.PlayEnemyHurtSound();
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Invoke("EndHurt", 0.5f);
        }
    }
    public void ApplyBuff(int damageIncrease)
    {
        if (!isBuffed)
        {
            currentDamageBuff += damageIncrease;
            isBuffed = true;
            Debug.Log($"{gameObject.name} is now buffed! Current damage buff: {currentDamageBuff}");
        }
    }

    public void ApplyPermanentSlow(float slowFactor)
    {
        if (!isDead && walkSpeed == originalWalkSpeed)
        {
            walkSpeed *= slowFactor;
            navMash.speed = walkSpeed;
            Debug.Log($"{gameObject.name} slowed permanently. New speed: {walkSpeed}");
        }
    }

    public void Stun(float duration)
    {
        if (!isDead && !isStunned)
        {
            StartCoroutine(StunCoroutine(duration));
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        canMove = false;
        navMash.isStopped = true;

        Debug.Log($"{gameObject.name} stunned for {duration} seconds.");
        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(duration);

        isStunned = false;
        canMove = true;
        navMash.isStopped = false;

        Debug.Log($"{gameObject.name} is no longer stunned.");
    }

    private void EndHurt()
    {
        isHurt = false;
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("Die");
        Debug.Log($"{gameObject.name} has died.");
        SoundManager.Instance.PlayEnemyDieSound();
        if (wasHitByProjectile)
        {
            if (reloadItemPrefab != null)
            {
                Debug.Log("Reload item prefab is assigned. Dropping reload item.");
                DropReloadItem();
            }
            else
            {
                Debug.LogError("Reload item prefab is not assigned! Cannot drop reload item.");
            }
        }

        Invoke("DestroyEnemy", 1.5f);
    }

    private void DropReloadItem()
    {
        if (reloadItemPrefab != null)
        {
            Instantiate(reloadItemPrefab, transform.position, Quaternion.identity);
            Debug.Log($"Dropped reload item at position: {transform.position}");
        }
        else
        {
            Debug.LogError("Reload item prefab is null inside DropReloadItem!");
        }
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(firePoint.position, 0.1f);
        }
    }
    #endregion
}