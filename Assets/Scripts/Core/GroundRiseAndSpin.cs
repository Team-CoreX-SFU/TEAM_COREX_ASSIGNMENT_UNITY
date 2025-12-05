using System.Collections;
using UnityEngine;

/// <summary>
/// When the player touches this trigger (e.g. ground outside),
/// a target object will rise from its current position to a target position
/// and then spin forever.
/// </summary>
[RequireComponent(typeof(Collider))]
public class GroundRiseAndSpin : MonoBehaviour
{
    [Header("Player Detection")]
    [Tooltip("Tag assigned to the player's root object (usually XR Origin / Player)")]
    public string playerTag = "Player";

    [Header("Object To Move")]
    [Tooltip("Object that will rise from the ground and then spin")]
    public Transform objectToMove;

    [Tooltip("Target world position where the object should end up")]
    public Transform targetPosition;

    [Header("Movement Settings")]
    [Tooltip("Time in seconds for the object to rise to the target position")]
    public float riseDuration = 1.5f;

    [Tooltip("Curve for the rising motion (optional)")]
    public AnimationCurve riseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Spin Settings")]
    [Tooltip("Degrees per second around Y axis for spinning")]
    public float spinSpeed = 90f;

    private bool hasActivated = false;
    private bool hasFinishedRising = false;

    private void Awake()
    {
        // Ensure this collider is a trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        // Once risen, spin forever
        if (hasFinishedRising && objectToMove != null)
        {
            objectToMove.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasActivated)
            return;

        if (!IsPlayer(other))
            return;

        if (objectToMove == null || targetPosition == null)
        {
            Debug.LogWarning("[GroundRiseAndSpin] objectToMove or targetPosition not assigned.");
            return;
        }

        hasActivated = true;
        StartCoroutine(RaiseObject());
    }

    private IEnumerator RaiseObject()
    {
        Vector3 startPos = objectToMove.position;
        Vector3 endPos = targetPosition.position;

        float elapsed = 0f;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / riseDuration);

            if (riseCurve != null && riseCurve.length > 0)
            {
                t = riseCurve.Evaluate(t);
            }

            objectToMove.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        objectToMove.position = endPos;
        hasFinishedRising = true;
    }

    /// <summary>
    /// Check if the collider belongs to the player (by tag or parent tag).
    /// </summary>
    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        if (other.CompareTag(playerTag))
            return true;

        Transform t = other.transform;
        while (t != null)
        {
            if (t.CompareTag(playerTag))
                return true;
            t = t.parent;
        }

        return false;
    }
}


