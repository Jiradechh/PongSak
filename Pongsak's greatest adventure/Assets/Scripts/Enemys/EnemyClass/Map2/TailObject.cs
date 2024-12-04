using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailObject : MonoBehaviour , IDamageable
{
    [Header("TailObject Status")]
    public float health = 100f;
    public float aoeRadius = 3f;
    public int aoeDamage = 15;
    public float attackDelay = 1f;
    public float attackCooldown = 5f;

    [Header("Projectile and Item")]
    public GameObject reloadItemPrefab;
    private bool wasHitByProjectile = false;

    [Header("Buffs")]
    private int currentDamageBuff = 0;
    private bool isBuffed = false;

    private bool isDead = false;
    private bool isAttacking = false;
    private bool isOnCooldown = false;

    [Header("Animator")]
    public Animator animator;

    private bool isIdle = true; // Tracks if the object is idle

    void Update()
    {
        if (health <= 0 && !isDead)
        {
            Die();
        }

        if (!isAttacking && !isOnCooldown && !isDead)
        {
            SetIdle(true);
        }

        CheckPlayerInRange();
    }

    private void CheckPlayerInRange()
    {
        if (isDead || isAttacking || isOnCooldown) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                SetIdle(false);
                StartCoroutine(AttackPlayer(hitCollider.GetComponent<PlayerHealth>()));
                break;
            }
        }
    }

    private IEnumerator AttackPlayer(PlayerHealth playerHealth)
    {
        if (playerHealth == null) yield break;

        isAttacking = true;
        SetIdle(false);

        Debug.Log("Preparing to attack the player...");
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(attackDelay);

        int totalDamage = aoeDamage + currentDamageBuff;
        playerHealth.TakeDamage(totalDamage);
        Debug.Log($"Player took {totalDamage} damage!");

        isAttacking = false;
        isOnCooldown = true;

        Debug.Log($"Entering cooldown for {attackCooldown} seconds.");
        yield return new WaitForSeconds(attackCooldown);
        isOnCooldown = false;
        Debug.Log("Cooldown ended. Ready to attack again.");
    }

    public void TakeDamage(int damage)
    {
        if (!isDead)
        {
            health -= (damage + currentDamageBuff);
            SetIdle(false);

            if (health > 0)
            {
                Debug.Log("TailObject hurt.");
                if (animator != null)
                {
                    animator.SetTrigger("Hurt");
                }
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
            wasHitByProjectile = true;
            currentDamageBuff += damageIncrease;
            isBuffed = true;
            Debug.Log($"TailObject buffed. Current damage buff: {currentDamageBuff}");
        }
    }

    private void Die()
    {
        isDead = true;
        SetIdle(false);
        Debug.Log("TailObject has died.");

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (wasHitByProjectile && reloadItemPrefab != null)
        {
            DropReloadItem();
        }

        Destroy(gameObject, 1f);
    }

    private void DropReloadItem()
    {
        Instantiate(reloadItemPrefab, transform.position, Quaternion.identity);
        Debug.Log("Dropped reload item on TailObject death.");
    }

    private void SetIdle(bool value)
    {
        if (animator != null && isIdle != value)
        {
            isIdle = value;
            animator.SetBool("Idle", isIdle);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}