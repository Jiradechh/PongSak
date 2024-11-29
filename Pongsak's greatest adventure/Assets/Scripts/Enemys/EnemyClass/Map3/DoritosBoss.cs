using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoritosBoss : MonoBehaviour , IDamageable
{
    #region Public Variables
    public Transform point1;
    public Transform retreatPoint;

    public float moveSpeed = 2f;
    public GameObject[] attackPrefabs;
    public GameObject reloadItemPrefab;

    public SpriteRenderer spriteRenderer;

    public int maxHealth = 100;
    private int currentHealth;

    public bool isDead = false;
    #endregion

    #region Private Variables
    private Transform currentTarget;
    private int currentDamageBuff = 0;
    private bool isBuffed = false;
    private bool wasHitByProjectile = false;
    private bool isAtRetreat = false;
    private bool isAttacking = false;
    private float attackInterval = 2f;
    private float nextAttackTime = 0f;
    private int lastAttackIndex = -1;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        currentHealth = maxHealth;
        currentTarget = point1;
        StartCoroutine(SwitchTargetsRoutine());
    }

    private void Update()
    {
        if (isDead) return;

        MoveToCurrentTarget();

        if (!isAtRetreat && Time.time >= nextAttackTime && !isAttacking)
        {
            StartCoroutine(ExecuteRandomAttack());
            nextAttackTime = Time.time + attackInterval;
        }
    }
    #endregion

    #region Damage Logic
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= (damage + currentDamageBuff);
        Debug.Log(gameObject.name + " took " + damage + " damage. Remaining health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplyBuff(int damageIncrease)
    {
        if (!isBuffed)
        {
            currentDamageBuff += damageIncrease;
            isBuffed = true;
            wasHitByProjectile = true;
            Debug.Log("Boss buffed. Extra damage: " + currentDamageBuff);
        }
    }

    private void Die()
    {
        Debug.Log("Boss has died!");
        isDead = true;
        currentTarget = null;

        if (wasHitByProjectile && reloadItemPrefab != null)
        {
            DropReloadItem();
        }

        Destroy(gameObject, 1f);
    }

    private void DropReloadItem()
    {
        Instantiate(reloadItemPrefab, transform.position, Quaternion.identity);
        Debug.Log("Dropped reload item on boss death.");
    }
    #endregion

    #region Movement Logic
    private void MoveToCurrentTarget()
    {
        if (isDead || currentTarget == null) return;

        float step = moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, currentTarget.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, step);
            FlipSprite();
        }
    }

    private void FlipSprite()
    {
        if (currentTarget.position.x > transform.position.x && !spriteRenderer.flipX)
        {
            spriteRenderer.flipX = true;
        }
        else if (currentTarget.position.x < transform.position.x && spriteRenderer.flipX)
        {
            spriteRenderer.flipX = false;
        }
    }

    private IEnumerator SwitchTargetsRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(5f);
            currentTarget = isAtRetreat ? point1 : retreatPoint;
            isAtRetreat = !isAtRetreat;
        }
    }
    #endregion

    #region Attack Logic
    private IEnumerator ExecuteRandomAttack()
    {
        if (attackPrefabs.Length == 0) yield break;

        isAttacking = true;
        Debug.Log("Boss is attacking!");

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, attackPrefabs.Length);
        } while (randomIndex == lastAttackIndex);

        lastAttackIndex = randomIndex;
        GameObject selectedPrefab = attackPrefabs[randomIndex];
        Instantiate(selectedPrefab, transform.position, Quaternion.identity);

        Debug.Log($"Boss spawned {selectedPrefab.name}");

        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }
    #endregion
}