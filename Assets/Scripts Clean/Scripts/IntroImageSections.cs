using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityEngine.UI; // Dodano do obs³ugi UI.Image

namespace MyGame.Intro
{
    public class IntroImageSections : MonoBehaviour
    {
        [Serializable]
        public struct Step
        {
            [TextArea(1, 6)]
            public string text;
            [Tooltip("Odczekaj tyle sekund przed t¹ porcj¹.")]
            public float pauseBeforeSeconds;
            [Tooltip("Dodaj nowa linijkê przed sekcj¹.")]
            public bool newLineBefore;
        }

        [Serializable]
        public struct Section
        {
            public string name;

            [Tooltip("Obrazek (Sprite), który pojawi siê w tle podczas tej sekcji.")]
            public Sprite backgroundImage;

            [Tooltip("Kroki w porcji tekstu.")]
            public Step[] steps;
            [Tooltip("Czas przed fade outem.")]
            public float holdAfterTypedSeconds;
        }

        [Header("UI & Image Referencje")]
        [SerializeField] private TMP_Text targetText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("Przeci¹gnij tutaj obiekt BackgroundImage z Canvasu.")]
        [SerializeField] private Image backgroundImageComponent;

        [Header("Sekcje")]
        [SerializeField] private Section[] sections;

        [Header("Pisanie")]
        [SerializeField] private float typeSecondsPerChar = 0.05f;

        [Header("Fade out (Tekstu)")]
        [SerializeField] private float fadeInSeconds = 0.15f;
        [SerializeField] private float fadeOutSeconds = 0.6f;
        [SerializeField] private float holdAfterFadeSeconds = 0.15f;

        [Header("Typing SFX")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip blipClip;
        [Range(0f, 1f)] [SerializeField] private float blipVolume = 0.7f;
        [SerializeField] private bool ignoreWhitespaceForBlip = true;
        [SerializeField] private Vector2 blipPitchRange = new Vector2(0.98f, 1.02f);

        [Header("Flow")]
        [SerializeField] private string nextSceneName;

        [Header("In-Scene Overlay Settings")]
        public bool deactivateOnComplete = true;
        [Tooltip("Obiekt do wy³¹czenia po zakoñczeniu.")]
        public GameObject objectToDeactivate;
        public bool pauseTimeDuringCutscene = true;
        public bool restorePlayerMovementOnComplete = true;
        public UnityEvent onComplete;

        private Coroutine routine;
        private float savedTimeScale = 1f;

        private void Reset()
        {
            targetText = GetComponentInChildren<TMP_Text>(true);
            canvasGroup = (targetText != null) ? targetText.GetComponent<CanvasGroup>() : null;
        }

        private void Awake()
        {
            if (targetText == null) Debug.LogError($"{nameof(IntroImageSections)}: Missing TMP_Text reference.", this);
            if (canvasGroup == null) Debug.LogError($"{nameof(IntroImageSections)}: Missing CanvasGroup reference.", this);
            if (backgroundImageComponent == null) Debug.LogWarning($"{nameof(IntroImageSections)}: Missing Image reference for background.", this);
        }

        private void OnEnable()
        {
            if (pauseTimeDuringCutscene)
            {
                savedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            routine = StartCoroutine(Play());
        }

        private void OnDisable()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;

            if (pauseTimeDuringCutscene)
            {
                Time.timeScale = savedTimeScale;
            }
        }

        private IEnumerator Play()
        {
            if (targetText == null || canvasGroup == null || sections == null || sections.Length == 0) yield break;

            canvasGroup.alpha = 0f;
            targetText.text = string.Empty;

            for (int s = 0; s < sections.Length; s++)
            {
                var section = sections[s];

                // ZMIANA T£A: Ustawiamy obrazek zdefiniowany dla tej konkretnej sekcji
                if (backgroundImageComponent != null && section.backgroundImage != null)
                {
                    backgroundImageComponent.sprite = section.backgroundImage;
                }

                targetText.text = string.Empty;

                // Fade in tekstu
                yield return Fade(canvasGroup, 0f, 1f, fadeInSeconds);

                if (section.steps != null)
                {
                    for (int i = 0; i < section.steps.Length; i++)
                    {
                        float pause = Mathf.Max(0f, section.steps[i].pauseBeforeSeconds);
                        yield return WaitRealtime(pause);

                        if (section.steps[i].newLineBefore && targetText.text.Length > 0)
                            targetText.text += "\n";

                        yield return TypeAppend(section.steps[i].text ?? string.Empty);
                    }
                }

                float hold = Mathf.Max(0f, section.holdAfterTypedSeconds);
                yield return WaitRealtime(hold);

                // Fade out tekstu
                yield return Fade(canvasGroup, 1f, 0f, fadeOutSeconds);
                yield return WaitRealtime(holdAfterFadeSeconds);
            }

            // ZAKOÑCZENIE
            if (!string.IsNullOrWhiteSpace(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                if (restorePlayerMovementOnComplete) RestorePlayerMovement();
                onComplete?.Invoke();

                if (deactivateOnComplete)
                {
                    GameObject targetDeactivate = (objectToDeactivate != null) ? objectToDeactivate : gameObject;
                    targetDeactivate.SetActive(false);
                }
            }
        }

        private void RestorePlayerMovement()
        {
            GameObject realPlayer = GameObject.FindGameObjectWithTag("Player");
            if (realPlayer != null)
            {
                NavMeshAgent realAgent = realPlayer.GetComponent<NavMeshAgent>();
                PlayerControllerClick1 realController = realPlayer.GetComponent<PlayerControllerClick1>();

                if (realAgent != null)
                {
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(realPlayer.transform.position, out hit, 3.0f, NavMesh.AllAreas))
                    {
                        realPlayer.transform.position = hit.position;
                    }
                    realAgent.enabled = true;
                    realAgent.isStopped = false;
                    if (realAgent.isOnNavMesh) realAgent.ResetPath();
                }

                if (realController != null) realController.enabled = true;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private IEnumerator TypeAppend(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend)) yield break;
            for (int i = 0; i < textToAppend.Length; i++)
            {
                char c = textToAppend[i];
                targetText.text += c;
                PlayBlipIfNeeded(c);
                yield return new WaitForSecondsRealtime(typeSecondsPerChar);
            }
        }

        private void PlayBlipIfNeeded(char typedChar)
        {
            if (sfxSource == null || blipClip == null) return;
            if (ignoreWhitespaceForBlip && char.IsWhiteSpace(typedChar)) return;

            float prevPitch = sfxSource.pitch;
            sfxSource.pitch = UnityEngine.Random.Range(blipPitchRange.x, blipPitchRange.y);
            sfxSource.PlayOneShot(blipClip, blipVolume);
            sfxSource.pitch = prevPitch;
        }

        private IEnumerator Fade(CanvasGroup cg, float from, float to, float seconds)
        {
            if (seconds <= 0f) { cg.alpha = to; yield break; }
            float t = 0f;
            cg.alpha = from;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / seconds));
                yield return null;
            }
            cg.alpha = to;
        }

        private IEnumerator WaitRealtime(float seconds)
        {
            if (seconds <= 0f) yield break;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SkipCutscene();
            }
        }

        private void SkipCutscene()
        {
            Debug.Log("[Cutscene] Skipped by player pressing Space.");
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            if (!string.IsNullOrWhiteSpace(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                if (restorePlayerMovementOnComplete) RestorePlayerMovement();
                onComplete?.Invoke();

                if (deactivateOnComplete)
                {
                    GameObject targetDeactivate = (objectToDeactivate != null) ? objectToDeactivate : gameObject;
                    targetDeactivate.SetActive(false);
                }
            }
        }
    }
}