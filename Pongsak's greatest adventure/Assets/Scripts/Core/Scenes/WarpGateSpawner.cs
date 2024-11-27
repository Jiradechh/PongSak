using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpGateSpawner : MonoBehaviour
{
    public GameObject warpGatePrefab;
    public GameObject treasurePrefab;
    public Transform warpGateSpawnLocation;
    public Transform treasureSpawnLocation;

    private bool warpGateSpawned = false;
    private bool treasureSpawned = false;

    void Update()
    {
        if (CheckAllEnemiesDefeated())
        {
            if (!warpGateSpawned)
            {
                SpawnWarpGate();
                warpGateSpawned = true;
            }

            if (!treasureSpawned && CanSpawnTreasure())
            {
                SpawnTreasure();
                treasureSpawned = true;
            }
        }
    }

    private bool CheckAllEnemiesDefeated()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length == 0;
    }

    private void SpawnWarpGate()
    {
        Instantiate(warpGatePrefab, warpGateSpawnLocation.position, warpGateSpawnLocation.rotation);
        Debug.Log("WarpGate spawned.");
    }

    private void SpawnTreasure()
    {
        Instantiate(treasurePrefab, treasureSpawnLocation.position, treasureSpawnLocation.rotation);
        Debug.Log("Treasure spawned.");
    }

    private bool CanSpawnTreasure()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        return currentSceneName != "Lobby" &&
               currentSceneName != "ShopScene1" &&
               currentSceneName != "ShopScene2" &&
               currentSceneName != "ShopScene3";
    }
}