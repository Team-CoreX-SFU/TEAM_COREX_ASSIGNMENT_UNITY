using UnityEngine;

public class XRCursorLock : MonoBehaviour
{
    private bool isLocked = true;

    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        // Unlock with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

        // Re-lock when clicking inside the Game view
        if (!isLocked && Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isLocked = true;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isLocked = false;
    }
}
