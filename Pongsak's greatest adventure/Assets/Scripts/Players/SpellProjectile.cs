using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    public GameObject reloadItemPrefab;
    public GameObject aoeEffectPrefab;
    private bool isAttachedToEnemy = false;
    private int damageBuff = 25;
    private Rigidbody rb;

    public float rotationSpeed = 360f;
    private bool slowEffectEnabled = false;
    private bool aoeEnabled = false;
    public float aoeRadius = 3f;
    public int aoeDamage = 20;

    private bool stunEffectEnabled = false;
    public float stunDuration = 2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 direction)
    {
        if (rb != null)
        {
            rb.velocity = direction;
            Debug.Log("Projectile fired in direction: " + direction + " with velocity: " + rb.velocity);
        }
        else
        {
            Debug.LogError("Rigidbody component missing on SpellProjectile.");
        }
    }

    void Update()
    {
        RotateProjectileBasedOnXMovement();
    }

    public void EnableSlowEffect()
    {
        slowEffectEnabled = true;
        Debug.Log("Slow effect enabled.");
    }

    public void EnableAOE()
    {
        aoeEnabled = true;
        Debug.Log("AOE enabled for this projectile.");
    }
    public void EnableStun()
    {
        stunEffectEnabled = true;
        Debug.Log("Stun effect enabled for this projectile.");
    }


    public void RotateProjectileBasedOnXMovement()
    {
        if (rb.velocity != Vector3.zero)
        {
            if (rb.velocity.x > 0)
            {
                transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
            }
            else if (rb.velocity.x < 0)
            {
                transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isAttachedToEnemy) return;

        if (other.gameObject.CompareTag("Wall"))
        {
            DropReloadItem();
            if (aoeEnabled) TriggerAOE();
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss"))
        {
            AttachToEnemy(other.gameObject);
            if (aoeEnabled) TriggerAOE();
            if (stunEffectEnabled)
            {
                Enemy enemyComponent = other.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    enemyComponent.Stun(stunDuration);
                    Debug.Log($"{other.name} stunned for {stunDuration} seconds.");
                }
                else
                {
                    Debug.LogWarning($"No Enemy component found on {other.name}. Cannot apply stun.");
                }
            }
            Destroy(gameObject);
        }
    }
    private void TriggerAOE()
    {
        
        if (aoeEffectPrefab != null)
        {
            Instantiate(aoeEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider[] hitTargets = Physics.OverlapSphere(transform.position, aoeRadius, LayerMask.GetMask("Enemy", "Boss"));
        foreach (Collider target in hitTargets)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(aoeDamage);
                Debug.Log($"AOE damage applied to {target.name}: {aoeDamage} damage.");
            }
        }
    }
    private void AttachToEnemy(GameObject enemy)
    {
        isAttachedToEnemy = true;
        transform.parent = enemy.transform;

        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        BirdsBossEnemy bossComponent = enemy.GetComponent<BirdsBossEnemy>();
        TailObject tailObjectComponent = enemy.GetComponent<TailObject>();
        SharkBoss sharkBossComponent =  enemy.GetComponent<SharkBoss>();
        DoritosBoss doritosComponent = enemy.GetComponent<DoritosBoss>();

        if (enemyComponent != null)
        {
            enemyComponent.ApplyBuff(damageBuff);
            Debug.Log("Projectile attached to enemy: " + enemy.name);

            if (slowEffectEnabled)
            {
                Debug.Log("Applying permanent slow to enemy: " + enemy.name);
                enemyComponent.ApplyPermanentSlow(0.5f);
            }
        }
        else if (bossComponent != null)
        {
            bossComponent.ApplyBuff(damageBuff);
            Debug.Log("Projectile attached to boss: " + enemy.name);
        }
        else if (tailObjectComponent != null)
        {
            tailObjectComponent.ApplyBuff(damageBuff);
            Debug.Log("Projectile attached to boss: " + enemy.name);
        }
        else if (sharkBossComponent != null)
        {
            sharkBossComponent.ApplyBuff(damageBuff);
            Debug.Log("Projectile attached to boss: " + enemy.name);
        }
        else if (doritosComponent != null)
        {
            doritosComponent.ApplyBuff(damageBuff);
            Debug.Log("Projectile attached to boss: " + enemy.name);
        }
        PunyaEnemy punyaEnemyComponent = enemy.GetComponent<PunyaEnemy>();
        
        if (punyaEnemyComponent != null)
        {
            punyaEnemyComponent.SetWasHitByProjectile(true);
            Debug.Log($"{enemy.name} was hit by a projectile. Setting wasHitByProjectile to true.");

            if (slowEffectEnabled)
            {
                Debug.Log($"Applying slow effect to {enemy.name}");
                punyaEnemyComponent.ApplyPermanentSlow(0.5f);
            }

            if (stunEffectEnabled)
            {
                Debug.Log($"Stunning {enemy.name} for {stunDuration} seconds.");
                punyaEnemyComponent.Stun(stunDuration);
            }
        }
        SaikrokEnemy saikrokEnemy = enemy.GetComponent<SaikrokEnemy>();

        if (saikrokEnemy != null)
        {
            saikrokEnemy.SetWasHitByProjectile(true);
            Debug.Log($"{enemy.name} was hit by a projectile. Setting wasHitByProjectile to true.");

            if (slowEffectEnabled)
            {
                Debug.Log($"Applying slow effect to {enemy.name}");
                punyaEnemyComponent.ApplyPermanentSlow(0.5f);
            }

            if (stunEffectEnabled)
            {
                Debug.Log($"Stunning {enemy.name} for {stunDuration} seconds.");
                punyaEnemyComponent.Stun(stunDuration);
            }
        }
    }

    private void DropReloadItem()
    {
        if (reloadItemPrefab != null)
        {
            Instantiate(reloadItemPrefab, transform.position, Quaternion.identity);
            Debug.Log("Dropping reload item at position: " + transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
      
    }
}