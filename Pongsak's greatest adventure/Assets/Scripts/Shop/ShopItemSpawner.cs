using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemSpawner : MonoBehaviour
{
    public GameObject[] shopItemPrefabs; 
    public Transform[] spawnPoints;     

    private void Start()
    {
        SpawnRandomShopItems();
    }

    
    private void SpawnRandomShopItems()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            int randomIndex = Random.Range(0, shopItemPrefabs.Length);
            Instantiate(shopItemPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);
        }
    }
}
