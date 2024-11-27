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
        saveData.gems = PlayerPrefs.GetInt("Gems");
        saveData.maxHealth = PlayerPrefs.GetInt("MaxHealth");
        saveData.baseLightAttackDamage = PlayerPrefs.GetInt("BaseLightAttackDamage");
        saveData.baseHeavyAttackDamage = PlayerPrefs.GetInt("BaseHeavyAttackDamage");

        if (saveData.maxHealth > 0)
        {
            //ไม่กดสามารถกด Continue
        }
        Debug.Log(saveData.maxHealth + "LoadMaxHealth");
    }

    public void SaveGems(int gems)
    {
        PlayerPrefs.SetInt("Gems", gems);
    }
    public void SaveMaxHealth(int maxHealth)
    {
        PlayerPrefs.SetInt("MaxHealth", maxHealth);
    }
    public void SaveBaseLightAttackDamage(int baseLightAttackDamage)
    {
        PlayerPrefs.SetInt("BaseLightAttackDamage", baseLightAttackDamage);
    }
    public void SaveBaseHeavyAttackDamage(int baseHeavyAttackDamage)
    {
        PlayerPrefs.SetInt("BaseHeavyAttackDamage", baseHeavyAttackDamage);
    }
}
