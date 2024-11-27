using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager :  Singleton<StageManager>
{
    public string[] randomStageScenesMap1;
    public string[] randomStageScenesMap2;
    public string[] randomStageScenesMap3;
    public string shopScene1 = "ShopScene";
    public string shopScene2 = "ShopScene2";
    public string shopScene3 = "ShopScene3";
    public string bossScene1 = "BossScene";
    public string bossScene2 = "BossScene2";
    public string bossScene3 = "BossScene3";
    public int currentStage = 1;
    public int currentMap = 1;

    List<CallSave> callSaves = new List<CallSave>();

    public void WarpToNextStage()
    {
        currentStage++;

        if (currentStage >= 1 && currentStage <= 4)
        {
            LoadRandomStageForCurrentMap();
        }
        else if (currentStage == 5)
        {
            LoadShopForCurrentMap();
        }
        else if (currentStage == 6)
        {
            LoadBossForCurrentMap();
        }
        else
        {
            GoToNextMap();
        }
    }

    private void LoadRandomStageForCurrentMap()
    {
        string[] randomStages = GetRandomStagesForCurrentMap();
        if (randomStages.Length > 0)
        {
            int randomIndex = Random.Range(0, randomStages.Length);
            LoadScene(randomStages[randomIndex]);
        }
    }

    private void LoadShopForCurrentMap()
    {
        string shopScene = currentMap switch
        {
            1 => shopScene1,
            2 => shopScene2,
            3 => shopScene3,
            _ => shopScene1
        };
        LoadScene(shopScene);
    }

    private void LoadBossForCurrentMap()
    {
        string bossScene = currentMap switch
        {
            1 => bossScene1,
            2 => bossScene2,
            3 => bossScene3,
            _ => bossScene1
        };
        LoadScene(bossScene);

        if (currentMap < 3)
            GoToNextMap();
        else
            ResetToFirstStage();
    }

    private string[] GetRandomStagesForCurrentMap()
    {
        return currentMap switch
        {
            1 => randomStageScenesMap1,
            2 => randomStageScenesMap2,
            3 => randomStageScenesMap3,
            _ => new string[0]
        };
    }

    public void GoToNextMap()
    {
        currentMap++;
        currentStage = 1;
        WarpToNextStage();
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SpawnPlayer();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SpawnPlayer()
    {
        GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
        if (spawnPoint != null)
        {
            GameObject playerPrefab = Resources.Load<GameObject>("PlayerPrefab");
            if (playerPrefab != null)
            {
                Instantiate(playerPrefab, spawnPoint.transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogError("PlayerPrefab not found in Resources folder!");
            }
        }
        else
        {
            Debug.LogWarning("PlayerSpawnPoint not found in the scene!");
        }
    }

    public void ResetToFirstStage()
    {
        currentStage = 1;
        currentMap = 1;
    }

    public void FollowSave(CallSave _callSave)
    {
        callSaves.Add(_callSave);
    }

}

public interface CallSave
{
    void OnCallSave();

}