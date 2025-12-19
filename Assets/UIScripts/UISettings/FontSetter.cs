using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UISettings {
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class FontSetter : MonoBehaviour {
        [SerializeField] private FontStyleScriptable fontStyle;

        private void OnEnable() {
            SetFont();
        }

        [ContextMenu("Set Font")]
        private void SetFont() {
            TMP_FontAsset fontAsset = fontStyle.fontAsset;
            TMP_Text textComponent = GetComponent<TMP_Text>();

            if (fontAsset == null) {
                Debug.LogWarning($"Font asset for {fontStyle.name} is not assigned.", this);
            }
            
            textComponent.font = fontAsset;
        }
    }
}