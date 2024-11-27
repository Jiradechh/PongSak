using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartBarUI : MonoBehaviour
{
    #region Public Variables
    public int maxHealth = 125;
    public int heartsPerRow = 5; // Number of hearts per row
    public GameObject heartPrefab; // Reference to the heart prefab
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;
    public RectTransform heartBarContainer; // Parent GameObject of hearts
    public int healthPerHeart = 25; // Health points per heart
    #endregion

    #region Private Variables
    private List<Image> hearts = new List<Image>();
    private int currentHealth;
    private int totalHearts;
    #endregion

    void Start()
    {
        currentHealth = maxHealth;
        totalHearts = maxHealth / healthPerHeart;
        CreateHeartBar();
    }

    void CreateHeartBar()
    {
        // Remove any existing hearts (if re-creating)
        foreach (Transform child in heartBarContainer)
        {
            Destroy(child.gameObject);
        }

        // Create heart icons based on the total health
        for (int i = 0; i < totalHearts; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartBarContainer);
            hearts.Add(newHeart.GetComponent<Image>());
        }

        UpdateHeartBar();
    }

    public void UpdateHeartBar()
    {
        // Loop through hearts and set sprite based on current health
        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < currentHealth / healthPerHeart)
            {
                hearts[i].sprite = fullHeartSprite;
            }
            else
            {
                hearts[i].sprite = emptyHeartSprite;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateHeartBar();
    }

    public void RestoreHealth(int health)
    {
        currentHealth += health;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHeartBar();
    }
}