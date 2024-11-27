using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CurrencyManager : Singleton<CurrencyManager>
{

    public int gems;
    public int gold;

    public bool canDropGold = false;

    public event Action OnCurrencyUpdated;

    private void Start()
    {
       /* if (SaveManager.Instance.onContinue)
        {
            //Load Savegems
            gems = SaveManager.Instance.saveData.gems;
        }*/
    }

    public void AddGems(int amount)
    {
        gems += amount;
        Debug.Log("Added Gems: " + amount + " | Total Gems: " + gems);
        RaiseCurrencyUpdated();
    }

    public bool SpendGems(int amount)
    {
        if (gems >= amount)
        {
            gems -= amount;
            Debug.Log("Spent Gems: " + amount + " | Remaining Gems: " + gems);
            RaiseCurrencyUpdated();
            return true;
        }
        Debug.Log("Not enough Gems!");
        return false;
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log("Added Gold: " + amount + " | Total Gold: " + gold);
        RaiseCurrencyUpdated();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            Debug.Log("Spent Gold: " + amount + " | Remaining Gold: " + gold);
            RaiseCurrencyUpdated();
            return true;
        }
        Debug.Log("Not enough Gold!");
        return false;
    }

    public void EnableGoldDrop()
    {
        canDropGold = true;
        Debug.Log("Gold drop enabled!");
    }

    public void RaiseCurrencyUpdated()
    {
        OnCurrencyUpdated?.Invoke();
    }
}