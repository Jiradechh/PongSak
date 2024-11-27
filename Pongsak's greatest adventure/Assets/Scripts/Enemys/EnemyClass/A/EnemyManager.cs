using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public int enemyCount = 5;

    private List<Enemy> enemies = new List<Enemy>();

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            enemies.Add(enemy);
        }
    }

    void Update()
    {
        ManageEnemies();
    }

    private void ManageEnemies()
    {
        List<Enemy> enemies = new List<Enemy>(FindObjectsOfType<Enemy>());

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i].IsDead)
            {
                enemies.RemoveAt(i);
            }
        }
    }
    public void NotifyEnemyDeath(Enemy enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
            
        }
    }
}