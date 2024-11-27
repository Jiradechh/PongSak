using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutWhenPlayerBehind : MonoBehaviour
{
    public Transform player;
    public float fadeSpeed = 2f;  // Speed of the fade transition
    public float fadedAlpha = 0.3f;  // Target alpha when faded out

    private Renderer[] childRenderers;  // To store the renderers on child objects
    private Color[][] originalColors;  // To store original colors for each material of each renderer
    private bool isFaded = false;

    void Start()
    {
        // Find the player by tag if not assigned
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        // Get all renderers in child objects (Edge and Face)
        childRenderers = GetComponentsInChildren<Renderer>();

        // Store each material's original color
        originalColors = new Color[childRenderers.Length][];
        for (int i = 0; i < childRenderers.Length; i++)
        {
            Material[] materials = childRenderers[i].materials;
            originalColors[i] = new Color[materials.Length];
            for (int j = 0; j < materials.Length; j++)
            {
                originalColors[i][j] = materials[j].color;
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        // Raycast from the player towards the camera to check if this object is hit
        Ray ray = new Ray(player.position, Camera.main.transform.position - player.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                FadeOutObject();
            }
            else
            {
                FadeInObject();
            }
        }
        else
        {
            FadeInObject();
        }
    }

    void FadeOutObject()
    {
        if (!isFaded)
        {
            bool allFaded = true;  // Track if all materials are fully faded

            for (int i = 0; i < childRenderers.Length; i++)
            {
                Material[] materials = childRenderers[i].materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    Color color = materials[j].color;
                    color.a = Mathf.Lerp(color.a, fadedAlpha, Time.deltaTime * fadeSpeed);
                    materials[j].color = color;

                    if (color.a > fadedAlpha + 0.01f)
                    {
                        allFaded = false;
                    }
                }
            }

            isFaded = allFaded;
        }
    }

    void FadeInObject()
    {
        if (isFaded)
        {
            bool allOpaque = true;  // Track if all materials are fully opaque

            for (int i = 0; i < childRenderers.Length; i++)
            {
                Material[] materials = childRenderers[i].materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    Color color = materials[j].color;
                    color.a = Mathf.Lerp(color.a, originalColors[i][j].a, Time.deltaTime * fadeSpeed);
                    materials[j].color = color;

                    if (color.a < originalColors[i][j].a - 0.01f)
                    {
                        allOpaque = false;
                    }
                }
            }

            isFaded = !allOpaque;
        }
    }
}