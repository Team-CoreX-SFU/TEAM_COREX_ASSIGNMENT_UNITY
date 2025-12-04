using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// VR Keypad Script - Manages keypad input and validation
/// Supports both VR (XR Interaction) and mouse input for testing
/// </summary>
public class KeyPadScript : MonoBehaviour
{
    [Header("Keypad Settings")]
    [Tooltip("Maximum code length (number of digits)")]
    public int maxCodeLength = 3;

    [Tooltip("The correct code to unlock")]
    public string correctCode = "152";

    [Header("Display")]
    [Tooltip("GameObject with TextMeshPro component to display the entered code")]
    public GameObject Screen;

    [Header("Audio (Optional)")]
    [Tooltip("Audio source for button press sounds")]
    public AudioSource audioSource;

    [Tooltip("Sound to play when button is pressed")]
    public AudioClip buttonPressSound;

    [Tooltip("Sound to play when code is correct")]
    public AudioClip correctSound;

    [Tooltip("Sound to play when code is wrong")]
    public AudioClip wrongSound;

    [Header("Events")]
    [Tooltip("Called when the correct code is entered")]
    public UnityEngine.Events.UnityEvent OnCodeCorrect;

    [Tooltip("Called when a wrong code is entered")]
    public UnityEngine.Events.UnityEvent OnCodeWrong;

    [Header("Door (Optional)")]
    [Tooltip("First door transform to rotate when the correct code is entered")]
    public Transform doorTransform;

    [Tooltip("Second door transform (optional) to also rotate when the code is correct")]
    public Transform secondDoorTransform;

    [Tooltip("Target local Y rotation for the FIRST door when opened (e.g. 0)")]
    public float door1OpenYAngle = 0f;

    [Tooltip("Target local Y rotation for the SECOND door when opened (e.g. 0)")]
    public float door2OpenYAngle = 0f;

    [Tooltip("Duration of the door opening animation in seconds")]
    public float doorOpenDuration = 1.5f;

    [Tooltip("Animation curve for door opening (optional)")]
    public AnimationCurve doorOpenCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Internal state
    private int[] enteredCode;
    private int currentPresses = 0;
    private TMPro.TextMeshPro screenTextComponent;
    private bool isProcessing = false;
    private bool doorOpened = false;

    void Start()
    {
        // Initialize code array
        enteredCode = new int[maxCodeLength];
        ResetCode();

        // Get TextMeshPro component from screen
        if (Screen != null)
        {
            screenTextComponent = Screen.GetComponent<TMPro.TextMeshPro>();
            if (screenTextComponent == null)
            {
                Debug.LogError("[KeyPadScript] Screen GameObject does not have a TextMeshPro component!");
            }
        }
        else
        {
            Debug.LogError("[KeyPadScript] Screen GameObject is not assigned!");
        }

        // Update display
        UpdateDisplay();
    }

    void Update()
    {
        // Keep display updated (for visual feedback)
        UpdateDisplay();

        // Legacy mouse input support (for testing in editor)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10))
            {
                if (hit.transform.gameObject.name != "Base")
                {
                    Number numberComponent = hit.transform.gameObject.GetComponent<Number>();
                    if (numberComponent != null)
                    {
                        OnButtonPressed(numberComponent.number);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called by VRKeypadButton when a button is pressed in VR
    /// </summary>
    public void OnButtonPressed(int number)
    {
        if (isProcessing) return; // Prevent multiple rapid presses

        // Play button press sound
        PlaySound(buttonPressSound);

        // Check if we can add more digits
        if (currentPresses < maxCodeLength)
        {
            enteredCode[currentPresses] = number;
            currentPresses++;
            UpdateDisplay();

            Debug.Log($"[KeyPadScript] Button {number} pressed. Current input: {GetCurrentCodeString()}");

            // Check if we've reached max length
            if (currentPresses >= maxCodeLength)
            {
                ValidateCode();
            }
        }
        else
        {
            Debug.Log("[KeyPadScript] Max code length reached. Code will be validated automatically.");
        }
    }

    /// <summary>
    /// Validates the entered code against the correct code
    /// </summary>
    private void ValidateCode()
    {
        isProcessing = true;
        string enteredCodeString = GetCurrentCodeString();

        Debug.Log($"[KeyPadScript] Validating code. Entered: '{enteredCodeString}', Correct: '{correctCode}'");

        if (enteredCodeString == correctCode)
        {
            // Correct code!
            Debug.Log("[KeyPadScript] ✓ CORRECT CODE ENTERED!");
            PlaySound(correctSound);
            OnCodeCorrect?.Invoke();

            // Open door(s) if assigned
            if (!doorOpened && (doorTransform != null || secondDoorTransform != null))
            {
                doorOpened = true;

                if (doorTransform != null)
                {
                    StartCoroutine(OpenDoorAnimation(doorTransform, door1OpenYAngle));
                }

                if (secondDoorTransform != null)
                {
                    StartCoroutine(OpenDoorAnimation(secondDoorTransform, door2OpenYAngle));
                }
            }
        }
        else
        {
            // Wrong code
            Debug.Log("[KeyPadScript] ✗ WRONG CODE!");
            PlaySound(wrongSound);
            ShowWrongMessage();
            OnCodeWrong?.Invoke();

            // Reset after showing wrong message
            StartCoroutine(ResetAfterDelay(1.5f));
        }

        isProcessing = false;
    }

    /// <summary>
    /// Smoothly rotates the given door's local Y to the given target angle.
    /// This uses the door's existing pivot; no extra hinge object is required.
    /// </summary>
    private IEnumerator OpenDoorAnimation(Transform targetDoor, float targetYAngle)
    {
        if (targetDoor == null)
            yield break;

        Vector3 startRot = targetDoor.localEulerAngles;
        float startY = startRot.y;
        float targetY = targetYAngle;

        // Normalize to -180..180 for smooth interpolation
        if (startY > 180f) startY -= 360f;
        if (targetY > 180f) targetY -= 360f;

        float elapsed = 0f;

        while (elapsed < doorOpenDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / doorOpenDuration);

            if (doorOpenCurve != null && doorOpenCurve.length > 0)
            {
                t = doorOpenCurve.Evaluate(t);
            }

            float currentY = Mathf.Lerp(startY, targetY, t);
            targetDoor.localEulerAngles = new Vector3(startRot.x, currentY, startRot.z);

            yield return null;
        }

        // Snap exactly to target
        targetDoor.localEulerAngles = new Vector3(startRot.x, targetY, startRot.z);
    }

    /// <summary>
    /// Shows "WRONG" message on the display
    /// </summary>
    private void ShowWrongMessage()
    {
        if (screenTextComponent != null)
        {
            screenTextComponent.text = "WRONG";
            screenTextComponent.color = Color.red;
        }
    }

    /// <summary>
    /// Resets the keypad after a delay
    /// </summary>
    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetCode();
        UpdateDisplay();
    }

    /// <summary>
    /// Resets the entered code
    /// </summary>
    private void ResetCode()
    {
        currentPresses = 0;
        for (int i = 0; i < enteredCode.Length; i++)
        {
            enteredCode[i] = 0;
        }
    }

    /// <summary>
    /// Gets the current entered code as a string
    /// </summary>
    private string GetCurrentCodeString()
    {
        string codeString = "";
        for (int i = 0; i < currentPresses; i++)
        {
            codeString += enteredCode[i].ToString();
        }
        return codeString;
    }

    /// <summary>
    /// Updates the display screen with current input
    /// </summary>
    private void UpdateDisplay()
    {
        if (screenTextComponent == null) return;

        // Only update if not showing "WRONG" message
        if (screenTextComponent.text != "WRONG" || currentPresses == 0)
        {
            string displayText = GetCurrentCodeString();

            // Pad with dashes to show remaining digits
            while (displayText.Length < maxCodeLength)
            {
                displayText += "-";
            }

            screenTextComponent.text = displayText;
            screenTextComponent.color = Color.white;
        }
    }

    /// <summary>
    /// Plays a sound effect if audio source and clip are available
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Public method to manually reset the keypad (useful for testing or external triggers)
    /// </summary>
    public void ResetKeypad()
    {
        ResetCode();
        UpdateDisplay();
    }
}

