using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles radio toggle input (keyboard, controller or XR action).
/// </summary>
public class RadioInput : MonoBehaviour
{
    public RadioController radio;
    public InputActionProperty toggleAction;

    void OnEnable() => toggleAction.action.Enable();
    void OnDisable() => toggleAction.action.Disable();

    void Update()
    {
        if (radio == null)
        {
            return;
        }

        if (toggleAction.action.WasPressedThisFrame())
        {
            radio.ToggleRadio();
        }
    }
}

