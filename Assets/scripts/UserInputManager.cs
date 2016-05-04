using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public enum KeyState {
    UP,
    DOWN,
    PRESSED,
    DOWN_UP,
    PRESSED_UP,
    CONTINUOUS
}

[System.Serializable]
public struct KeyInputData {
    public string name;
    public KeyCode key;
    public KeyState state;

    public KeyInputData(string name, KeyCode key, KeyState state) {
        this.name = name;
        this.key = key;
        this.state = state;
    }
}

public class KeyInputEvent : UnityEvent<string, bool> { }

public class UserInputManager : MonoBehaviour {

    public static UserInputManager instance { get; private set; }

    public List<KeyInputData> KeyEvents;

    public static KeyInputEvent OnKeyInput = new KeyInputEvent();

    public void Awake() {
        if(instance == null)
            instance = this;
        else
            return;
    }

	public void Update() {
        // this is terribly inefficient, but it works :3
        foreach(KeyInputData data in KeyEvents) {
            switch(data.state) {

                case KeyState.UP:
                    if(Input.GetKeyUp(data.key))
                        OnKeyInput.Invoke(data.name, false);
                    break;

                case KeyState.DOWN:
                    if(Input.GetKeyDown(data.key))
                        OnKeyInput.Invoke(data.name, true);
                    break;

                case KeyState.PRESSED:
                    if(Input.GetKey(data.key))
                        OnKeyInput.Invoke(data.name, true);
                    break;

                case KeyState.DOWN_UP:
                    if(Input.GetKeyDown(data.key))
                        OnKeyInput.Invoke(data.name, true);
                    else if(Input.GetKeyUp(data.key))
                        OnKeyInput.Invoke(data.name, false);
                    break;

                case KeyState.PRESSED_UP:
                    if(Input.GetKey(data.key))
                        OnKeyInput.Invoke(data.name, true);
                    else if(Input.GetKeyUp(data.key))
                        OnKeyInput.Invoke(data.name, false);
                    break;

                case KeyState.CONTINUOUS:
                    if(Input.GetKey(data.key))
                        OnKeyInput.Invoke(data.name, true);
                    else
                        OnKeyInput.Invoke(data.name, false);
                    break;
            }
        }
    }
}
