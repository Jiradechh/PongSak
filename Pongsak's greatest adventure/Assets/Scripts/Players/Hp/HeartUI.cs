using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartUI : Singleton<HeartUI>
{
    public List<Image> hearts;
    public Color fullHeartColor = Color.white;
    public Color emptyHeartColor = new Color(1, 1, 1, 0.3f); 
    public int heartHealthValue = 20;


    public void UpdateHearts(int currentHealth, int maxHealth)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            int heartMaxValue = (i + 1) * heartHealthValue;

            if (currentHealth >= heartMaxValue)
            {
                hearts[i].color = fullHeartColor;
                hearts[i].fillAmount = 1f;
                hearts[i].transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            }
            else if (currentHealth > heartMaxValue - heartHealthValue)
            {
                hearts[i].color = fullHeartColor;
                hearts[i].fillAmount = (float)(currentHealth % heartHealthValue) / heartHealthValue;
                hearts[i].transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            }
            else
            {
                hearts[i].color = emptyHeartColor;
                hearts[i].fillAmount = 1f;
                hearts[i].transform.localScale = new Vector3(0.45f, 0.45f, 1f);
            }
        }
    }
}