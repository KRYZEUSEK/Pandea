using UnityEngine;

namespace VolumeSettings {
    /// <summary>
    /// Gets and applies the volume setting from PlayerPrefs to the AudioSource component.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class VolumeGetter : MonoBehaviour {
        [SerializeField] private VolumeTypeScriptable volumeType;
        private AudioSource audioSource;

        private void OnEnable() {
            audioSource = GetComponent<AudioSource>();
            UpdateVolume();
        }

        internal void UpdateVolume() {
            if (audioSource == null) {
                audioSource = GetComponent<AudioSource>();
            }

            audioSource.volume = PlayerPrefs.GetFloat(volumeType.volumeTypeName, 1.0f);
        }
    }
}