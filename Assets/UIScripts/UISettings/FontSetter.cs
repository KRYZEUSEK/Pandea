using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UISettings {
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class FontSetter : MonoBehaviour {
        [SerializeField] private FontName fontName;

        private void OnEnable() {
            SetFont();
        }

        [ContextMenu("Set Font")]
        private void SetFont() {
            TMP_FontAsset fontAsset = FontStyleScriptable.Instance.GetFontAsset(fontName);
            TMP_Text textComponent = GetComponent<TMP_Text>();

            if (fontAsset != null) {
                textComponent.font = fontAsset;
            }
        }
    }
}