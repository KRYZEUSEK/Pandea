using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartManager : MonoBehaviour
{
    [Header("Ustawienia Startowe")]
    [Tooltip("Ile sekund ma trwaæ rozjaœnianie ekranu po za³adowaniu sceny")]
    public float czasRozjasniania = 2.0f;

    [Header("Referencje UI")]
    [Tooltip("Przeci¹gnij tutaj swój czarny obrazek z Canvasa")]
    public Image faderImage;

    private void Start()
    {
        // Sprawdzamy, czy podpi¹³eœ obrazek w Inspektorze
        if (faderImage != null)
        {
            // Na samym starcie wymuszamy, ¿eby obrazek by³ w³¹czony i w 100% nieprzezroczysty
            faderImage.gameObject.SetActive(true);
            Color startColor = faderImage.color;
            startColor.a = 1f;
            faderImage.color = startColor;

            // Odpalamy proces rozjaœniania
            StartCoroutine(RozjasniajEkran());
        }
        else
        {
            Debug.LogWarning("StartManager: Zapomnia³eœ przypisaæ Fader Image!");
        }
    }

    private IEnumerator RozjasniajEkran()
    {
        float currentTime = 0f;
        Color color = faderImage.color;

        // Pêtla dzia³aj¹ca dok³adnie przez wyznaczon¹ liczbê sekund
        while (currentTime < czasRozjasniania)
        {
            currentTime += Time.deltaTime;
            float progress = currentTime / czasRozjasniania;

            // Zmniejszamy Alfê p³ynnie od 1 (czarny) do 0 (przezroczysty)
            color.a = Mathf.Lerp(1f, 0f, progress);
            faderImage.color = color;

            yield return null; // Czekamy na nastêpn¹ klatkê
        }

        // Upewniamy siê, ¿e na sam koniec obraz jest w 100% niewidoczny
        color.a = 0f;
        faderImage.color = color;

        // BARDZO WA¯NE: Wy³¹czamy ca³y obiekt obrazka, 
        // ¿eby "niewidzialna œciana" nie zablokowa³a chodzenia myszk¹!
        faderImage.gameObject.SetActive(false);
    }
}