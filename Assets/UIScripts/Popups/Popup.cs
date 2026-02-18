using System.Collections;
using UnityEngine;

namespace UIScripts.Popups {
    public class Popup : MonoBehaviour {
        [Tooltip("Audio source for popup sound effects.")]
        [SerializeField] protected AudioSource popupAudioSource;
        [SerializeField] protected AudioClip popupShowClip, popupHideClip;

        protected float originialTimeScale;
        protected bool isPopupActive;

        private void OnEnable() {
            Show();
        }

        public virtual void Show() {
            if (isPopupActive) { return; }

            isPopupActive = true;
            originialTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            gameObject.SetActive(true);

            if (popupAudioSource != null && popupShowClip != null) {
                popupAudioSource.PlayOneShot(popupShowClip);
            }
        }

        public virtual void Hide() {
            if (isPopupActive == false) { return; }
            
            Time.timeScale = originialTimeScale;
            isPopupActive = false;

            if (popupAudioSource != null && popupHideClip != null) {
                StartCoroutine(HideCoroutine());
            }
            else {
                gameObject.SetActive(false);
            }
        }

        protected virtual IEnumerator HideCoroutine() {
            popupAudioSource.PlayOneShot(popupHideClip);
            yield return new WaitForSecondsRealtime(popupHideClip.length);
            Debug.Log($"Finished after: {popupHideClip.length} seconds.");
            gameObject.SetActive(false);
        }

        protected virtual void UpdatePopup() { }

        private void Update() {
            UpdatePopup();
        }
    }
}
