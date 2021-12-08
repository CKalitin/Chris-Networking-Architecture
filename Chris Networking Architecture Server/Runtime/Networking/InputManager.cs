using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager {
    // Keys
    private Dictionary<int, LocalKeyCode> localKeyCodes = new Dictionary<int, LocalKeyCode>();
    public Dictionary<int, LocalKeyCode> LocalKeyCodes { get { return localKeyCodes; } set { localKeyCodes = value; } }

    private List<int> localKeyCodesToReset = new List<int>();
    public List<int> LocalKeyCodesToReset { get { return localKeyCodesToReset; } set { localKeyCodesToReset = value; } }

    // Mouse pos
    private Vector2 mousePos;
    public Vector2 MousePosition { get { return mousePos; } set { mousePos = value; } }

    public struct LocalKeyCode {
        // Constructor to inizialize values
        public LocalKeyCode(int _keyCode) {
            keyCode = _keyCode;
            onDown = false;
            onUp = false;
            pressed = false;
        }

        public void SetBools(bool _onDown, bool _onUp, bool _pressed) {
            onDown = _onDown;
            onUp = _onUp;
            pressed = _pressed;
        }

        public int keyCode;
        public bool onDown;
        public bool onUp;
        public bool pressed;
    }

    public void SetTCPInput(List<int> _keysDown, List<int> _keysUp) {
        // Loop through keysDown and update the LocalKeyCode of that key
        for (int i = 0; i < _keysDown.Count; i++) {
            SetLocalKeyCode(_keysDown[i], true, false, true); // Set onDown true, onUp false, pressed true
            localKeyCodesToReset.Add(_keysDown[i]);
        }

        // Loop through keysUp and update the LocalKeyCode of that key
        for (int i = 0; i < _keysUp.Count; i++) {
            SetLocalKeyCode(_keysUp[i], false, true, false); // Set onDown false, onUp true, pressed false
            localKeyCodesToReset.Add(_keysUp[i]);
        }
    }

    public void SetLocalKeyCode(int _keyCode, bool _onDown, bool _onUp, bool _pressed) {
        LocalKeyCode value; // If there is a value at _keysDown[i] it is copied into this by using out

        if (localKeyCodes.TryGetValue(_keyCode, out value)) {
            value.SetBools(_onDown, _onUp, _pressed); // Set onDown true, onUp false, pressed true
            localKeyCodes[_keyCode] = value; // Update LocalKeyCode at _keysDown[i] to updated Local Key Code (This might not be neccessary if out uses a pointer)
        } else {
            LocalKeyCode newLocalKeyCode = new LocalKeyCode(_keyCode); // Create new KeyCode
            newLocalKeyCode.SetBools(_onDown, _onUp, _pressed); // Set new KeyCode values
            localKeyCodes.Add(_keyCode, newLocalKeyCode); // Put new keycode in dictionary
        }
    }

    public void SetUDPInput(Vector2 _mousePos) {
        mousePos = _mousePos;
    }

    public bool GetKeyDown(KeyCode _keyCode) {
        LocalKeyCode value;

        // See if the is a localKeyCode for key _keyCode
        if (localKeyCodes.TryGetValue((int)_keyCode, out value)) {
            // return onDown of localKeyCode at the index of _keyCode
            return localKeyCodes[(int)_keyCode].onDown;
        }

        return false;
    }

    public bool GetKeyUp(KeyCode _keyCode) {
        LocalKeyCode value;

        // See if the is a localKeyCode for key _keyCode
        if (localKeyCodes.TryGetValue((int)_keyCode, out value)) {
            // return onDown of localKeyCode at the index of _keyCode
            return localKeyCodes[(int)_keyCode].onUp;
        }

        return false;
    }      
    
    public bool GetKey(KeyCode _keyCode) {
        LocalKeyCode value;

        // See if the is a localKeyCode for key _keyCode
        if (localKeyCodes.TryGetValue((int)_keyCode, out value)) {
            // return onDown of localKeyCode at the index of _keyCode
            return localKeyCodes[(int)_keyCode].pressed;
        }

        return false;
    }
}

