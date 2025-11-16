using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

[RequireComponent(typeof(XRGrabInteractable))]
public class FlashlightGrabTracker : MonoBehaviour
{
    public FlashlightController flashlight;
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Subscribe to events
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void Start()
    {
        // Wait one frame to ensure XR system initialized
        StartCoroutine(ResetGrabStateNextFrame());
    }

    private IEnumerator ResetGrabStateNextFrame()
    {
        yield return null;
        flashlight.isGrabbed = false;
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        flashlight.isGrabbed = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        flashlight.isGrabbed = false;
    }
}
