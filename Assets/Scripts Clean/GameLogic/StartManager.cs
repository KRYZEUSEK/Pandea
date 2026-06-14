using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartManager : MonoBehaviour
{
    [Header("Ustawienia Startowe")]
    [Tooltip("Ile sekund ma trwaÄ‡ rozjaĹ›nianie ekranu po zaĹ‚adowaniu sceny")]
    public float czasRozjasniania = 2.0f;

    [Tooltip("Czy rozjaĹ›nianie ma siÄ™ rozpoczÄ…Ä‡ automatycznie przy starcie sceny?")]
    public bool autoStart = true;

    [Header("Referencje UI")]
    [Tooltip("PrzeciÄ…gnij tutaj swĂłj czarny obrazek z Canvasa (np. EndFade)")]
    public Image faderImage;

    private bool hasStarted = false;

    private void Start()
    {
        // Sprawdzamy, czy podpiÄ…Ĺ‚eĹ› obrazek w Inspektorze
        if (faderImage != null)
        {
            // Na samym starcie wymuszamy, ĹĽeby obrazek byĹ‚ wĹ‚Ä…czony i w 100% nieprzezroczysty
            faderImage.gameObject.SetActive(true);
            Color startColor = faderImage.color;
            startColor.a = 1f;
            faderImage.color = startColor;

            if (autoStart)
            {
                TriggerFadeIn();
            }
        }
        else
        {
            Debug.LogWarning("<color=red>[StartManager]</color> ZapomniaĹ‚eĹ› przypisaÄ‡ Fader Image w Inspektorze!");
        }
    }

    public void TriggerFadeIn()
    {
        if (hasStarted) return;
        hasStarted = true;

        if (faderImage != null)
        {
            StartCoroutine(RozjasniajEkran());
        }
    }

    private IEnumerator RozjasniajEkran()
    {
        Debug.Log("<color=yellow>[StartManager]</color> Rozpoczynam rozjaĹ›nianie ekranu!");

        float currentTime = 0f;
        Color color = faderImage.color;

        // PÄ™tla dziaĹ‚ajÄ…ca dokĹ‚adnie przez wyznaczonÄ… liczbÄ™ sekund
        while (currentTime < czasRozjasniania)
        {
            // BARDZO WAĹ»NE: unscaledDeltaTime ignoruje pauzÄ™ (Time.timeScale = 0)
            currentTime += Time.unscaledDeltaTime;

            float progress = currentTime / czasRozjasniania;

            // Zmniejszamy AlfÄ™ pĹ‚ynnie od 1 (czarny) do 0 (przezroczysty)
            color.a = Mathf.Lerp(1f, 0f, progress);
            faderImage.color = color;

            yield return null; // Czekamy na nastÄ™pnÄ… klatkÄ™
        }

        Debug.Log("<color=green>[StartManager]</color> RozjaĹ›nianie zakoĹ„czone. WyĹ‚Ä…czam czarny ekran.");

        // Upewniamy siÄ™, ĹĽe na sam koniec obraz jest w 100% niewidoczny
        color.a = 0f;
        faderImage.color = color;

        // WyĹ‚Ä…czamy caĹ‚y obiekt obrazka, ĹĽeby "niewidzialna Ĺ›ciana" nie zablokowaĹ‚a chodzenia myszkÄ…!
        faderImage.gameObject.SetActive(false);
    }
}
