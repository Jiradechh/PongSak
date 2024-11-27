using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaikrokEnemy : MonoBehaviour, IDamageable
{
    #region Public Variables
    public int maxHealth = 100;
    public int currentHealth;
    public int damage = 10;
    public float attackRange = 0.5f;
    public float attackCooldown = 3f;
    public float walkSpeed = 2f;
    public Transform player;
    public Animator animator;
    public LayerMask playerLayer;
    public Rigidbody rb;
    public bool isDead = false;
    public SpriteRenderer spriteRenderer;

    public GameObject projectilePrefab;
    public Transform firePoint;
    #endregion

    #region Private Variables
    private float attackCooldownTimer;
    private bool isAttacking = false;
    private bool isHurt = false;
    private bool hasDetectedPlayer = false;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        currentHealth = maxHealth;
        attackCooldownTimer = attackCooldown;

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

    void Update()
    {
        if (isDead || isHurt) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!hasDetectedPlayer && distanceToPlayer <= attackRange * 2)
        {
            hasDetectedPlayer = true;
        }

        if (hasDetectedPlayer)
        {
            if (distanceToPlayer <= attackRange && attackCooldownTimer <= 0f)
            {
                Shoot();
            }
            else if (distanceToPlayer > attackRange)
            {
                WalkTowardsPlayer();
            }

            if (attackCooldownTimer > 0)
            {
                attackCooldownTimer -= Time.deltaTime;
            }
        }
    }
    #endregion

    #region Attack Logic
    void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        Vector3 shootDirection = (player.position - firePoint.position).normalized;
        projectileRb.velocity = new Vector3(shootDirection.x, 0, shootDirection.z) * 10f;
        attackCooldownTimer = attackCooldown;
        animator.SetTrigger("Attack");
    }
    #endregion

    #region Movement Logic
    private void WalkTowardsPlayer()
    {
        if (isAttacking) return;
        animator.SetBool("isWalking", true);
        Vector3 direction = (player.position - transform.position).normalized;

        if (direction.x > 0)
        {
            spriteRenderer.flipX = false;
            firePoint.localPosition = new Vector3(Mathf.Abs(firePoint.localPosition.x), firePoint.localPosition.y, firePoint.localPosition.z); // Fire point on the right
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = true;
            firePoint.localPosition = new Vector3(-Mathf.Abs(firePoint.localPosition.x), firePoint.localPosition.y, firePoint.localPosition.z); // Fire point on the left
        }

        rb.MovePosition(transform.position + direction * walkSpeed * Time.deltaTime);
    }

    private void StopWalking()
    {
        animator.SetBool("isWalking", false);
    }
    #endregion

    #region Hurt and Death Logic
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage. Remaining health: " + currentHealth);
        animator.SetTrigger("Hurt");
        isHurt = true;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Invoke("EndHurt", 0.5f);
        }
    }

    private void EndHurt()
    {
        isHurt = false;
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        Invoke("DestroyEnemy", 1f);
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

        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(firePoint.position, 0.1f);
        }
    }
    #endregion
}