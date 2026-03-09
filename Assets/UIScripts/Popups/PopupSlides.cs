using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.Popups {
    [System.Serializable]
    internal class Slide {
        public Sprite picture;
        [TextArea(1, int.MaxValue)]
        public string text;
        public AudioClip clip;
    }

    public class PopupSlides : Popup {
        [Tooltip("Reference to the Image, which will show slide sprites")]
        [SerializeField] private Image imageArea;
        [Tooltip("Reference to the Text, which will show slide text")]
        [SerializeField] private TMP_Text textArea;

        [Tooltip("If true, the slide will change after the user clicks. If false, the slide will change after secondsPerSlide.")]
        [SerializeField] private bool changeSlideAfterClick = false;
        [Tooltip("If true, the slides will loop back to the first slide after the last slide. If false, the slides will stop at the last slide.")]
        [SerializeField] private bool loopSlides = false;
        [Tooltip("The number of seconds each slide will be shown before switching to the next slide. Only used if changeSlideAfterClick is false.")]
        [SerializeField] private float secondsPerSlide = 1f;

        [SerializeField] private List<Slide> slides = new List<Slide>();

        private int currentSlideIndex = 0;
        private float timeSinceLastSlideChange = 0f;

        private void ShowSlide(int index) {
            if (index < 0 || index >= slides.Count) { return; }

            popupAudioSource.Stop();

            Slide slide = slides[index];
            timeSinceLastSlideChange = 0f;

            if (slide.picture != null) { 
                imageArea.sprite = slide.picture;
            }

            if (slide.text != "") {
                textArea.text = slide.text;
            }

            if (slide.clip != null) { 
                popupAudioSource.PlayOneShot(slide.clip);
            }

            currentSlideIndex = index;
        }

        public void ShowNextSlide() {
            if (currentSlideIndex + 1 >= slides.Count) {
                if (loopSlides) {
                    ShowSlide(0);
                }

                return;
            }

            ShowSlide(currentSlideIndex + 1);
        }

        public void ShowPreviousSlide() {
            if (currentSlideIndex - 1 < 0) { return; }

            ShowSlide(currentSlideIndex - 1);
        }

        override public void Show() {
            ShowSlide(0);
            base.Show();
        }

        override protected void UpdatePopup() {
            HandleTimer();
        }

        private void HandleTimer() {
            if (changeSlideAfterClick) { return; }

            timeSinceLastSlideChange += Time.unscaledDeltaTime;

            if (timeSinceLastSlideChange >= secondsPerSlide) {
                if (currentSlideIndex + 1 >= slides.Count) {
                    if (loopSlides == false) {
                        changeSlideAfterClick = true;
                        return;
                    }
                }

                ShowNextSlide();
            }
        }
    }
}