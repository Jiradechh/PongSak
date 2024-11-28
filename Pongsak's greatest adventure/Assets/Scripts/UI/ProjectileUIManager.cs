using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileUIManager : MonoBehaviour
{
    public GameObject projectileCirclePrefab;
    public Transform projectileContainer;
    private int currentProjectileCount = 0;

    /// <summary>
    /// Updates the projectile UI to reflect the current remaining projectiles.
    /// </summary>
    /// <param name="remainingProjectiles">The number of remaining projectiles.</param>
    public void UpdateProjectileUI(int remainingProjectiles)
    {
        // Log the number of projectiles to debug potential issues
        Debug.Log($"Updating projectile UI. Remaining projectiles: {remainingProjectiles}");

        // Clear the existing projectile UI
        foreach (Transform child in projectileContainer)
        {
            Destroy(child.gameObject);
        }

        // Instantiate the circles for the remaining projectiles
        for (int i = 0; i < remainingProjectiles; i++)
        {
            Instantiate(projectileCirclePrefab, projectileContainer);
        }

        currentProjectileCount = remainingProjectiles;
    }

    /// <summary>
    /// Synchronizes the projectile UI on game start or scene load.
    /// </summary>
    public void InitializeProjectileUI(int maxProjectiles)
    {
        Debug.Log($"Initializing projectile UI with max projectiles: {maxProjectiles}");
        UpdateProjectileUI(maxProjectiles);
    }
}