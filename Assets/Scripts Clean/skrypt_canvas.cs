using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class skrypt_canvas : MonoBehaviour
{
    [Header("Ustawienia Kodu")]
    public int[] correctCode = { 4, 7, 5 };
    private int[] currentCode = { 0, 0, 0 };

    [Header("Referencje UI")]
    public TextMeshProUGUI[] digitTexts; // Przeciagnij tu 3 obiekty tekstowe z Canvasu

    [Header("Ustawienia Przejscia Lore")]
    [Tooltip("Czas trwania sciemniania ekranu (w sekundach).")]
    public float fadeDuration = 2.0f;
    [Tooltip("Dokladna nazwa czarnego obrazka FaderImage na scenie.")]
    public string faderObjectName = "FaderImage";

    [HideInInspector]
    public string sceneToLoad = ""; // Wczytywana dynamicznie z pudelka, z ktorym gracz wchodzi w interakcje

    private Image faderImage;
    
    // Mapowanie pozycji: [0] = Lewy, [1] = Srodkowy, [2] = Prawy
    private int[] visualToLogical = { 0, 1, 2 };

    private GameObject FindGameObjectEvenInactive(string goName)
    {
        if (string.IsNullOrEmpty(goName)) return null;

        UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (activeScene.isLoaded)
        {
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            foreach (GameObject rootObj in rootObjects)
            {
                if (rootObj.name == goName) return rootObj;

                Transform[] allChildren = rootObj.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child.name == goName)
                    {
                        return child.gameObject;
                    }
                }
            }
        }
        return null;
    }

    private void Awake()
    {
        InitializeVisualMapping();
    }

    private void InitializeVisualMapping()
    {
        if (digitTexts == null || digitTexts.Length != 3) return;

        // Dynamicznie sortujemy indeksy 0, 1, 2 na podstawie ich pozycji X (od lewej do prawej)
        List<int> indices = new List<int> { 0, 1, 2 };
        indices.Sort((a, b) =>
        {
            if (digitTexts[a] == null || digitTexts[b] == null) return 0;
            return digitTexts[a].transform.position.x.CompareTo(digitTexts[b].transform.position.x);
        });

        visualToLogical[0] = indices[0]; // Lewy wyswietlacz
        visualToLogical[1] = indices[1]; // Srodkowy wyswietlacz
        visualToLogical[2] = indices[2]; // Prawy wyswietlacz

        Debug.Log($"[skrypt_canvas] Zainicjalizowano mapowanie wizualne cyfr: Lewy = indeks {visualToLogical[0]}, Srodkowy = indeks {visualToLogical[1]}, Prawy = indeks {visualToLogical[2]}");
    }
    
    // Ta funkcja bedzie wywolywana przez przyciski pod cyframi
    public void IncrementDigit(int index)
    {
        currentCode[index]++;
        if (currentCode[index] > 9) 
        {
            currentCode[index] = 0; // Zapetlenie po 9 wraca do 0
        }
        UpdateUI();
    }

    // Ta funkcja bedzie wywolywana przez przycisk "Sprawdz"
    public void CheckCode()
    {
        int codeLength = correctCode != null ? correctCode.Length : 0;
        if (codeLength < 3)
        {
            Debug.LogWarning($"skrypt_canvas: correctCode w Inspektorze ma tylko {codeLength} cyfry! Brakujace cyfry traktowane sa jako 0.");
        }

        int val0 = codeLength > 0 ? correctCode[0] : 0;
        int val1 = codeLength > 1 ? correctCode[1] : 0;
        int val2 = codeLength > 2 ? correctCode[2] : 0;

        // Odczytujemy wartosci cyfr w kolejnosci od lewej do prawej na ekranie
        int visualLeftVal = currentCode[visualToLogical[0]];
        int visualMiddleVal = currentCode[visualToLogical[1]];
        int visualRightVal = currentCode[visualToLogical[2]];

        if (visualLeftVal == val0 &&
            visualMiddleVal == val1 &&
            visualRightVal == val2)
        {
            Debug.Log("Kod poprawny! Gra toczy sie dalej.");
            
            string targetScene = "";
            string cutsceneName = "";

            // Usuwamy tag z pudelka (zmieniamy na Untagged) i odczytujemy scene do wczytania z tego konkretnego pudelka
            if (skrypt_pudelka.activePudelko != null)
            {
                skrypt_pudelka.activePudelko.gameObject.tag = "Untagged";
                targetScene = skrypt_pudelka.activePudelko.sceneToLoad;
                cutsceneName = skrypt_pudelka.activePudelko.cutsceneCanvasName;

                // Powiadom QuestManager o ukończeniu kroku
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.CompleteCurrentStep();
                }
            }

            GameObject cutsceneGo = FindGameObjectEvenInactive(cutsceneName);

            // Jeśli przypisano lokalną cutscenkę nakładkową
            if (cutsceneGo != null)
            {
                if (skrypt_pudelka.activePudelko != null)
                {
                    skrypt_pudelka.activePudelko.PrepareForCutscene();
                }

                cutsceneGo.SetActive(true);
                gameObject.SetActive(false); // Zamykamy Canvas kodu, skrypt_pudelka nie odblokuje ruchu dzięki PrepareForCutscene()
                Debug.Log("[skrypt_canvas] Odpalanie lokalnej cutscenki (overlay): " + cutsceneName);
            }
            // Jesli pudelko ma zdefiniowana scene do wczytania, wczytujemy ja z efektem fade-out
            else if (!string.IsNullOrEmpty(targetScene))
            {
                sceneToLoad = targetScene;
                StartCoroutine(FadeAndLoadScene());
            }
            else
            {
                ClosePuzzle();
            }
        }
        else
        {
            Debug.Log($"Bledny kod! Wpisano od lewej do prawej: {visualLeftVal}{visualMiddleVal}{visualRightVal}, a oczekiwano: {val0}{val1}{val2}. Reset.");
            ResetPuzzle();
        }
    }

    private void ZnajdzFaderImage()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            Transform[] allChildren = canvas.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child.name == faderObjectName)
                {
                    faderImage = child.GetComponent<Image>();
                    return;
                }
            }
        }
    }

    private void StworzDynamicznyFader()
    {
        // Szukamy aktywnego Canvasu na scenie
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }

        if (canvas != null)
        {
            GameObject faderObj = new GameObject("DynamicFaderImage");
            faderObj.transform.SetParent(canvas.transform, false);
            
            // Przenosimy na sam wierzch wyswietlania
            faderObj.transform.SetAsLastSibling();

            RectTransform rect = faderObj.AddComponent<RectTransform>();
            faderImage = faderObj.AddComponent<Image>();

            // Rozciagamy obrazek na caly ekran
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            // Dajemy mu przezroczysty czarny kolor
            faderImage.color = new Color(0f, 0f, 0f, 0f);
            
            // Blokujemy klikniecia w UI pod spodem podczas fade-outu
            faderImage.raycastTarget = true;
            
            Debug.Log("[skrypt_canvas] Wykreowano dynamiczny FaderImage na Canvasie.");
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        ZnajdzFaderImage();
        
        // Jesli na scenie nie ma pre-skonfigurowanego Fadera, tworzymy go w locie
        if (faderImage == null)
        {
            StworzDynamicznyFader();
        }

        if (faderImage != null)
        {
            faderImage.gameObject.SetActive(true);
            float currentTime = 0f;
            Color color = faderImage.color;
            color.a = 0f;
            faderImage.color = color;

            while (currentTime < fadeDuration)
            {
                currentTime += Time.deltaTime;
                float progress = currentTime / fadeDuration;
                color.a = Mathf.Lerp(0f, 1f, progress);
                faderImage.color = color;
                yield return null;
            }

            color.a = 1f;
            faderImage.color = color;
        }
        else
        {
            Debug.LogWarning("[skrypt_canvas] Brak faderImage i nie udalo sie go wykreowac. Wczytywanie sceny bez efektu sciemnienia.");
            yield return new WaitForSeconds(fadeDuration);
        }

        // Zamykamy okno przed zaladowaniem nowej sceny
        gameObject.SetActive(false);

        // Wczytujemy scene fabularna zdefiniowana w pudelku
        SceneManager.LoadScene(sceneToLoad);
    }

    private void UpdateUI()
    {
        if (digitTexts == null) return;
        for (int i = 0; i < digitTexts.Length; i++)
        {
            if (digitTexts[i] != null)
            {
                digitTexts[i].text = currentCode[i].ToString();
            }
        }
    }

    private void ResetPuzzle()
    {
        for (int i = 0; i < 3; i++)
        {
            currentCode[i] = 0;
        }
        UpdateUI();
    }

    // Wywolywane tez z przycisku "Zamknij/Wyjdz", jesli gracz chce zrezygnowac
    public void ClosePuzzle() 
    {
        gameObject.SetActive(false);
        
        // Odblokowujemy kursor dla gry Click-to-Move, aby gracz mogl normalnie chodzic i klikac
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnEnable()
    {
        // Resetuje zagadke za kazdym razem, gdy gracz ja otwiera
        ResetPuzzle();
    }
}
