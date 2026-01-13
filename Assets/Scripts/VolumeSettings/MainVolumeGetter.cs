using UnityEngine;

namespace VolumeSettings {
    /// <summary>
    /// Gets and applies the main volume setting from PlayerPrefs to the AudioListener component.
    /// </summary>
    public class MainVolumeGetter : MonoBehaviour {
        [SerializeField] private VolumeTypeScriptable volumeType;

        private void OnEnable() {
            UpdateVolume();
        }

        internal void UpdateVolume() {
            AudioListener.volume = PlayerPrefs.GetFloat(volumeType.volumeTypeName, 1.0f);
        }
    }
}