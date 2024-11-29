using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SharkBoss : MonoBehaviour, IDamageable
{
    [Header("General Settings")]
    public float maxHealth = 200f;
    public float moveSpeed = 3f;
    public GameObject reloadItemPrefab;

    [Header("Associated Objects")]
    public GameObject husharkwater; 
    private SpriteRenderer husharkwaterSpriteRenderer;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public int attackDamage = 20;
    public float attackCooldown = 1.5f;
    public float delayAttack = 0.5f;

    [Header("Tail Spawn Settings")]
    public GameObject tailPrefab;
    public Transform[] tailSpawnPositions;
    public float tailSpawnCooldown = 5f;

    [Header("Retreat Settings")]
    public Transform[] retreatPoints;
    public float retreatDuration = 4f;
    public float retreatInterval = 10f;

    private Rigidbody rigidbody3D;
    private Transform player;
    private PlayerHealth playerHealth;
    private float health;
    private bool isDead = false;
    private bool isRetreating = false;
    private bool onAttack = false;
    private bool canMove = true;
    private bool wasHitByProjectile = false;
    private bool isBuffed = false;
    private int currentDamageBuff = 25;
    private float nextAttackTime = 0f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rigidbody3D = GetComponent<Rigidbody>();
        player = GameObject.FindWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        husharkwaterSpriteRenderer = husharkwater.GetComponent<SpriteRenderer>();
        health = maxHealth;

        StartCoroutine(SpawnTailsRoutine());
        StartCoroutine(RetreatRoutine());

        rigidbody3D.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
    }

    void Update()
    {
        if (!isDead && !isRetreating && player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime && !onAttack)
            {
                StartCoroutine(StopMoveAttack());
            }
            else if (distanceToPlayer > attackRange && canMove)
            {
                MoveTowardsPlayer();
            }

            FlipSprite(player.position.x - transform.position.x);
        }
    }

    private void MoveTowardsPlayer()
    {
        if (!isDead && canMove)
        {
            Vector3 direction = (player.position - transform.position).normalized;

            rigidbody3D.velocity = new Vector3(direction.x * moveSpeed, 0, direction.z * moveSpeed);
        }
    }

    private IEnumerator StopMoveAttack()
    {
        onAttack = true;
        canMove = false;
        rigidbody3D.velocity = Vector3.zero;
        yield return new WaitForSeconds(delayAttack);
        HandleAttack();
        nextAttackTime = Time.time + attackCooldown;
        onAttack = false;
        canMove = true;
    }

    protected virtual void HandleAttack()
    {
        if (playerHealth != null && !isDead)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"SharkBoss attacks the player for {attackDamage} damage!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (!isDead)
        {
            health -= (damage + currentDamageBuff);
            SoundManager.Instance.PlayEnemyHurtSound();
            if (health > 0)
            {
                Debug.Log($"SharkBoss took {damage} damage, current health: {health}");
            }
            else
            {
                health = 0;
                Die();
            }
        }
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

    private void Die()
    {
        isDead = true;
        rigidbody3D.velocity = Vector3.zero;
        Debug.Log("SharkBoss has been defeated!");
        SoundManager.Instance.PlayEnemyDieSound();
        if (wasHitByProjectile && reloadItemPrefab != null)
        {
            DropReloadItem();
        }

        Destroy(gameObject, 2f);
    }

    private void DropReloadItem()
    {
        Instantiate(reloadItemPrefab, transform.position, Quaternion.identity);
        Debug.Log("Dropped reload item on SharkBoss death.");
    }

    private IEnumerator SpawnTailsRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(tailSpawnCooldown);

            if (tailSpawnPositions.Length > 0)
            {
                SpawnRandomTails();
            }
        }
    }

    private void SpawnRandomTails()
    {
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, tailSpawnPositions.Length);
            Transform spawnPosition = tailSpawnPositions[randomIndex];
            Instantiate(tailPrefab, spawnPosition.position, spawnPosition.rotation);
        }

        Debug.Log("SharkBoss spawned 3 tails!");
    }

    private IEnumerator RetreatRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(retreatInterval);

            if (retreatPoints.Length > 0)
            {
                isRetreating = true;
                canMove = false;
                rigidbody3D.velocity = Vector3.zero;

                Transform retreatPoint = retreatPoints[Random.Range(0, retreatPoints.Length)];
                Debug.Log("SharkBoss is retreating.");

                yield return StartCoroutine(SmoothMoveToPositionWithFlip(retreatPoint.position, retreatDuration));

                Debug.Log("SharkBoss is resting at the retreat point.");
                yield return new WaitForSeconds(4f);

                isRetreating = false;
                canMove = true;
                Debug.Log("SharkBoss finished retreating.");
            }
        }
    }

    private IEnumerator SmoothMoveToPositionWithFlip(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;

            FlipSprite(direction.x);

            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = targetPosition;
    }
    private void FlipSprite(float directionX)
    {
        bool shouldFlip = directionX > 0; 

        spriteRenderer.flipX = shouldFlip;

        if (husharkwaterSpriteRenderer != null)
        {
            husharkwaterSpriteRenderer.flipX = shouldFlip;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}