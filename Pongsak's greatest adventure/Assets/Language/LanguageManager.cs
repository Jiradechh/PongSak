using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    public LanguageData thaiLanguage;
    public LanguageData englishLanguage;

    private Dictionary<string, string> currentLanguage;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        SetLanguage("Thai"); // Default language
    }

    public void SetLanguage(string language)
    {
        if (language == "Thai")
            currentLanguage = CreateDictionary(thaiLanguage);
        else if (language == "English")
            currentLanguage = CreateDictionary(englishLanguage);
    }

    private Dictionary<string, string> CreateDictionary(LanguageData languageData)
    {
        var dict = new Dictionary<string, string>();
        for (int i = 0; i < languageData.keys.Length; i++)
        {
            dict.Add(languageData.keys[i], languageData.values[i]);
        }
        return dict;
    }

    public string GetText(string key)
    {
        return currentLanguage.ContainsKey(key) ? currentLanguage[key] : key;
    }
}