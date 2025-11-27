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
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeActions();
    }

    private void OnEnable()
    {
        // Re-apply state when component is enabled
        if (isInitialized)
        {
            ApplyHandState();
        }
    }

    private void InitializeActions()
    {
        if (simulator == null)
        {
            Debug.LogWarning("[HandMovementToggleInspector] XR Device Simulator component not assigned. Hand manipulation toggle disabled.");
            return;
        }

        try
        {
            var type = simulator.GetType();

            // Grab the two public InputActionReferences from the component
            var leftActionProperty = type.GetProperty("manipulateLeftAction");
            var rightActionProperty = type.GetProperty("manipulateRightAction");

            if (leftActionProperty != null)
            {
                manipulateLeft = leftActionProperty.GetValue(simulator) as InputActionReference;
            }

            if (rightActionProperty != null)
            {
                manipulateRight = rightActionProperty.GetValue(simulator) as InputActionReference;
            }

            // Validate that we got valid references
            if (manipulateLeft == null || manipulateRight == null)
            {
                Debug.LogWarning("[HandMovementToggleInspector] Could not retrieve InputActionReferences from simulator. Hand manipulation toggle disabled.");
                return;
            }

            // Validate that the actions themselves are not null
            if (manipulateLeft.action == null || manipulateRight.action == null)
            {
                Debug.LogWarning("[HandMovementToggleInspector] InputActions are null. Hand manipulation toggle disabled.");
                return;
            }

            isInitialized = true;
            ApplyHandState();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HandMovementToggleInspector] Error initializing actions: {e.Message}");
            isInitialized = false;
        }
    }

    private void OnValidate()
    {
        // Only apply in edit mode, and only if already initialized
        // OnValidate can be called at inappropriate times in play mode
        if (Application.isPlaying && isInitialized)
        {
            ApplyHandState();
        }
    }

    public void ApplyHandState()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[HandMovementToggleInspector] Cannot apply hand state - not initialized. Call InitializeActions() first.");
            return;
        }

        // Safety checks before accessing actions
        if (manipulateLeft == null || manipulateRight == null)
        {
            Debug.LogWarning("[HandMovementToggleInspector] InputActionReferences are null. Cannot apply hand state.");
            return;
        }

        if (manipulateLeft.action == null || manipulateRight.action == null)
        {
            Debug.LogWarning("[HandMovementToggleInspector] InputActions are null. Cannot apply hand state.");
            return;
        }

        try
        {
            if (handsEnabled)
            {
                // Only enable if not already enabled to avoid unnecessary calls
                if (!manipulateLeft.action.enabled)
                {
                    manipulateLeft.action.Enable();
                }
                if (!manipulateRight.action.enabled)
                {
                    manipulateRight.action.Enable();
                }
                Debug.Log("[HandMovementToggleInspector] Hand manipulation ENABLED");
            }
            else
            {
                // Only disable if currently enabled to avoid unnecessary calls
                if (manipulateLeft.action.enabled)
                {
                    manipulateLeft.action.Disable();
                }
                if (manipulateRight.action.enabled)
                {
                    manipulateRight.action.Disable();
                }
                Debug.Log("[HandMovementToggleInspector] Hand manipulation DISABLED");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HandMovementToggleInspector] Error applying hand state: {e.Message}");
        }
    }

    /// <summary>
    /// Re-initialize the action references (useful if simulator is assigned at runtime)
    /// </summary>
    public void Reinitialize()
    {
        isInitialized = false;
        InitializeActions();
    }
}
