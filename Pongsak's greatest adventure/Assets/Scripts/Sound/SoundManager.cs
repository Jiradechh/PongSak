using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [Header("Audio Sources")]
    public AudioSource walkSource;
    public AudioSource effectSource;

    [Header("Audio Clips")]
    public AudioClip walkClip;
    public AudioClip lightAttackClip;
    public AudioClip heavyAttackClip;
    public AudioClip hurtClip;
    public AudioClip dieClip;
    public AudioClip dashClip;
    public AudioClip fireProjectileClip;

    public AudioClip treasureOpenClip;

    public AudioClip enemyHurtClip;
    public AudioClip enemyDieClip;

    public AudioClip woodDieClip;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    [Range(0f, 1f)] public float treasureVolume = 1f; // Add this in SoundManager

    public void PlayTreasureOpenSound()
    {
        if (treasureOpenClip != null && effectSource != null)
        {
            effectSource.PlayOneShot(treasureOpenClip, treasureVolume);
        }
    }
    public void PlayDashSound()
    {
        if (effectSource != null && dashClip != null)
        {
            effectSource.PlayOneShot(dashClip);
        }
        else
        {
            Debug.LogWarning("Dash sound or effect source is missing!");
        }
    }

    public void PlayWalkSound()
    {
        if (!walkSource.isPlaying)
        {
            walkSource.clip = walkClip;
            walkSource.Play();
        }
    }

    public void StopWalkSound()
    {
        if (walkSource.isPlaying)
        {
            walkSource.Stop();
        }
    }
    public void PlayWoodDieSound()
    {
        if (effectSource != null && woodDieClip != null)
        {
            effectSource.PlayOneShot(woodDieClip);
        }
        else
        {
            Debug.LogWarning("Wood die sound or effect source is missing!");
        }
    }
    public void PlayEffect(AudioClip clip)
    {
        if (effectSource != null && clip != null)
        {
            effectSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("AudioSource or AudioClip is missing for the effect!");
        }
    }
    public void PlayEnemyHurtSound()
    {
        if (effectSource != null && enemyHurtClip != null)
        {
            effectSource.PlayOneShot(enemyHurtClip);
        }
        else
        {
            Debug.LogWarning("Enemy hurt sound or effect source is missing!");
        }
    }

    public void PlayEnemyDieSound()
    {
        if (effectSource != null && enemyDieClip != null)
        {
            effectSource.PlayOneShot(enemyDieClip);
        }
        else
        {
            Debug.LogWarning("Enemy die sound or effect source is missing!");
        }
    }

    public void PlayFireProjectileSound()
    {
        if (effectSource != null && fireProjectileClip != null)
        {
            effectSource.PlayOneShot(fireProjectileClip);
        }
        else
        {
            Debug.LogWarning("Fire projectile sound or effect source is missing!");
        }
    }
}