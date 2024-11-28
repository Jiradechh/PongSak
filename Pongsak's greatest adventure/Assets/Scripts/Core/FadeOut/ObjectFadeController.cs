using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ObjectFadeController : Singleton<ObjectFadeController>
{
    private Transform player;
    private Transform camera;
    public LayerMask obstacleLayer;

    private Dictionary<Renderer, Coroutine> fadeCoroutines = new Dictionary<Renderer, Coroutine>();
    private HashSet<Renderer> currentlyFadedObjects = new HashSet<Renderer>();

    void Start()
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
            Debug.Log("Checking all objects for CinemachineVirtualCamera...");

            foreach (var obj in FindObjectsOfType<CinemachineVirtualCamera>())
            {
                Debug.Log($"Found CinemachineVirtualCamera on object: {obj.gameObject.name}");
            }
        }
    }

    void Update()
    {
        if (player != null && camera != null)
        {
            HandleObjectFade();
        }
    }

    private void HandleObjectFade()
    {
        HashSet<Renderer> newlyFadedObjects = new HashSet<Renderer>();

        Vector3 direction = player.position - camera.position;
        float distance = Vector3.Distance(player.position, camera.position);

        RaycastHit[] hits = Physics.RaycastAll(camera.position, direction, distance, obstacleLayer);

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.collider.transform;
            if (hitTransform.gameObject.layer == LayerMask.NameToLayer("Fade3D"))
            {
                Renderer[] renderers = hitTransform.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    newlyFadedObjects.Add(renderer);

                    if (!currentlyFadedObjects.Contains(renderer))
                    {
                        StartSmoothFade(renderer, 0.3f);
                    }
                }
            }
        }

        foreach (Renderer renderer in currentlyFadedObjects)
        {
            if (!newlyFadedObjects.Contains(renderer))
            {
                StartSmoothFade(renderer, 1f);
            }
        }

        currentlyFadedObjects = newlyFadedObjects;
    }

    private void StartSmoothFade(Renderer renderer, float targetAlpha)
    {
        if (fadeCoroutines.ContainsKey(renderer))
        {
            StopCoroutine(fadeCoroutines[renderer]);
        }

        Coroutine fadeCoroutine = StartCoroutine(FadeObject(renderer, targetAlpha));
        fadeCoroutines[renderer] = fadeCoroutine;
    }

    private IEnumerator FadeObject(Renderer renderer, float targetAlpha)
    {
        Material material = renderer.material;
        float currentAlpha = material.color.a;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(currentAlpha, targetAlpha, elapsed / duration);
            SetObjectAlpha(renderer, newAlpha);
            yield return null;
        }

        SetObjectAlpha(renderer, targetAlpha);

        if (Mathf.Approximately(targetAlpha, 1f))
        {
            fadeCoroutines.Remove(renderer);
        }
    }

    private void SetObjectAlpha(Renderer renderer, float alpha)
    {
        Material material = renderer.material;
        Color color = material.color;
        color.a = alpha;
        material.color = color;
    }
}