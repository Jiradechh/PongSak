using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileKob : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;
    private Rigidbody rb;
    private float lifespan = 5f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing; please attach it to the projectile prefab.");
            Destroy(gameObject);
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component is missing; please attach it to the projectile prefab.");
            Destroy(gameObject);
            return;
        }
        if (rb.velocity.x < 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (rb.velocity.x > 0)
        {
            spriteRenderer.flipX = true;
        }

        Destroy(gameObject, lifespan);
    }

    public void SetDirection(Vector3 direction)
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing; cannot set velocity.");
            return;
        }
        if (Mathf.Abs(direction.x) > 0.01f)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
        else
        {
            Debug.LogWarning("Direction x-component is near zero; flipping may not apply.");
        }
        rb.velocity = direction.normalized * speed;

        Debug.Log($"Projectile fired with direction: {direction}, velocity: {rb.velocity}, flipped: {spriteRenderer.flipX}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}