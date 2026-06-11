using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;



namespace MyGame.Intro
{
    [RequireComponent(typeof(VideoPlayer))]
    public class IntroVideoController : MonoBehaviour
    {
        [Header("Video Clips")]
        [Tooltip("Wideo odtwarzane w pêtli w tle podczas czytania tekstu.")]
        [SerializeField] private VideoClip loopingClip;

        [Tooltip("Wideo odtwarzane na zakoñczenie przed zamkniêciem.")]
        [SerializeField] private VideoClip endingClip;

        [SerializeField] private VideoPlayer videoPlayer;

        private void Awake()
        {
            if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
        }

        // Zmienione na IEnumerator, aby móc poczekaæ na za³adowanie wideo
        public IEnumerator PlayLoopingVideoCoroutine()
        {
            if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null || loopingClip == null) yield break;

            videoPlayer.clip = loopingClip;
            videoPlayer.isLooping = true;

            // 1. Rozpocznij przygotowywanie wideo (³adowanie do pamiêci)
            videoPlayer.Prepare();

            // 2. Czekaj w pêtli, dopóki wideo nie bêdzie w pe³ni gotowe
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            // 3. Odpal wideo natychmiast po za³adowaniu
            Debug.Log("Wideo gotowe i zbuforowane - START!");
            videoPlayer.Play();
        }

        public IEnumerator PlayEndingVideoCoroutine()
        {
            if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null || endingClip == null) yield break;

            videoPlayer.clip = endingClip;
            videoPlayer.isLooping = false;

            // Tutaj te¿ pre-³adujemy drugie wideo, ¿eby nie by³o migniêcia!
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            videoPlayer.Play();
            yield return new WaitForSecondsRealtime((float)endingClip.length);
        }
    }
}