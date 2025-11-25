using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple script to attach to Save File Button for easy setup
/// This will automatically connect to GameManager and trigger save
/// </summary>
[RequireComponent(typeof(Button))]
public class SaveButtonHandler : MonoBehaviour
{
    private Button button;
    private GameManager gameManager;

    void Start()
    {
        button = GetComponent<Button>();

        // Find GameManager
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            // Create GameManager if it doesn't exist
            GameObject managerObj = new GameObject("GameManager");
            gameManager = managerObj.AddComponent<GameManager>();
        }

        // Connect button to save function
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                if (gameManager != null)
                {
                    gameManager.SaveGame();
                }
                else
                {
                    Debug.LogError("GameManager not found! Cannot save game.");
                }
            });

            Debug.Log("Save button connected successfully!");
        }
    }
}

