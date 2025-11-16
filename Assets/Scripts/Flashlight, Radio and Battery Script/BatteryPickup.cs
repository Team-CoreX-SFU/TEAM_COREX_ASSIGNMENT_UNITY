using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class BatteryPickup : MonoBehaviour
{
    public FlashlightController flashlight; // assign the player's flashlight
    public float disappearDelay = 0.1f; // optional small delay before disappearing

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Subscribe to grab events
        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Let the flashlight know battery is inserted
        if (flashlight != null)
        {
            flashlight.InsertBattery();
        }

        // Disable battery object so it disappears
        StartCoroutine(Disappear());
    }

    private System.Collections.IEnumerator Disappear()
    {
        // Optional: wait one frame for XR system to finish grab
        yield return new WaitForSeconds(disappearDelay);

        // Destroy battery object
        Destroy(gameObject);
    }
}
