using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FlashlightController : MonoBehaviour
{
    public Light flashlightLight;
    public bool hasBattery = false;   // Player must pick battery first
    public bool isOn = false;

    public bool isGrabbed = false; // true when player holds flashlight

    void Start()
    {
        flashlightLight.enabled = false;
    }

    public void ToggleFlashlight()
    {
        // Only allow toggle if battery is inserted AND flashlight is being held
        if (!hasBattery || !isGrabbed) return;

        isOn = !isOn;
        flashlightLight.enabled = isOn;
    }

    public void InsertBattery()
    {
        hasBattery = true;
    }
}