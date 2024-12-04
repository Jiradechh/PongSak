using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopSystem : MonoBehaviour
{
    #region Public Variables
    public GameObject shopUI;
    public GameObject confirmationPopupUI;
    public GameObject proximityIndicatorUI;
    public Button confirmButton;
    public Button cancelButton;

    public Sprite confirmNormal;
    public Sprite confirmSelect;
    public Sprite cancelNormal;
    public Sprite cancelSelect;

    public AudioClip shopOpenSound;
    private AudioSource audioSource;
    public AudioClip proximitySound;

    public GameObject[] itemButtons;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.red;
    public int[] gemPrices;
    public PlayerHealth playerHealth;
    public PlayerCombat playerCombat;

    public TextMeshProUGUI lightAttackDamageText;
    public TextMeshProUGUI maxHealthText;

    private Animator animator;
    private float idleTimer = 0f;
    private float idleInterval = 10f;

    private readonly string[] idleAnimationNames = { "Npc_Idle1", "Npc_Idle2", "Npc_Idle3", "Npc_Idle4", "Npc_Idle5" };

    #endregion

    #region Private Variables
    private bool isShopOpen = false;
    private bool playerIsNear = false;
    private bool confirmationOpen = false;
    private int currentIndex = 0;
    private int selectedConfirmIndex = 0;
    private float inputDelay = 0.2f;
    private float lastInputTime = 0f;

    public static bool IsShopOpen { get; private set; } = false;
    #endregion

    #region Unity Callbacks

    private void Start()
    {
        UpdateButtonColors();
        shopUI.SetActive(false);

        if (proximityIndicatorUI != null)
        {
            proximityIndicatorUI.SetActive(false);
        }
        if (animator != null)
        {
            animator.Play("Npc_Idle1");
        }
        playerHealth = FindObjectOfType<PlayerHealth>();
        UpdateCombatStatsUI();
        audioSource = gameObject.AddComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    private void UpdateCombatStatsUI()
    {
        if (lightAttackDamageText != null)
        {
            lightAttackDamageText.text = playerCombat.baseLightAttackDamage.ToString();
        }
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleInterval)
        {
            PlayRandomIdleAnimation();
            idleTimer = 0f;
        }
        if (maxHealthText != null)
        {
            maxHealthText.text = playerHealth.maxHealth.ToString();
        }
    }
    private void PlayRandomIdleAnimation()
    {
        if (animator != null)
        {
            string randomAnimation = idleAnimationNames[Random.Range(1, idleAnimationNames.Length)];
            animator.Play(randomAnimation);
        }
    }
    private void Update()
    {
        if (playerIsNear && Input.GetKeyDown(KeyCode.Joystick1Button5))
        {
            ToggleShop(!isShopOpen);
        }

        if (isShopOpen && !confirmationOpen)
        {
            HandleInput();
        }
        else if (confirmationOpen)
        {
            HandleConfirmationInput();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
            if (audioSource != null && proximitySound != null)
            {
                audioSource.PlayOneShot(proximitySound);
            }
            if (animator != null)
            {
                animator.SetBool("isPlayerNear", true);
            }
            if (proximityIndicatorUI != null)
            {
                proximityIndicatorUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;

            if (animator != null)
            {
                animator.SetBool("isPlayerNear", false);
            }
            if (proximityIndicatorUI != null)
            {
                proximityIndicatorUI.SetActive(false);
            }

            if (isShopOpen)
            {
                StartCoroutine(CloseShopWithDelay(0.2f));
            }
        }
    }
    #endregion

    #region Input Logic
    private void HandleInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (horizontalInput == 0 && verticalInput == 0)
        {
            lastInputTime = 0;
        }
        if (Time.time - lastInputTime > inputDelay)
        {
            if (horizontalInput > 0.5f)
            {
                MoveRight();
                lastInputTime = Time.time;
            }
            else if (horizontalInput < -0.5f)
            {
                MoveLeft();
                lastInputTime = Time.time;
            }

            if (verticalInput > 0.5f)
            {
                MoveUp();
                lastInputTime = Time.time;
            }
            else if (verticalInput < -0.5f)
            {
                MoveDown();
                lastInputTime = Time.time;
            }
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            ShowConfirmationPopup();
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            ToggleShop(false);
        }
    }

    private void HandleConfirmationInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput < 0)
        {
            selectedConfirmIndex = 0;
            UpdateConfirmationButtonSprites();
            lastInputTime = Time.time;
        }
        else if (horizontalInput > 0)
        {
            selectedConfirmIndex = 1;
            UpdateConfirmationButtonSprites();
            lastInputTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            if (selectedConfirmIndex == 0)
            {
                ConfirmPurchase();
            }
            else
            {
                CancelPurchase();
            }
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            CancelPurchase();
        }
    }
    #endregion

    #region Shop Navigation
    private void MoveRight()
    {
        currentIndex++;
        if (currentIndex >= itemButtons.Length)
        {
            currentIndex = 0;
        }
        UpdateButtonColors();
    }

    private void MoveLeft()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = itemButtons.Length - 1;
        }
        UpdateButtonColors();
    }

    private void MoveUp()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = itemButtons.Length - 1;
        }
        UpdateButtonColors();
    }

    private void MoveDown()
    {
        currentIndex++;
        if (currentIndex >= itemButtons.Length)
        {
            currentIndex = 0;
        }
        UpdateButtonColors();
    }

    private void UpdateButtonColors()
    {
        for (int i = 0; i < itemButtons.Length; i++)
        {
            Image buttonImage = itemButtons[i].GetComponent<Image>();
            if (buttonImage == null) continue;

            buttonImage.color = (i == currentIndex) ? selectedColor : normalColor;
        }
    }
    #endregion

    #region Confirmation Popup
    private void ShowConfirmationPopup()
    {
        if (currentIndex < 0 || currentIndex >= itemButtons.Length)
        {
            Debug.LogWarning("Invalid item selected.");
            return;
        }

        confirmationOpen = true;
        confirmationPopupUI.SetActive(true);

        TextMeshProUGUI confirmationText = confirmationPopupUI.GetComponentInChildren<TextMeshProUGUI>();
        if (confirmationText != null)
        {
            string itemName = itemButtons[currentIndex].name;
            int itemPrice = gemPrices[currentIndex];
            confirmationText.text = $"{itemPrice}";
        }

        selectedConfirmIndex = 0;
        UpdateConfirmationButtonSprites();
        Time.timeScale = 0f;
    }

    private void ConfirmPurchase()
    {
        if (currentIndex < 0 || currentIndex >= itemButtons.Length)
        {
            Debug.LogWarning("Invalid item selected.");
            CloseConfirmationPopup();
            return;
        }

        int itemPrice = gemPrices[currentIndex];
        string itemName = itemButtons[currentIndex].name;

        if (CurrencyManager.Instance.SpendGems(itemPrice))
        {
            Debug.Log($"Purchased {itemName} for {itemPrice} gems.");

            if (currentIndex == 0)
            {
                playerHealth.IncreaseMaxHealth(25);
                Debug.Log("Player's max HP increased by 25.");
            }
            else if (currentIndex == 1)
            {
                playerCombat.baseLightAttackDamage += 10;
                Debug.Log("Player's light attack damage increased by 10.");
            }

            UpdateCombatStatsUI();
        }
        else
        {
            Debug.Log("Not enough Gems to purchase this item.");
        }

        CloseAllUI();
    }
    private void CancelPurchase()
    {
        Debug.Log("Purchase cancelled.");
        CloseConfirmationPopup();
    }

    private void CloseConfirmationPopup()
    {
        confirmationOpen = false;
        confirmationPopupUI.SetActive(false);
        Time.timeScale = 1f;
    }
    private void CloseAllUI()
    {
        shopUI.SetActive(false);
        confirmationPopupUI.SetActive(false);

        isShopOpen = false;
        confirmationOpen = false;
        IsShopOpen = false;

        Time.timeScale = 1f;
    }
    private void UpdateConfirmationButtonSprites()
    {
        Image confirmImage = confirmButton.GetComponent<Image>();
        Image cancelImage = cancelButton.GetComponent<Image>();

        confirmImage.sprite = (selectedConfirmIndex == 0) ? confirmSelect : confirmNormal;
        cancelImage.sprite = (selectedConfirmIndex == 1) ? cancelSelect : cancelNormal;
    }
    #endregion

    #region Shop Logic

    public void ToggleShop(bool open)
    {
        if (!playerIsNear && open) return;

        if (open)
        {
            IsShopOpen = true;
            isShopOpen = true;
            shopUI.SetActive(true);
            if (shopOpenSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shopOpenSound);
            }
            currentIndex = 0;
            UpdateButtonColors();

            Time.timeScale = 0f;
        }
        else
        {
            StartCoroutine(CloseShopWithDelay(0.2f));
        }
    }
    private IEnumerator CloseShopWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        shopUI.SetActive(false);
        isShopOpen = false;
        IsShopOpen = false;
        Time.timeScale = 1f;
    }
    #endregion
}