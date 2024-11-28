using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LanguageData", menuName = "Localization/LanguageData")]
public class LanguageData : ScriptableObject
{
    public string[] keys;
    public string[] values;
}
