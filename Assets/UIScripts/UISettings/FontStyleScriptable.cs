using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UISettings {
    [System.Serializable]
    internal class FontGroup {
        [SerializeField] internal FontName fontName;
        [SerializeField] internal TMP_FontAsset fontAsset;
    }

    [CreateAssetMenu(fileName = "FontStyleScriptable", menuName = "ScriptableObjects/FontStyleScriptable")]
    public class FontStyleScriptable : ScriptableObject {
        private static FontStyleScriptable _instance;

        public static FontStyleScriptable Instance {
            get {
                if (_instance == null) {
                    _instance = Resources.Load<FontStyleScriptable>("FontStyleScriptable");
                }

                return _instance;
            }
        }

        [SerializeField] private List<FontGroup> fontAssets = new();

        internal TMP_FontAsset GetFontAsset(FontName fontName) {
            FontGroup found = fontAssets.Find(f => f.fontName.Equals(fontName));

            if (found == null) {
                Debug.LogError($"Font asset with name {fontName} not found in {nameof(FontStyleScriptable)} instance!");
            }

            return found.fontAsset;
        }
    }
}