using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    public float meleeAttackCooldown = 1f;
    private float nextAttackTime = 0f;
    private static bool isAnotherEnemyAttacking = false;

    protected override void Start()
    {
        base.Start();
        enemyType = EnemyType.Melee;
    }

    protected override void HandleAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime && !isAnotherEnemyAttacking)
        {
            isAnotherEnemyAttacking = true;
            animator.SetTrigger("Attack");
            player.GetComponent<PlayerHealth>().TakeDamage(25);
            nextAttackTime = Time.time + meleeAttackCooldown;
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(meleeAttackCooldown);

        isAnotherEnemyAttacking = false;
    }
}