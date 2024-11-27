using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageObject : MonoBehaviour
{
    private int damage;
    private float lifetime;

    [SerializeField] private LayerMask enemyLayer;

    public void SetDamage(int damageAmount)
    {
        damage = damageAmount;
    }

    public void SetLifetime(float time)
    {
        lifetime = time;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            IDamageable target = other.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }
    }
}