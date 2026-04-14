using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Popups
{

    [System.Serializable]
    internal class Slide
    {
        public Sprite picture;
        [TextArea(1, int.MaxValue)]
        public string text;
        public AudioClip clip;
    }

    // NOWE: Struktura wi¹¿¹ca slajd z unikalnym tekstem (ID)
    [System.Serializable]
    internal class SpecialSlide
    {
        [Tooltip("Unikalne ID, po którym bêdziesz wywo³ywaæ ten slajd z kodu (np. 'SecretShip')")]
        public string id;
        public Slide slideData;
    }

    public class PopupSlides : Popup
    {

        [Header("UI References")]
        [Tooltip("Reference to the Image, which will show slide sprites")]
        [SerializeField] private Image imageArea;
        [Tooltip("Reference to the Text, which will show slide text")]
        [SerializeField] private TMP_Text textArea;

        [Header("Sequence Settings")]
        [Tooltip("If true, the slide will change after the user clicks. If false, the slide will change after secondsPerSlide.")]
        [SerializeField] private bool changeSlideAfterClick = false;
        [Tooltip("If true, the slides will loop back to the first slide after the last slide. If false, the slides will stop at the last slide.")]
        [SerializeField] private bool loopSlides = false;
        [Tooltip("The number of seconds each slide will be shown before switching to the next slide. Only used if changeSlideAfterClick is false.")]
        [SerializeField] private float secondsPerSlide = 1f;

        [Header("Slides Data")]
        [SerializeField] private List<Slide> slides = new List<Slide>();

        // NOWE: Oddzielna lista w Inspektorze tylko na specjalne slajdy
        [Tooltip("Slajdy poza g³ówn¹ sekwencj¹, wywo³ywane rêcznie przez kod.")]
        [SerializeField] private List<SpecialSlide> specialSlides = new List<SpecialSlide>();

        private int currentSlideIndex = 0;
        private float timeSinceLastSlideChange = 0f;

        // NOWE: Flaga blokuj¹ca zachowanie g³ównej sekwencji
        private bool isShowingSpecialSlide = false;

        public static PopupSlides Instance;

        private void Awake()
        {
            Instance = this;
        }

        // NOWE: Zunifikowana metoda odœwie¿ania UI. U¿ywana przez zwyk³e i specjalne slajdy.
        private void ApplySlideData(Slide slide)
        {
            if (popupAudioSource != null) popupAudioSource.Stop();

            timeSinceLastSlideChange = 0f;

            if (slide.picture != null && imageArea != null)
            {
                imageArea.sprite = slide.picture;
            }

            if (slide.text != "" && textArea != null)
            {
                textArea.text = slide.text;
            }

            if (slide.clip != null && popupAudioSource != null)
            {
                popupAudioSource.PlayOneShot(slide.clip);
            }
        }

        public void ShowSlide(int index)
        {
            if (index < 0 || index >= slides.Count) { return; }

            isShowingSpecialSlide = false; // Odblokowujemy normalny tryb
            currentSlideIndex = index;
            ApplySlideData(slides[index]);
        }

        // NOWE: G³ówna funkcja do odpalania specjalnych slajdów (np. ze skryptu gracza lub Interactable)
        public void ShowSpecialSlide(string slideId)
        {
            SpecialSlide special = specialSlides.Find(s => s.id == slideId);

            if (special != null)
            {
                isShowingSpecialSlide = true;
                ApplySlideData(special.slideData);

                // Jeœli Popup by³ zamkniêty, wywo³ujemy bazow¹ funkcjê Show() z Popup.cs
                if (!isPopupActive)
                {
                    base.Show();
                }
            }
            else
            {
                Debug.LogWarning($"[PopupSlides] Nie znaleziono specjalnego slajdu o ID: {slideId}");
            }
        }

        public void ShowNextSlide()
        {
            // Jeœli byliœmy w specjalnym slajdzie, klikniêcie powraca do normalnego slajdu (tego przed przerwaniem)
            if (isShowingSpecialSlide)
            {
                ShowSlide(currentSlideIndex);
                return;
            }

            if (currentSlideIndex + 1 >= slides.Count)
            {
                if (loopSlides)
                {
                    ShowSlide(0);
                }
                return;
            }

            ShowSlide(currentSlideIndex + 1);
        }

        public void ShowPreviousSlide()
        {
            if (isShowingSpecialSlide)
            {
                ShowSlide(currentSlideIndex);
                return;
            }

            if (currentSlideIndex - 1 < 0) { return; }
            ShowSlide(currentSlideIndex - 1);
        }

        override public void Show()
        {
            // Jeœli okno otwiera siê naturalnie (nie wymuszono wczeœniej specjalnego slajdu)
            if (!isShowingSpecialSlide)
            {
                ShowSlide(0);
            }
            base.Show();
        }

        override protected void UpdatePopup()
        {
            HandleTimer();
        }

        private void HandleTimer()
        {
            // ZMIANA: Zatrzymujemy auto-prze³¹czanie, jeœli wyœwietla siê specjalny slajd
            if (changeSlideAfterClick || isShowingSpecialSlide) { return; }

            timeSinceLastSlideChange += Time.unscaledDeltaTime;

            if (timeSinceLastSlideChange >= secondsPerSlide)
            {
                if (currentSlideIndex + 1 >= slides.Count)
                {
                    if (loopSlides == false)
                    {
                        changeSlideAfterClick = true;
                        return;
                    }
                }

                ShowNextSlide();
            }
        }
    }
}