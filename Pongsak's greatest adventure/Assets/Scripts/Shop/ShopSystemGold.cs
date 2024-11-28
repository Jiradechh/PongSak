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
    public Sprite buySelectedSprite; 
    public Sprite buyNotSelectedSprite;
    public Sprite cancelSelectedSprite;
    public Sprite cancelNotSelectedSprite;

    public int selectedButtonIndex = 0;
    private Image buyButtonImage;
    private Image cancelButtonImage; 

    public GameObject shopUI;
    public GameObject proximityIndicatorUI;
    public GameObject confirmationPopupUI;
    public TextMeshProUGUI confirmationText;
    public Button confirmButton;
    public Button cancelButton;
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
    private List<string> purchasedItems = new List<string>();
    public static bool IsShopOpen { get; private set; } = false;
    public static bool IsPopupActive { get; private set; } = false;
    private bool hasInitializedItems = false;
    private List<GameObject> selectedItems = new List<GameObject>();
    private bool hasSelectedItems = false;
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

        InitializeItems();
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
        else
        {
            Debug.LogWarning("confirmationText is not assigned!");
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
        StartCoroutine(ClosePopupWithDelay(0.5f));
    }
    private IEnumerator ClosePopupWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        isPopupActive = false;
        IsPopupActive = false;
        Time.timeScale = 1f;
    }
    private bool isPurchaseInProgress = false;
    public void ConfirmPurchase()
    {
        int goldPriceIndex = System.Array.IndexOf(itemButtons.Select(b => b.name).ToArray(), selectedItemName);
        if (goldPriceIndex < 0 || goldPriceIndex >= goldPrices.Length)
        {
            Debug.LogError($"Gold price not found for item: {selectedItemName}");
            CloseConfirmationPopup();
            return;
        }
        int goldPrice = goldPrices[goldPriceIndex];
        if (!CurrencyManager.Instance.SpendGold(goldPrice))
        {
            Debug.Log($"Not enough Gold to purchase: {selectedItemName}");
            CloseConfirmationPopup();
            return;
        }
        PurchaseItemLogic(selectedItemName);
        CloseConfirmationPopup();
        ToggleShop(false);
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
    private void InitializeItems()
    {
        if (hasSelectedItems) return;

        List<GameObject> availableItems = new List<GameObject>();

        foreach (var item in itemButtons)
        {
            if (!purchasedItems.Contains(item.name))
            {
                availableItems.Add(item);
            }
        }

        while (selectedItems.Count < 3 && availableItems.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableItems.Count);
            selectedItems.Add(availableItems[randomIndex]);
            availableItems.RemoveAt(randomIndex);
        }

        hasSelectedItems = true;
    }
    private void PurchaseItemLogic(string itemName)
    {
        if (!purchasedItems.Contains(itemName))
        {
            purchasedItems.Add(itemName);
            selectedItems.RemoveAll(item => item.name == itemName);
        }

        switch (itemName)
        {
            case "MaxDash":
                PlayerController.Instance.IncreaseMaxDashes();
                Debug.Log("Max dashes purchased");
                break;

            case "MaxPro":
                PlayerCombat.Instance.IncreaseMaxProjectiles();
                Debug.Log("Max projectiles purchased");
                break;

            case "Slow":
                PlayerCombat.Instance.EnableSlowEffect();
                Debug.Log("Slow Effect purchased!");
                break;

            case "UpRateGold":
                CurrencyManager.Instance.EnableGoldDrop();
                Debug.Log("Gold drop feature purchased");
                break;

            case "UpHp":
                PlayerHealth.Instance.RestoreToMaxHealth();
                Debug.Log("HpUp purchased");
                break;

            case "Arrmor":
                PlayerHealth.Instance.ActivateAvoidNextHit();
                Debug.Log("Arrmor purchased");
                break;

            case "Carti":
                PlayerHealth.Instance.hasCarti = true;
                Debug.Log("Carti purchased");
                break;

            case "DashRange":
                PlayerController.Instance.IncreaseDashSpeed(5f);
                Debug.Log("DashRange purchased");
                break;

            case "Bomb":
                PlayerCombat.Instance.EnableAOEProjectiles();
                Debug.Log("Bomb purchased");
                break;

            case "Stun":
                PlayerCombat.Instance.EnableStunEffect();
                Debug.Log("Stun purchased");
                break;

            case "CamSpeed":
                AdjustCameraFOV(10f);
                PlayerController.Instance.SetMoveSpeed(3.5f);
                Debug.Log("CamSpeed purchased");
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
        if (open)
        {
            isShopOpen = true;
            IsShopOpen = true;
            shopUI.SetActive(true);
            Time.timeScale = 0f;

            SpawnItemButtons();
        }
        else
        {
            StartCoroutine(CloseShopWithDelay(0.2f));
        }
    }

    private IEnumerator CloseShopWithDelay(float delay)
    {
        shopUI.SetActive(false);
        DestroyItemButtons();
        yield return new WaitForSecondsRealtime(delay);
        isShopOpen = false;
        IsShopOpen = false;
        Time.timeScale = 1f;
    }


    private void SpawnItemButtons()
    {
        DestroyItemButtons();

        if (selectedItems.Count == 0)
        {
            Debug.LogWarning("No items available to display in shop.");
            shopUI.SetActive(false);
            return;
        }
        for (int i = 0; i < spawnPoints.Length && i < selectedItems.Count; i++)
        {
            GameObject selectedItem = selectedItems[i];

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