using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Treasure : MonoBehaviour
{
    public int goldAmount = 5;
    public int gemAmount = 5;
    private bool isOpening = false;

    private Animator treasureAnimator;
    private readonly string[] bossSceneNames = { "BossScene1", "BossScene2", "BossScene3" };

    private void Awake()
    {
        treasureAnimator = GetComponent<Animator>();
        if (treasureAnimator == null)
        {
            Debug.LogError("No Animator component found on the treasure!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpening)
        {
            isOpening = true;
            OpenTreasure();
        }
    }

    private void OpenTreasure()
    {
        if (treasureAnimator != null)
        {
            treasureAnimator.SetTrigger("OpenGem");
            treasureAnimator.SetTrigger("OpenGold");
        }

        SoundManager.Instance.PlayTreasureOpenSound();

        if (IsInBossFinishScene())
        {
            CurrencyManager.Instance.AddGems(gemAmount);
        }

        CurrencyManager.Instance.AddGold(goldAmount);
        Destroy(gameObject, 2f);
    }

    private bool IsInBossFinishScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        foreach (var sceneName in bossSceneNames)
        {
            if (currentSceneName == sceneName)
            {
                return true;
            }
        }
        return false;
    }
}