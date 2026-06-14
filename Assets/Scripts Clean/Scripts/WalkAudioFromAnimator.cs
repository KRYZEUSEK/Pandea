using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class WalkAudioFromAnimator : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string walkingBoolParam = "isWalking";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip walkClip;

    [Header("Settings")]
    [SerializeField, Tooltip("D³ugoœæ jednego cyklu audio w sekundach.")]
    private float loopDuration = 6f;

    [SerializeField, Tooltip("Restartuje audio od pocz¹tku przy ka¿dym rozpoczêciu chodzenia.")]
    private bool restartOnStart = true;

    private bool wasWalking;
    private float walkTimer;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        if (animator == null || audioSource == null) return;

        bool isWalking = animator.GetBool(walkingBoolParam);

        if (isWalking && !wasWalking)
        {
            walkTimer = 0f;
            if (restartOnStart) audioSource.time = 0f;
            PlayIfPossible();
        }
        else if (!isWalking && wasWalking)
        {
            StopPlayback();
        }

        if (isWalking)
        {
            walkTimer += Time.deltaTime;
            if (walkTimer >= loopDuration)
            {
                walkTimer = 0f;
                RestartPlayback();
            }
        }

        wasWalking = isWalking;
    }

    private void PlayIfPossible()
    {
        if (walkClip == null) return;
        audioSource.clip = walkClip;
        audioSource.Play();
    }

    private void RestartPlayback()
    {
        if (walkClip == null) return;
        audioSource.Stop();
        audioSource.time = 0f;
        audioSource.clip = walkClip;
        audioSource.Play();
    }

    private void StopPlayback()
    {
        audioSource.Stop();
        audioSource.time = 0f;
    }
}