using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SpriteFadeController : MonoBehaviour
{
    private Transform player;
    private Transform camera;
    public string obstacleName = "Pillar";

    private Dictionary<SpriteRenderer, Coroutine> fadeCoroutines = new Dictionary<SpriteRenderer, Coroutine>();
    private HashSet<SpriteRenderer> currentlyFadedSprites = new HashSet<SpriteRenderer>();

    private void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the Player object has the 'Player' tag.");
        }

        CinemachineVirtualCamera cinemachineCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (cinemachineCamera != null)
        {
            camera = cinemachineCamera.transform;
        }
        else
        {
            Debug.LogError("Cinemachine Virtual Camera not found! Make sure there is a Cinemachine camera in the scene.");
        }
    }

    private void Update()
    {
        if (player != null && camera != null)
        {
            HandleSpriteFade();
        }
    }

    private void HandleSpriteFade()
    {
        HashSet<SpriteRenderer> newlyFadedSprites = new HashSet<SpriteRenderer>();

        Vector3 direction = player.position - camera.position;
        float distance = Vector3.Distance(player.position, camera.position);

        RaycastHit[] hits = Physics.RaycastAll(camera.position, direction, distance);

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.collider.transform;
            if (hitTransform.gameObject.name == obstacleName)
            {
                SpriteRenderer spriteRenderer = hitTransform.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    newlyFadedSprites.Add(spriteRenderer);

                    if (!currentlyFadedSprites.Contains(spriteRenderer))
                    {
                        StartSmoothFade(spriteRenderer, 0.3f);
                    }
                }
            }
        }

        foreach (SpriteRenderer sprite in currentlyFadedSprites)
        {
            if (!newlyFadedSprites.Contains(sprite))
            {
                StartSmoothFade(sprite, 1f);
            }
        }

        currentlyFadedSprites = newlyFadedSprites;
    }

    private void StartSmoothFade(SpriteRenderer spriteRenderer, float targetAlpha)
    {
        if (fadeCoroutines.ContainsKey(spriteRenderer))
        {
            StopCoroutine(fadeCoroutines[spriteRenderer]);
        }

        Coroutine fadeCoroutine = StartCoroutine(FadeSprite(spriteRenderer, targetAlpha));
        fadeCoroutines[spriteRenderer] = fadeCoroutine;
    }

    private IEnumerator FadeSprite(SpriteRenderer spriteRenderer, float targetAlpha)
    {
        if (spriteRenderer == null)
            yield break; // Exit the coroutine if the SpriteRenderer is null

        float currentAlpha = spriteRenderer.color.a;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (spriteRenderer == null) // Check if the SpriteRenderer has been destroyed
                yield break;

            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(currentAlpha, targetAlpha, elapsed / duration);
            SetSpriteAlpha(spriteRenderer, newAlpha);
            yield return null;
        }

        if (spriteRenderer != null) // Final null check before setting the target alpha
            SetSpriteAlpha(spriteRenderer, targetAlpha);

        if (Mathf.Approximately(targetAlpha, 1f))
        {
            fadeCoroutines.Remove(spriteRenderer);
        }
    }

    private void SetSpriteAlpha(SpriteRenderer spriteRenderer, float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}