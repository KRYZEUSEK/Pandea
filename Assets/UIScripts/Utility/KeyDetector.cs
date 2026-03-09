using UnityEngine;
using UnityEngine.Events;

namespace UIScripts.Utility {
    public class KeyDetector : MonoBehaviour {
        [SerializeField] private KeyCode keyToDetect;
        [SerializeField] private UnityEvent onKeyPressed;
        [SerializeField] private UnityEvent onKeyHold;
        [SerializeField] private UnityEvent onKeyReleased;

        void Update() {
            if (Input.GetKeyDown(keyToDetect)) {
                onKeyPressed?.Invoke();
            }

            if (Input.GetKey(keyToDetect)) {
                onKeyHold?.Invoke();
            }

            if (Input.GetKeyUp(keyToDetect)) {
                onKeyReleased?.Invoke();
            }
        }
    }
}