using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileUIManager : MonoBehaviour
{
    public GameObject projectileCirclePrefab;
    public Transform projectileContainer;
    private int currentProjectileCount = 0;

    public void UpdateProjectileUI(int remainingProjectiles)
    {
        foreach (Transform child in projectileContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < remainingProjectiles; i++)
        {
            Instantiate(projectileCirclePrefab, projectileContainer);
        }

        currentProjectileCount = remainingProjectiles;
    }
}