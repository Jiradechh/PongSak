using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrencyDisplay : MonoBehaviour
{
    public TextMeshProUGUI gemText;
    public TextMeshProUGUI goldText;
    public CanvasGroup canvasGroup;

    private void Start()
    {
        CurrencyManager.Instance.OnCurrencyUpdated += UpdateCurrencyDisplayWithFade;
        UpdateCurrencyDisplay();
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyUpdated -= UpdateCurrencyDisplayWithFade;
        }
    }

    private void UpdateCurrencyDisplay()
    {
        gemText.text = $"{CurrencyManager.Instance.gems}";
        goldText.text = $"{CurrencyManager.Instance.gold}";
    }

    private void UpdateCurrencyDisplayWithFade()
    {
        UpdateCurrencyDisplay();
        StartCoroutine(FadeInUI());
    }

    private IEnumerator FadeInUI()
    {
        float duration = 1.0f;
        canvasGroup.alpha = 0;

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null;
        }

        yield return new WaitForSeconds(2.0f);

        elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = 1 - Mathf.Clamp01(elapsedTime / duration);
            yield return null;
        }
    }
}