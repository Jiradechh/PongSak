using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using MyInterface;

public enum EnemyType
{
    Melee,
    Ranged,
}

public class Enemy : MonoBehaviour, IDamageable
{
    #region Variables
    NavMeshAgent navMash;

    [Header("Status")]
    public EnemyType enemyType;
    public float health = 100f;

    [Header("Move")]
    public float moveSpeed = 2f;
    public bool canMove = true;

    [Header("Knock")]
    public float knockbackForce;
    public float durationKnockback;

    [Header("Attack")]
    public int attackDamage = 10;
    public float attackRange = 1f;
    public float delayAttack = 1f;
    public float detectionRange = 5f;
    public float rangedAttackRange = 3f;
    float distanceToPlayer;
    public GameObject reloadItemPrefab;
    bool onAttack = false;

    [Header("Die")]
    private int currentDamageBuff = 0;
    private bool isBuffed = false;
    private bool isSlowed = false;
    private bool wasHitByProjectile = false;
    private bool hasDetectedPlayer = false;
    private bool isStunned = false;

    private Rigidbody rigidbody3D;
    private Collider collider3D;
    protected Animator animator;
    private SpriteRenderer spriteRenderer;
    protected Transform player;
    private bool isDead = false;

    private PlayerHealth playerHealth;
    private float attackCooldown;
    private float nextAttackTime = 0;
    private float initialYPosition;

    public bool IsDead => isDead;
    #endregion

    #region Unity Methods
    protected virtual void Start()
    {
        navMash = GetComponent<NavMeshAgent>();
        navMash.updateRotation = false;
        navMash.speed = moveSpeed;
        navMash.acceleration = 4f;
        navMash.angularSpeed = 120f;

        player = GameObject.FindWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        rigidbody3D = GetComponent<Rigidbody>();
        collider3D = GetComponent<Collider>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rigidbody3D.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        initialYPosition = transform.position.y;
        attackCooldown = Random.Range(0.5f, 1.5f);
    }

    protected virtual void Update()
    {
        if (!isDead)
        {
            Vector3 lockedPosition = transform.position;
            lockedPosition.y = initialYPosition;
            transform.position = lockedPosition;

            HandleGroupBehavior();
        }
    }
    #endregion

    #region Damage Methods
    public void ApplyKnockback(Vector3 knockbackDirection, float knockbackForce, float duration = 0.2f)
    {
        if (!isDead)
        {
            StartCoroutine(KnockbackCoroutine(knockbackDirection * knockbackForce, duration));
        }
    }

    private IEnumerator KnockbackCoroutine(Vector3 knockbackDistance, float duration)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + knockbackDistance;
        endPosition.y = startPosition.y;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        Debug.Log("Enemy smoothly knocked back to position: " + endPosition);
    }

    public void ApplyPermanentSlow(float slowFactor)
    {
        if (!isDead && !isSlowed)
        {
            moveSpeed *= slowFactor;
            isSlowed = true;
            Debug.Log("Enemy permanently slowed. New speed: " + moveSpeed);

            navMash.speed = moveSpeed;
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
        navMash.speed = 0;

        Debug.Log($"{gameObject.name} is stunned for {duration} seconds.");
        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(duration);

        isStunned = false;
        canMove = true;

        navMash.isStopped = false;
        navMash.speed = moveSpeed;
        Debug.Log($"{gameObject.name} is no longer stunned.");
    }

    public void ApplyBuff(int damageIncrease)
    {
        if (!isBuffed)
        {
            currentDamageBuff += damageIncrease;
            isBuffed = true;
            wasHitByProjectile = true;
            Debug.Log("Enemy buffed. Extra damage: " + currentDamageBuff);
        }
    }

    public void TakeDamage(int damage)
    {
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth component not found on player!");
        }
        if (!isDead)
        {
            health -= (damage + currentDamageBuff);

            if (health > 0)
            {
                StartCoroutine(KnockBack());
                animator.SetTrigger("Hurt");
                SoundManager.Instance.PlayEnemyHurtSound();
            }
            else
            {
                health = 0;
                Die();
            }
            ResetEnemyState();
        }
    }

    private IEnumerator KnockBack()
    {
        if (!isDead)
        {
            canMove = false;
            if (navMash != null)
            {
                navMash.isStopped = true;
            }

            Vector3 knockbackDirection = transform.right * knockbackForce;
            Vector3 startPosition = transform.position;
            Vector3 endPosition = startPosition + knockbackDirection;
            endPosition.y = startPosition.y;

            float elapsedTime = 0f;

            while (elapsedTime < durationKnockback)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / durationKnockback);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = new Vector3(endPosition.x, startPosition.y, endPosition.z);

            if (navMash != null)
            {
                navMash.isStopped = false;
            }
            canMove = true;
        }
    }
    #endregion

    #region Death Methods
    protected virtual void Die()
    {
        if (!isDead)
        {
            isDead = true;
            animator.SetTrigger("Die");
            canMove = false;

            SoundManager.Instance.PlayEnemyDieSound();

            if (navMash != null)
            {
                navMash.isStopped = true;
                navMash.speed = 0;
            }

            if (rigidbody3D != null)
            {
                rigidbody3D.velocity = Vector3.zero;
                rigidbody3D.angularVelocity = Vector3.zero;
                rigidbody3D.isKinematic = true;
            }
            if (collider3D != null)
            {
                collider3D.enabled = false;
            }
            if (wasHitByProjectile && reloadItemPrefab != null)
            {
                DropReloadItem();
            }

            Debug.Log($"{gameObject.name} has died and stopped moving.");

            Invoke("DestroyEnemy", 1.5f);
        }
    }

    private void DropReloadItem()
    {
        Instantiate(reloadItemPrefab, transform.position, Quaternion.identity);
        Debug.Log("Dropped reload item on enemy death.");
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
    #endregion

    #region Behavior Methods
    private void HandleGroupBehavior()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && !hasDetectedPlayer)
        {
            hasDetectedPlayer = true;
            Debug.Log("Enemy detected the player and will continue to follow.");
        }

        if (hasDetectedPlayer)
        {
            if (!isDead)
            {
                if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime && !onAttack)
                {
                    StartCoroutine(StopMoveAttack());
                }
                else if (distanceToPlayer > attackRange)
                {
                    if (canMove)
                    {
                        MoveTowardsPlayer();
                    }
                }
                else
                {
                    animator.SetBool("isWalking", false);
                }
            }
        }
    }

    private IEnumerator StopMoveAttack()
    {
        Debug.Log("Attack coroutine started.");
        onAttack = true;
        canMove = false;
        navMash.speed = 0;
        animator.Play("Idle");
        yield return new WaitForSeconds(delayAttack);
        Debug.Log("Attempting to attack the player.");
        animator.SetTrigger("Attack");
        nextAttackTime = Time.time + attackCooldown;
    }

    protected virtual void HandleAttack()
    {
        if (playerHealth != null && !isDead)
        {
            if (distanceToPlayer <= attackRange)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    protected virtual void MoveTowardsPlayer()
    {
        if (!isDead && canMove)
        {
            if (Mathf.Abs(navMash.speed - moveSpeed) > 0.01f)
            {
                navMash.speed = Mathf.Lerp(navMash.speed, moveSpeed, Time.deltaTime * 2f);
            }

            FlipSprite(player.position.x);
            navMash.destination = player.position;
            animator.SetBool("isWalking", true);
        }
    }

    private void ResetEnemyState()
    {
        canMove = true;
        onAttack = false;
        if (navMash != null)
        {
            navMash.isStopped = false;
            navMash.speed = moveSpeed;
        }
        Debug.Log("Enemy state reset: canMove = " + canMove + ", onAttack = " + onAttack);
    }

    protected void FlipSprite(float playerPositionX)
    {
        spriteRenderer.flipX = playerPositionX < transform.position.x ? false : true;
    }

    void FollowPlayer()
    {
        onAttack = false;
        canMove = true;
        navMash.speed = 1;
    }
    #endregion

    #region Debugging Methods
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}