using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager :  Singleton<StageManager>
{
    [Header("Stage Configurations")]
    public string[] randomStageScenesMap1;
    public string[] randomStageScenesMap2;
    public string[] randomStageScenesMap3;
    public string shopScene1 = "ShopScene";
    public string shopScene2 = "ShopScene2";
    public string shopScene3 = "ShopScene3";
    public string bossScene1 = "BossScene1";
    public string bossScene2 = "BossScene2";
    public string bossScene3 = "BossScene3";

    [Header("Audio Configurations")]
    public AudioClip map1Song;
    public AudioClip map2Song;
    public AudioClip map3Song;
    public AudioClip boss1Song;
    public AudioClip boss2Song;
    public AudioClip boss3Song;

    [Range(0f, 1f)] public float musicVolume = 1f;
    private AudioSource audioSource;
    private Coroutine musicTransitionCoroutine;

    [Header("Light Configurations")]
    public GameObject map2Light;
    public GameObject map3Light;

    public int currentStage = 1;
    public int currentMap = 1;

    private List<CallSave> callSaves = new List<CallSave>();

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = true;
        audioSource.volume = musicVolume;

        UpdateLightState();
    }
    private void UpdateLightState()
    {
        if (map2Light != null) map2Light.SetActive(currentMap == 2);
        if (map3Light != null) map3Light.SetActive(currentMap == 3);
    }
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
        else if (currentStage > 6)
        {
            GoToNextMap();
        }
    }

    private void LoadRandomStageForCurrentMap()
    {
        PlayMapSong();
        string[] randomStages = GetRandomStagesForCurrentMap();
        if (randomStages.Length > 0)
        {
            int randomIndex = Random.Range(0, randomStages.Length);
            LoadScene(randomStages[randomIndex]);
        }
    }

    private void LoadShopForCurrentMap()
    {
        PlayMapSong(); 
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
        PlayBossSong();

        string bossScene = currentMap switch
        {
            1 => bossScene1,
            2 => bossScene2,
            3 => bossScene3,
            _ => bossScene1
        };

        LoadScene(bossScene);
        Debug.Log($"Loading Boss Scene: {bossScene}");

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
        if (currentMap > 3) currentMap = 1;
        currentStage = 1;
        UpdateLightState();
        WarpToNextStage();
    }
    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log($"Loading scene: {sceneName}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == bossScene1)
        {
            PlaySong(boss1Song, 1.5f);
        }
        else if (scene.name == bossScene2)
        {
            PlaySong(boss2Song, 1.5f);
        }
        else if (scene.name == bossScene3)
        {
            PlaySong(boss3Song, 1.5f);
        }
        else
        {
            PlayMapSong();
        }

        SpawnPlayer();
        UpdateLightState();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SpawnPlayer()
    {
        GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
        if (spawnPoint != null)
        {
            GameObject playerPrefab = Resources.Load<GameObject>("Player");
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

    public void FollowSave(CallSave callSave)
    {
        callSaves.Add(callSave);
    }

    private void PlayMapSong()
    {
        AudioClip clip = currentMap switch
        {
            1 => map1Song,
            2 => map2Song,
            3 => map3Song,
            _ => null
        };

        PlaySong(clip, 1.5f);
    }

    private void PlayBossSong()
    {
        AudioClip clip = currentMap switch
        {
            1 => boss1Song,
            2 => boss2Song,
            3 => boss3Song,
            _ => null
        };

        PlaySong(clip, 1.5f);
    }

    private void PlaySong(AudioClip clip, float fadeDuration)
    {
        if (audioSource.clip == clip) return;

        if (musicTransitionCoroutine != null)
        {
            StopCoroutine(musicTransitionCoroutine);
        }

        musicTransitionCoroutine = StartCoroutine(FadeOutAndIn(clip, fadeDuration));
    }

    private IEnumerator FadeOutAndIn(AudioClip newClip, float fadeDuration)
    {
        if (audioSource.isPlaying)
        {
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(musicVolume, 0, t / fadeDuration);
                yield return null;
            }
            audioSource.volume = 0;
            audioSource.Stop();
        }

        audioSource.clip = newClip;
        audioSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, musicVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = musicVolume;
        Debug.Log($"Playing audio clip: {newClip.name} at volume {audioSource.volume}");
    }
}

public interface CallSave
{
    void OnCallSave();
}