using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;      // The enemy prefab to spawn
    public Transform[] spawnPoints;     // Array of possible spawn locations
    public int maxEnemies = 3;          // Maximum number of enemies to keep in the scene

    private int currentEnemies = 0;     // Number of enemies currently in the scene

    void Start()
    {
        // Initially spawn enemies until there are 
        SpawnEnemies();
    }

    void Update()
    {
        // Continuously check and spawn enemies until there are 3 in the scene
        CheckAndSpawnEnemies();
    }

    // Spawns an enemy at a random spawn point
    private void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;

        // Randomly select a spawn point
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnLocation = spawnPoints[randomIndex];

        // Instantiate the enemy
        Instantiate(enemyPrefab, spawnLocation.position, spawnLocation.rotation);
        currentEnemies++;
    }

    // Spawns enemies until the number of enemies reaches the max limit
    private void SpawnEnemies()
    {
        while (currentEnemies < maxEnemies)
        {
            SpawnEnemy();
        }
    }

    // Checks how many enemies are in the scene and spawns new ones if needed
    private void CheckAndSpawnEnemies()
    {
        currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (currentEnemies < maxEnemies)
        {
            SpawnEnemies();
        }
    }
}