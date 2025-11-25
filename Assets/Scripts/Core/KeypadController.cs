using UnityEngine;

/// <summary>
/// Controller for keypad system - manages PIN entry and unlock state
/// You can extend this or replace it with your existing keypad implementation
/// </summary>
public class KeypadController : MonoBehaviour
{
    [Header("Keypad Settings")]
    [Tooltip("The correct PIN code")]
    public string correctPin = "1234";

    [Tooltip("Current entered PIN")]
    private string currentPin = "";

    [Tooltip("Is the keypad unlocked?")]
    private bool isUnlocked = false;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnUnlock;
    public UnityEngine.Events.UnityEvent OnLock;

    /// <summary>
    /// Add a digit to the current PIN
    /// </summary>
    public void AddDigit(string digit)
    {
        if (isUnlocked) return;

        if (currentPin.Length < correctPin.Length)
        {
            currentPin += digit;
            CheckPin();
        }
    }

    /// <summary>
    /// Clear the current PIN entry
    /// </summary>
    public void ClearPin()
    {
        currentPin = "";
    }

    /// <summary>
    /// Check if the entered PIN is correct
    /// </summary>
    private void CheckPin()
    {
        if (currentPin == correctPin)
        {
            Unlock();
        }
        else if (currentPin.Length >= correctPin.Length)
        {
            // Wrong PIN, clear and try again
            ClearPin();
        }
    }

    /// <summary>
    /// Unlock the keypad
    /// </summary>
    public void Unlock()
    {
        isUnlocked = true;
        OnUnlock?.Invoke();
        Debug.Log("Keypad unlocked!");
    }

    /// <summary>
    /// Lock the keypad
    /// </summary>
    public void Lock()
    {
        isUnlocked = false;
        currentPin = "";
        OnLock?.Invoke();
    }

    /// <summary>
    /// Get the current PIN being entered
    /// </summary>
    public string GetCurrentPin()
    {
        return currentPin;
    }

    /// <summary>
    /// Check if keypad is unlocked
    /// </summary>
    public bool IsUnlocked()
    {
        return isUnlocked;
    }

    /// <summary>
    /// Set the PIN (for save/load system)
    /// </summary>
    public void SetPin(string pin)
    {
        currentPin = pin;
        if (pin == correctPin)
        {
            isUnlocked = true;
        }
    }
}

