using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class HandMovementToggleInspector: MonoBehaviour
{
    [Header("Assign XR Device Simulator Component")]
    public MonoBehaviour simulator; // drag the XR Device Simulator component here

    [Header("Enable / Disable Hand Manipulation")]
    [SerializeField] public bool handsEnabled = true;

    private InputActionReference manipulateLeft;
    private InputActionReference manipulateRight;

    private void Awake()
    {
        if (simulator == null)
        {
            Debug.LogError("Assign the XR Device Simulator component!");
            return;
        }

        var type = simulator.GetType();

        // Grab the two public InputActionReferences from the component
        manipulateLeft = (InputActionReference)type.GetProperty("manipulateLeftAction").GetValue(simulator);
        manipulateRight = (InputActionReference)type.GetProperty("manipulateRightAction").GetValue(simulator);

        ApplyHandState();
    }

    private void OnValidate()
    {
        if (manipulateLeft != null && manipulateRight != null)
            ApplyHandState();
    }

    public void ApplyHandState()
    {
        if (handsEnabled)
        {
            manipulateLeft.action.Enable();
            manipulateRight.action.Enable();
            Debug.Log("Hand manipulation ENABLED");
        }
        else
        {
            manipulateLeft.action.Disable();
            manipulateRight.action.Disable();
            Debug.Log("Hand manipulation DISABLED");
        }
    }
}
