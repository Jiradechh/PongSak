using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSystemGold : MonoBehaviour
{
    #region Public Variables
    [Header("Button Sprites")]
    public Sprite buySelectedSprite; // Buy button when selected
    public Sprite buyNotSelectedSprite; // Buy button when not selected
    public Sprite cancelSelectedSprite; // Cancel button when selected
    public Sprite cancelNotSelectedSprite; // Cancel button when not selected

    public int selectedButtonIndex = 0; // 0 = Buy, 1 = Cancel
    private Image buyButtonImage; // Reference to the Buy button's Image component
    private Image cancelButtonImage; // Reference to the Cancel button's Image component

    public GameObject shopUI;
    public GameObject proximityIndicatorUI;
    public GameObject confirmationPopupUI; // Confirmation popup UI
    public TextMeshProUGUI confirmationText; // TextMeshPro to display item price
    public Button confirmButton; // Confirm purchase button
    public Button cancelButton; // Cancel button
    public GameObject[] itemButtons;
    public Transform[] spawnPoints;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public int[] goldPrices = new int[14];
    public Animator shopAnimator;
    public CinemachineVirtualCamera cinemachineCamera;
    #endregion

    #region Private Variables
    private bool isShopOpen = false;
    private bool playerIsNear = false;
    private int currentIndex = 0;
    private float inputDelay = 0f;
    private float lastInputTime = 0f;
    private GameObject[] instantiatedButtons;
    private List<GameObject> availableItems;
    private int selectedItemPrice = 0;
    private string selectedItemName = "";
    private int selectedItemIndex = -1;

    private bool isPopupActive = false;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        buyButtonImage = confirmButton.GetComponent<Image>();
        cancelButtonImage = cancelButton.GetComponent<Image>();
        UpdateButtonSprites();

        instantiatedButtons = new GameObject[spawnPoints.Length];
        availableItems = new List<GameObject>(itemButtons);
        shopUI.SetActive(false);

        if (proximityIndicatorUI != null)
        {
            proximityIndicatorUI.SetActive(false);
        }
        if (cinemachineCamera == null)
        {
            GameObject camObject = GameObject.Find("CV CAM");
            if (camObject != null)
            {
                cinemachineCamera = camObject.GetComponent<CinemachineVirtualCamera>();
                Debug.Log("CinemachineVirtualCamera found and assigned.");
            }
            else
            {
                Debug.LogWarning("CinemachineVirtualCamera with name 'CV cam' not found!");
            }
        }

        goldPrices[0] = 50;  // MaxDash
        goldPrices[1] = 50;  // MaxPro
        goldPrices[2] = 50;  // Slow
        goldPrices[3] = 100; // UpRateGold
        goldPrices[4] = 25;  // HpUp
        goldPrices[5] = 75;  // Arrmor
        goldPrices[6] = 100; // Carti
        goldPrices[7] = 75;  // DashRange
        goldPrices[8] = 100; // Bomb
        goldPrices[9] = 100; // Stun
        goldPrices[10] = 100; // CamSpeed
    }

    private void Update()
    {
        if (isPopupActive)
        {
            HandlePopupInput();
            return;
        }

        if (playerIsNear && (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Joystick1Button5)))
        {
            ToggleShop(!isShopOpen);
        }

        if (isShopOpen)
        {
            HandleInput();
        }
    }
    private void UpdateButtonSprites()
    {
        if (selectedButtonIndex == 0)
        {
            buyButtonImage.sprite = buySelectedSprite;
            cancelButtonImage.sprite = cancelNotSelectedSprite;
        }
        else 
        {
            buyButtonImage.sprite = buyNotSelectedSprite;
            cancelButtonImage.sprite = cancelSelectedSprite;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;

            if (shopAnimator != null)
            {
                shopAnimator.SetTrigger("isHi");
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
            ToggleShop(false);

            if (shopAnimator != null)
            {
                shopAnimator.ResetTrigger("isHi");
            }

            if (proximityIndicatorUI != null)
            {
                proximityIndicatorUI.SetActive(false);
            }
        }
    }
    #endregion

    #region Input Logic

    private void HandlePopupInput()
    {
        if (!isPopupActive) return;

        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput < 0)
        {
            selectedButtonIndex = 0;
            UpdateButtonSprites();
            lastInputTime = Time.time;
        }
        else if (horizontalInput > 0)
        {
            selectedButtonIndex = 1;
            UpdateButtonSprites();
            lastInputTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            if (selectedButtonIndex == 0)
            {
                ConfirmPurchase();
            }
            else
            {
                CancelPurchase();
            }
        }
    }
    private void HandleInput()
    {
        if (isPopupActive) return;

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

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            OpenConfirmationPopup();
        }
    }
    #endregion

    #region Confirmation Popup Logic
    private void OpenConfirmationPopup()
    {
        if (instantiatedButtons[currentIndex] == null)
        {
            Debug.LogWarning("No item is selected at the current index.");
            return;
        }

        selectedItemIndex = currentIndex;
        selectedItemName = instantiatedButtons[currentIndex].name;
        selectedItemPrice = goldPrices[selectedItemIndex];

        if (confirmationText != null)
        {
            confirmationText.text = $"{selectedItemPrice}";
        }

        confirmationPopupUI.SetActive(true);
        isPopupActive = true; 
        selectedButtonIndex = 0;
        UpdateButtonSprites();
        Time.timeScale = 0f;
    }

    private void CloseConfirmationPopup()
    {
        confirmationPopupUI.SetActive(false);
        isPopupActive = false; 
        Time.timeScale = 1f;
    }

    public void ConfirmPurchase()
    {
        if (SpendGoldAndValidate(selectedItemName))
        {
            PurchaseItemLogic(selectedItemName);
        }

        CloseConfirmationPopup();
    }

    public void CancelPurchase()
    {
        CloseConfirmationPopup();
    }
    #endregion


    #region Mouse Selection Logic
    private void SelectItemWithMouse()
    {
        for (int i = 0; i < instantiatedButtons.Length; i++)
        {
            if (instantiatedButtons[i] == null) continue;

            RectTransform buttonRect = instantiatedButtons[i].GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(buttonRect, Input.mousePosition))
            {
                currentIndex = i;
                UpdateButtonColors();
                Debug.Log($"Mouse selected item: {instantiatedButtons[i].name}");
                break;
            }
        }
    }
    #endregion

    #region Shop Logic
    private void MoveRight()
    {
        currentIndex++;
        if (currentIndex >= instantiatedButtons.Length)
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
            currentIndex = instantiatedButtons.Length - 1;
        }
        UpdateButtonColors();
    }

    private void MoveUp()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = instantiatedButtons.Length - 1;
        }
        UpdateButtonColors();
    }

    private void MoveDown()
    {
        currentIndex++;
        if (currentIndex >= instantiatedButtons.Length)
        {
            currentIndex = 0;
        }
        UpdateButtonColors();
    }

    private void UpdateButtonColors()
    {
        for (int i = 0; i < instantiatedButtons.Length; i++)
        {
            Color targetColor = (i == currentIndex) ? highlightColor : normalColor;
            instantiatedButtons[i].GetComponent<Image>().color = targetColor;
        }
    }

    private void PurchaseItemLogic(string itemName)
    {
        switch (itemName)
        {
            case "MaxDash":
                if (SpendGoldAndValidate(itemName))
                {
                    PlayerController.Instance.IncreaseMaxDashes();
                    Debug.Log("Max dashes purchased");
                }
                break;

            case "MaxPro":
                if (SpendGoldAndValidate(itemName))
                {
                    PlayerCombat.Instance.IncreaseMaxProjectiles();
                    Debug.Log("Max projectiles purchased");
                }
                break;

            case "Slow":
                if (SpendGoldAndValidate(itemName))
                {
                    PlayerCombat.Instance.EnableSlowEffect();
                    Debug.Log("Slow Effect purchased!");
                }
                break;

            case "UpRateGold":
                if (SpendGoldAndValidate(itemName))
                {
                    CurrencyManager.Instance.EnableGoldDrop();
                    Debug.Log("Gold drop feature purchased");
                }
                break;

            case "UpHp":
                if (SpendGoldAndValidate(itemName))
                {
                    PlayerHealth.Instance.RestoreToMaxHealth();
                    Debug.Log("HpUp purchased");
                }
                break;

            case "Arrmor":
                if (SpendGoldAndValidate(itemName))
                {
                    PlayerHealth.Instance.ActivateAvoidNextHit();
                    Debug.Log("Arrmor purchased");
                }
                break;

            case "Carti":
                if (SpendGoldAndValidate(itemName))
                {
                    PlayerHealth.Instance.hasCarti = true;
                    Debug.Log("Carti purchased");
                }
                break;

            case "DashRange":
                if (SpendGoldAndValidate(itemName))
                {
                    PlayerController.Instance.IncreaseDashSpeed(5f);
                    Debug.Log("DashRange purchased");
                }
                break;

            case "Bomb":
                if (SpendGoldAndValidate(itemName))
                {
                    PlayerCombat.Instance.EnableAOEProjectiles();
                    Debug.Log("Bomb purchased");
                }
                break;

            case "Stun":
                if (SpendGoldAndValidate(itemName))
                {
                    PlayerCombat.Instance.EnableStunEffect();
                    Debug.Log("Stun purchased");
                }
                break;

            case "CamSpeed":
                if (SpendGoldAndValidate(itemName))
                {
                    AdjustCameraFOV(10f);
                    PlayerController.Instance.SetMoveSpeed(3.5f);
                    Debug.Log("CamSpeed purchased");
                }
                break;

            default:
                Debug.LogError($"Unknown item purchased: {itemName}");
                break;
        }
    }

    private void AdjustCameraFOV(float newFOV)
    {
        if (cinemachineCamera != null)
        {
            cinemachineCamera.m_Lens.FieldOfView = newFOV;
            Debug.Log($"Cinemachine camera FOV adjusted to {newFOV}");
        }
        else
        {
            Debug.LogWarning("CinemachineVirtualCamera reference is missing!");
        }
    }
    private bool SpendGoldAndValidate(string itemName)
    {
        int goldPriceIndex = System.Array.IndexOf(itemButtons.Select(b => b.name).ToArray(), itemName);

        if (goldPriceIndex < 0 || goldPriceIndex >= goldPrices.Length)
        {
            Debug.LogError($"Gold price not found for item: {itemName}");
            return false;
        }

        int goldPrice = goldPrices[goldPriceIndex];

        if (!CurrencyManager.Instance.SpendGold(goldPrice))
        {
            Debug.Log($"Not enough Gold to purchase: {itemName}");
            return false;
        }

        return true;
    }

    public void ToggleShop(bool open)
    {
        isShopOpen = open;
        shopUI.SetActive(open);
        Time.timeScale = open ? 0f : 1f;

        if (open)
        {
            SpawnItemButtons();
        }
        else
        {
            DestroyItemButtons();
        }
    }

    private void SpawnItemButtons()
    {
        DestroyItemButtons();

        List<int> chosenIndices = new List<int>();

        while (chosenIndices.Count < 3)
        {
            int randomIndex = UnityEngine.Random.Range(0, itemButtons.Length);
            if (!chosenIndices.Contains(randomIndex))
            {
                chosenIndices.Add(randomIndex);
            }
        }

        for (int i = 0; i < spawnPoints.Length && i < chosenIndices.Count; i++)
        {
            int itemIndex = chosenIndices[i];
            GameObject selectedItem = itemButtons[itemIndex];

            instantiatedButtons[i] = Instantiate(selectedItem, spawnPoints[i].position, Quaternion.identity);
            instantiatedButtons[i].transform.SetParent(shopUI.transform);

            instantiatedButtons[i].name = selectedItem.name;
        }

        UpdateButtonColors();
    }

    private void DestroyItemButtons()
    {
        for (int i = 0; i < instantiatedButtons.Length; i++)
        {
            if (instantiatedButtons[i] != null)
            {
                Destroy(instantiatedButtons[i]);
                instantiatedButtons[i] = null;
            }
        }
    }
    #endregion
}