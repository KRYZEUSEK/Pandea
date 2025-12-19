using UnityEngine;
using TMPro;

namespace UISettings {
    [CreateAssetMenu(fileName = "FontStyleScriptable", menuName = "ScriptableObjects/FontStyleScriptable")]
    public class FontStyleScriptable : ScriptableObject {
        public string styleName = "";
        public TMP_FontAsset fontAsset;
    }
}