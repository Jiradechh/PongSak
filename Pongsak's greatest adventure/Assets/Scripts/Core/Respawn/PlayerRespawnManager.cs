using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawnManager : Singleton<PlayerRespawnManager>
{
    public string respawnPointName = "RespawnPoint";
    public string lobbySceneName = "Lobby";
    public float respawnDelay = 3f;

    public PlayerController playerController;
    public PlayerHealth playerHealth;
    public PlayerCombat playerCombat;
    public CurrencyManager currencyManager;
    private Transform[] respawnPoints;

    private void Awake()
    {
        playerController = PlayerController.Instance;
        playerHealth = PlayerHealth.Instance;
        playerCombat = PlayerCombat.Instance;
        currencyManager = CurrencyManager.Instance;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void RespawnPlayer()
    {
        StartCoroutine(HandleRespawn());
    }

    private IEnumerator HandleRespawn()
    {
        playerController.canMove = false;
        playerController.enabled = false;
        playerCombat.enabled = false;

        
        SaveManager.Instance.SaveBaseHeavyAttackDamage(playerCombat.baseHeavyAttackDamage);
        SaveManager.Instance.SaveBaseLightAttackDamage(playerCombat.baseLightAttackDamage);
        SaveManager.Instance.SaveMaxHealth(playerHealth.maxHealth);
        SaveManager.Instance.SaveGems(currencyManager.gems);

        yield return new WaitForSeconds(respawnDelay);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(lobbySceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == lobbySceneName)
        {
            
            FindRespawnPointsByName();

            Transform respawnPoint = FindAvailableRespawnPoint();
            if (respawnPoint != null)
            {
                playerController.transform.position = respawnPoint.position;
            }

          
            playerHealth.RestoreToMaxHealth();
            playerHealth.isDead = false;

            playerController.canMove = true;
            playerController.ResetControllerState();
            playerCombat.ResetCombatState();
            playerController.enabled = true;
            playerCombat.enabled = true;
        }
    }

    private void FindRespawnPointsByName()
    {
        GameObject[] respawnObjects = GameObject.FindGameObjectsWithTag(respawnPointName);
        respawnPoints = new Transform[respawnObjects.Length];

        for (int i = 0; i < respawnObjects.Length; i++)
        {
            respawnPoints[i] = respawnObjects[i].transform;
        }
    }

    private Transform FindAvailableRespawnPoint()
    {
        if (respawnPoints.Length > 0)
        {
            return respawnPoints[Random.Range(0, respawnPoints.Length)];
        }
        return null;
    }
}