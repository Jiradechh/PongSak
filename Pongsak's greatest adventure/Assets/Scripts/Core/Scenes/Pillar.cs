using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pillar : MonoBehaviour, IDamageable
{
    [Header("Pillar Settings")]
    [SerializeField] private int maxStage = 3;
    [SerializeField] private Sprite[] pillarStages;
    [SerializeField] private int[] healthPerStage = { 50, 50 };

    [Header("Damage Object Settings")]
    [SerializeField] private GameObject damageObjectPrefab;
    [SerializeField] private float spawnRadius = 3f;
    [SerializeField] private int objectDamage = 30;
    [SerializeField] private float objectLifetime = 3f;

    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem hitParticleEffect;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip hitSound;
    private AudioSource audioSource;

    private int currentStage = 0;
    private int currentHealth;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = healthPerStage[currentStage];
        UpdatePillarSprite();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void TakeDamage(int damage)
    {
        if (currentStage >= maxStage - 1)
        {
            Debug.Log("Pillar is in stage 3 and cannot be damaged.");
            return;
        }

        currentHealth -= damage;

        Debug.Log($"Pillar took {damage} damage. Current Stage: {currentStage + 1}, Remaining Health: {currentHealth}");

        PlayHitSound(); 
        StartCoroutine(ShakeAndPlayParticle(0.5f));

        SpawnRandomDamageObjects();

        if (currentHealth <= 0)
        {
            if (currentStage < maxStage - 1)
            {
                AdvanceStage();
            }
            else
            {
                EnterIndestructibleStage();
            }
        }
    }

    private void PlayHitSound()
    {
        if (hitSound != null && audioSource != null)
        {
            audioSource.clip = hitSound;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Hit sound or AudioSource is not set up.");
        }
    }

    private IEnumerator ShakeAndPlayParticle(float duration)
    {
        PlayHitParticleEffect();

        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0.0f;
        float magnitude = 0.1f;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.5f, 0.5f) * magnitude;
            float y = Random.Range(-0.5f, 0.5f) * magnitude;

            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    private void PlayHitParticleEffect()
    {
        if (hitParticleEffect != null)
        {
            hitParticleEffect.Play();
        }
        else
        {
            Debug.LogWarning("Hit Particle Effect is not assigned.");
        }
    }

    private void SpawnRandomDamageObjects()
    {
        int numberOfObjects = Random.Range(1, 4);

        for (int i = 0; i < numberOfObjects; i++)
        {
            Vector3 randomPosition = transform.position + (Random.insideUnitSphere * spawnRadius);
            randomPosition.y = transform.position.y;

            GameObject damageObject = Instantiate(damageObjectPrefab, randomPosition, Quaternion.identity);

            DamageObject doScript = damageObject.GetComponent<DamageObject>();
            if (doScript != null)
            {
                doScript.SetDamage(objectDamage);
                doScript.SetLifetime(objectLifetime);
            }
        }
    }

    private void AdvanceStage()
    {
        currentStage++;
        currentHealth = healthPerStage[currentStage];
        UpdatePillarSprite();
    }

    private void EnterIndestructibleStage()
    {
        Debug.Log("Pillar has entered stage 3 and is now indestructible.");
        currentStage = maxStage - 1;
        currentHealth = int.MaxValue;
        UpdatePillarSprite();
    }

    private void UpdatePillarSprite()
    {
        if (currentStage < pillarStages.Length)
        {
            spriteRenderer.sprite = pillarStages[currentStage];
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}