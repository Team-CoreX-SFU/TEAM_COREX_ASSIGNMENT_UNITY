using System.Collections;
using UnityEngine;

/// <summary>
/// Simple door controller that opens the door by rotating its local Y to a target angle (default 0)
/// with a smooth animation. Intended to be triggered from events like KeyPadScript.OnCodeCorrect.
/// </summary>
public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("Transform of the door object to rotate. If null, uses this GameObject's transform.")]
    public Transform doorTransform;

    [Tooltip("Closed Y rotation (local). This is the angle when the door is closed.")]
    public float closedYAngle = 0f;

    [Tooltip("Open Y rotation (local). This is the angle when the door is fully open (default 0).")]
    public float openYAngle = 0f;

    [Header("Animation")]
    [Tooltip("Duration of the opening animation in seconds.")]
    public float animationDuration = 1.5f;

    [Tooltip("Animation curve for smooth opening (optional).")]
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip openSound;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        // Use own transform if none assigned
        if (doorTransform == null)
        {
            doorTransform = transform;
        }

        // Ensure door starts in closed position
        Vector3 rot = doorTransform.localEulerAngles;
        doorTransform.localEulerAngles = new Vector3(rot.x, closedYAngle, rot.z);
    }

    /// <summary>
    /// Opens the door (rotates Y to openYAngle).
    /// Call this from KeyPadScript.OnCodeCorrect.
    /// </summary>
    public void OpenDoor()
    {
        if (isOpen || isAnimating)
            return;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimateDoor(openYAngle, true));
    }

    /// <summary>
    /// Closes the door (optional, if you ever want to close it again).
    /// </summary>
    public void CloseDoor()
    {
        if (!isOpen || isAnimating)
            return;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimateDoor(closedYAngle, false));
    }

    private IEnumerator AnimateDoor(float targetYAngle, bool opening)
    {
        isAnimating = true;

        // Play sound if available
        if (opening && audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        Vector3 startRot = doorTransform.localEulerAngles;
        float startY = startRot.y;
        float elapsed = 0f;

        // Normalize angles to -180..180 for smooth interpolation
        if (startY > 180f) startY -= 360f;
        if (targetYAngle > 180f) targetYAngle -= 360f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);

            if (animationCurve != null && animationCurve.length > 0)
            {
                t = animationCurve.Evaluate(t);
            }

            float currentY = Mathf.Lerp(startY, targetYAngle, t);
            doorTransform.localEulerAngles = new Vector3(startRot.x, currentY, startRot.z);

            yield return null;
        }

        // Snap exactly to target
        doorTransform.localEulerAngles = new Vector3(startRot.x, targetYAngle, startRot.z);

        isOpen = opening;
        isAnimating = false;
        animationCoroutine = null;
    }
}


