using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class TemporaryWin : MonoBehaviour
{
    [Header("Ustawienia Przejœcia")]
    [Tooltip("Dok³adna nazwa sceny, która ma siê za³adowaæ")]
    public string nazwaNowejSceny;

    [Tooltip("Ile sekund ma trwaæ œciemnianie i czekanie na now¹ scenê")]
    public float czasPrzejscia = 3.0f;

    [Header("Referencje UI")]
    [Tooltip("Przeci¹gnij tutaj czarny obrazek z Canvasa")]
    public Image faderImage;

    private bool czyGraczWzasiegu = false;

    private void Start()
    {
        // Upewniamy siê, ¿e na starcie ekran jest w 100% przezroczysty
        if (faderImage != null)
        {
            Color startColor = faderImage.color;
            startColor.a = 0f;
            faderImage.color = startColor;
            faderImage.gameObject.SetActive(false); // Ukrywamy obrazek, ¿eby nie blokowa³ gry
        }
        else
        {
            Debug.LogWarning("Brak przypisanego Fader Image w skrypcie TemporaryWin!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Sprawdzamy, czy to gracz i czy mechanika ju¿ siê nie odpali³a
        if (other.CompareTag("Player") && !czyGraczWzasiegu)
        {
            czyGraczWzasiegu = true;
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        if (faderImage != null)
        {
            faderImage.gameObject.SetActive(true);
            float currentTime = 0f;
            Color color = faderImage.color;

            // Pêtla œciemniaj¹ca dzia³aj¹ca dok³adnie przez 'czasPrzejscia'
            while (currentTime < czasPrzejscia)
            {
                currentTime += Time.deltaTime;

                // Obliczamy postêp od 0.0 do 1.0
                float progress = currentTime / czasPrzejscia;

                // Ustawiamy Alfê (przezroczystoœæ) p³ynnie od 0 do 1
                color.a = Mathf.Lerp(0f, 1f, progress);
                faderImage.color = color;

                yield return null; // Czekamy do nastêpnej klatki
            }

            // Upewniamy siê, ¿e na sam koniec obraz jest w 100% czarny
            color.a = 1f;
            faderImage.color = color;
        }
        else
        {
            // Jeœli zapomnia³eœ podpi¹æ obrazka, gra po prostu odczeka w ukryciu
            yield return new WaitForSeconds(czasPrzejscia);
        }

        // £adujemy now¹ scenê
        SceneManager.LoadScene(nazwaNowejSceny);
    }
}