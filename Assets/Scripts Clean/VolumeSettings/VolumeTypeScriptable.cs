using UnityEngine;

namespace VolumeSettings {
    /// <summary>
    /// The name of the volume type, used as a key in PlayerPrefs.
    /// </summary>
    [CreateAssetMenu(fileName = "NewVolumeType", menuName = "ScriptableObjects/VolumeSettings/VolumeType")]
    public class VolumeTypeScriptable : ScriptableObject {

        public string volumeTypeName = "";
    }
}