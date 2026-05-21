using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using TMPro;

using UnityEngine.SceneManagement;

namespace MyGame.Intro
{
    public class IntroTypewriterSections : MonoBehaviour
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

            [Tooltip("Kroki w porcji tekstu.")]
            public Step[] steps;

            [Tooltip("Czas przed fade outem.")]
            public float holdAfterTypedSeconds;
        }

        [Header("UI")]
        [SerializeField] private TMP_Text targetText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Sekcje")]
        [SerializeField] private Section[] sections;

        [Header("Pisanie")]
        [SerializeField] private float typeSecondsPerChar = 0.05f;

        [Header("Fade out")]
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

        private Coroutine routine;

        private void Reset()
        {
            targetText = GetComponentInChildren<TMP_Text>(true);
            canvasGroup = (targetText != null) ? targetText.GetComponent<CanvasGroup>() : null;
        }

        private void Awake()
        {
            if (targetText == null)
                Debug.LogError($"{nameof(IntroTypewriterSections)}: Missing TMP_Text reference.", this);

            if (canvasGroup == null)
                Debug.LogError($"{nameof(IntroTypewriterSections)}: Missing CanvasGroup reference.", this);
        }

        private void OnEnable()
        {
            routine = StartCoroutine(Play());
        }

        private void OnDisable()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
        }

        private IEnumerator Play()
        {
            if (targetText == null || canvasGroup == null) yield break;
            if (sections == null || sections.Length == 0) yield break;

           
            canvasGroup.alpha = 0f;
            targetText.text = string.Empty;

            for (int s = 0; s < sections.Length; s++)
            {
                
                targetText.text = string.Empty;

                
                yield return Fade(canvasGroup, 0f, 1f, fadeInSeconds);

                
                var section = sections[s];
                var steps = section.steps;

                if (steps != null)
                {
                    for (int i = 0; i < steps.Length; i++)
                    {
                        float pause = Mathf.Max(0f, steps[i].pauseBeforeSeconds);
                        yield return WaitRealtime(pause);

                        if (steps[i].newLineBefore && targetText.text.Length > 0)
                            targetText.text += "\n";

                        yield return TypeAppend(steps[i].text ?? string.Empty);
                    }
                }

               
                float hold = Mathf.Max(0f, section.holdAfterTypedSeconds);
                yield return WaitRealtime(hold);

               
                yield return Fade(canvasGroup, 1f, 0f, fadeOutSeconds);

               
                yield return WaitRealtime(holdAfterFadeSeconds);
            }

            if (!string.IsNullOrWhiteSpace(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
        }

        private IEnumerator TypeAppend(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
                yield break;

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
            if (seconds <= 0f)
            {
                cg.alpha = to;
                yield break;
            }

            float t = 0f;
            cg.alpha = from;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / seconds);
                cg.alpha = Mathf.Lerp(from, to, p);
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
    }
}