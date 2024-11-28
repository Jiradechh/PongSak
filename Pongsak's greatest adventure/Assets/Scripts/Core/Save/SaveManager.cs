using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    public SaveData saveData = new SaveData();

    public bool onContinue = false;

    private static string savePath => Path.Combine(Application.persistentDataPath, "savefile.json");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadGame();
    }
    public void LoadGame()
    {
        if (MainMenuScripts.Instance == null)
        {
            Debug.LogError("MainMenuScripts.Instance is null. Ensure MainMenuScripts initializes first.");
            return;
        }

        saveData.gems = PlayerPrefs.GetInt("Gems", 0);
        saveData.maxHealth = PlayerPrefs.GetInt("MaxHealth", 0);
        saveData.baseLightAttackDamage = PlayerPrefs.GetInt("BaseLightAttackDamage", 0);
        saveData.baseHeavyAttackDamage = PlayerPrefs.GetInt("BaseHeavyAttackDamage", 0);

        bool hasValidSave = saveData.maxHealth > 0;

        MainMenuScripts.Instance.SetContinueButtonState(hasValidSave);
        Debug.Log($"Save loaded: MaxHealth = {saveData.maxHealth}");
    }
    public bool HasSaveGame()
    {
        return PlayerPrefs.HasKey("MaxHealth") && PlayerPrefs.GetInt("MaxHealth") > 0;
    }

    public void SaveGems(int gems)
    {
        PlayerPrefs.SetInt("Gems", gems);
        PlayerPrefs.Save();
    }

    public void SaveMaxHealth(int maxHealth)
    {
        PlayerPrefs.SetInt("MaxHealth", maxHealth);
        PlayerPrefs.Save();
    }

    public void SaveBaseLightAttackDamage(int baseLightAttackDamage)
    {
        PlayerPrefs.SetInt("BaseLightAttackDamage", baseLightAttackDamage);
        PlayerPrefs.Save();
    }

    public void SaveBaseHeavyAttackDamage(int baseHeavyAttackDamage)
    {
        PlayerPrefs.SetInt("BaseHeavyAttackDamage", baseHeavyAttackDamage);
        PlayerPrefs.Save();
    }
}