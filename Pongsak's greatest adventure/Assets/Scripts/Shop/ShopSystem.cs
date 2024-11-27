using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    public GameObject[] itemButtons;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public int[] gemPrices;
    public PlayerHealth playerHealth;

    public Animator animator;
    public string defaultIdleAnimation = "Idle1";
    public string[] specialIdleAnimations;
    public string playerNearAnimationName;

    private PlayerCombat playerCombat;

    public TextMeshProUGUI lightAttackDamageText;
    public TextMeshProUGUI maxHealthText;
    #endregion

    #region Private Variables
    private bool isShopOpen = false;
    private bool playerIsNear = false;
    private bool confirmationOpen = false;
    private int currentIndex = 0;
    private int selectedConfirmIndex = 0;
    private float inputDelay = 0.2f;
    private float lastInputTime = 0f;

    private Coroutine idleAnimationCoroutine;
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

        playerHealth = FindObjectOfType<PlayerHealth>();
        playerCombat = FindObjectOfType<PlayerCombat>();

        idleAnimationCoroutine = StartCoroutine(PlayRandomIdleAnimations());

        
        UpdateCombatStatsUI();
    }

    private void UpdateCombatStatsUI()
    {
        if (lightAttackDamageText != null)
        {
            lightAttackDamageText.text = "" + playerCombat.baseLightAttackDamage.ToString();
        }

        if (maxHealthText != null)
        {
            maxHealthText.text = "" + playerHealth.maxHealth.ToString();
        }
    }

    private void Update()
    {
        if (playerIsNear && (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Joystick1Button5)))
        {
            ToggleShop(!isShopOpen);
        }

        if (isShopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleShop(false);
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
            StopCoroutine(idleAnimationCoroutine);
            StartCoroutine(PlayPlayerNearAnimation());

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
            ToggleShop(false);
            idleAnimationCoroutine = StartCoroutine(PlayRandomIdleAnimations());

            if (proximityIndicatorUI != null)
            {
                proximityIndicatorUI.SetActive(false);
            }
        }
    }
    #endregion

    #region Animation Logic
    private IEnumerator PlayRandomIdleAnimations()
    {
        animator.Play(defaultIdleAnimation);

        while (!playerIsNear)
        {
            yield return new WaitForSeconds(6f);

            string randomSpecialIdle = specialIdleAnimations[Random.Range(0, specialIdleAnimations.Length)];
            animator.Play(randomSpecialIdle);

            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

            animator.Play(defaultIdleAnimation);
        }
    }

    private IEnumerator PlayPlayerNearAnimation()
    {
        animator.Play(playerNearAnimationName);

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        animator.Play(defaultIdleAnimation);
    }
    #endregion

    #region Input Logic
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectItemWithMouse();
        }

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

        
        if ( Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            if (currentIndex >= 0 && currentIndex < itemButtons.Length)
            {
                ShowConfirmationPopup();
            }
        }
    }

    private void SelectItemWithMouse()
    {
        for (int i = 0; i < itemButtons.Length; i++)
        {
            RectTransform buttonRect = itemButtons[i].GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(buttonRect, Input.mousePosition))
            {
                currentIndex = i;
                UpdateButtonColors();
                ShowConfirmationPopup();
                break;
            }
        }
    }
    #endregion

    #region Shop Logic
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
            if (i == currentIndex)
            {
                buttonImage.color = highlightColor;
            }
            else
            {
                buttonImage.color = normalColor;
            }
        }
    }

    private void ShowConfirmationPopup()
    {
        confirmationOpen = true;
        confirmationPopupUI.SetActive(true);
        Time.timeScale = 0f;
        selectedConfirmIndex = 0;
        UpdateConfirmationButtonSprites();
    }

    public void ConfirmPurchase()
    {
        confirmationOpen = false;
        confirmationPopupUI.SetActive(false);

        int gemPrice = gemPrices[currentIndex];

        if (CurrencyManager.Instance.SpendGems(gemPrice))
        {
            Debug.Log("Purchased item with Gems: " + itemButtons[currentIndex].name);

            
            if (currentIndex == 0)
            {
                playerHealth.IncreaseMaxHealth(25);
                Debug.Log("Player's max HP increased by 25");
            }
            
            else if (currentIndex == 1) 
            {
                playerCombat.baseLightAttackDamage += 10;
                Debug.Log("Player's light attack damage increased by 10");
            }

            UpdateCombatStatsUI();
        }
        else
        {
            Debug.Log("Not enough Gems to purchase this item.");
        }

        Time.timeScale = 1f;
        CloseShop();
    }

    public void CancelPurchase()
    {
        confirmationOpen = false;
        confirmationPopupUI.SetActive(false);
        Time.timeScale = 1f;
        CloseShop();
    }

    public void ToggleShop(bool open)
    {
        isShopOpen = open;
        shopUI.SetActive(open);
        Time.timeScale = open ? 0f : 1f;
    }

    private void CloseShop()
    {
        isShopOpen = false;
        shopUI.SetActive(false);
        Time.timeScale = 1f;
    }
    #endregion

    #region Confirmation Popup Logic
    private void HandleConfirmationInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput != 0 && Time.time - lastInputTime > inputDelay)
        {
            selectedConfirmIndex = (selectedConfirmIndex == 0) ? 1 : 0;
            UpdateConfirmationButtonSprites();
            lastInputTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Joystick1Button0))
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

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            CancelPurchase();
        }
    }

    private void UpdateConfirmationButtonSprites()
    {
        Image confirmImage = confirmButton.GetComponent<Image>();
        Image cancelImage = cancelButton.GetComponent<Image>();

        if (selectedConfirmIndex == 0)
        {
            confirmImage.sprite = confirmSelect;
            cancelImage.sprite = cancelNormal;
        }
        else
        {
            confirmImage.sprite = confirmNormal;
            cancelImage.sprite = cancelSelect;
        }
    }
    #endregion
}