using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightInput : MonoBehaviour
{
    public FlashlightController flashlight;
    public InputActionProperty toggleAction;

    void OnEnable() => toggleAction.action.Enable();
    void OnDisable() => toggleAction.action.Disable();

    void Update()
    {
        if (toggleAction.action.WasPressedThisFrame())
        {
            flashlight.ToggleFlashlight();
        }
    }
}
