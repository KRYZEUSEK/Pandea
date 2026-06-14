using UnityEngine;
using UnityEngine.UI;

namespace VolumeSettings {
    /// <summary>
    /// Uses a UI Slider to set and save the volume setting in PlayerPrefs.
    /// </summary>
    public class VolumeSetter : MonoBehaviour {
        [SerializeField] private VolumeTypeScriptable volumeType;
        [SerializeField] private Slider volumeSlider;

        private void OnEnable() {
            volumeSlider.value = PlayerPrefs.GetFloat(volumeType.volumeTypeName, 1.0f);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        private void SetVolume(float volume) {
            Debug.Log($"Setting {volumeType.volumeTypeName} volume to {volume}");

            if (volume < 0 || volume > 1) {
                Debug.LogError("Volume must be between 0 and 1!");
                return;
            }

            PlayerPrefs.SetFloat(volumeType.volumeTypeName, volume);
            UpdateVolumeGetters();
        }

        private void UpdateVolumeGetters() {
            VolumeGetter[] volumeGetters = FindObjectsOfType<VolumeGetter>(true);

            foreach (VolumeGetter volumeGetter in volumeGetters) {
                volumeGetter.UpdateVolume();
            }
        }
    }
}