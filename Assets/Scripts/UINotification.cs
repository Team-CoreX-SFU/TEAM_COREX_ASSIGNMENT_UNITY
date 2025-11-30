using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Component for individual UI notifications that can display messages, timers, etc.
/// Automatically handles fade in/out and destruction.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UINotification : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component to display message (auto-found if null)")]
    public TextMeshProUGUI textComponent;
    
    [Tooltip("Image component for icon (optional)")]
    public Image iconImage;
    
    [Tooltip("Background image (auto-found if null)")]
    public Image backgroundImage;
    
    [Header("Animation Settings")]
    [Tooltip("Fade in duration")]
    public float fadeInDuration = 0.3f;
    
    [Tooltip("Fade out duration")]
    public float fadeOutDuration = 0.3f;
    
    [Tooltip("Animation curve for fade")]
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Layout Settings")]
    [Tooltip("Spacing between notifications")]
    public float verticalSpacing = 10f;
    
    private CanvasGroup canvasGroup;
    private bool isTimer = false;
    private float timerDuration;
    private float timerRemaining;
    private string timerLabel;
    private System.Action onTimerComplete;
    private Coroutine timerCoroutine;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        // Get or add CanvasGroup for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Find text component if not assigned
        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Find background image if not assigned
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
        
        // Find icon image
        if (iconImage == null)
        {
            Image[] images = GetComponentsInChildren<Image>();
            foreach (Image img in images)
            {
                if (img != backgroundImage)
                {
                    iconImage = img;
                    break;
                }
            }
        }
        
        // Start with alpha 0 for fade in
        canvasGroup.alpha = 0f;
    }
    
    /// <summary>
    /// Setup a regular notification
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="duration">Duration before auto-hiding. Use 0 or negative to never auto-hide</param>
    /// <param name="icon">Optional icon sprite</param>
    public void Setup(string message, float duration, Sprite icon = null)
    {
        isTimer = false;
        
        if (textComponent != null)
        {
            textComponent.text = message;
        }
        
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }
        
        // Fade in
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeIn());
        
        // Auto-hide after duration (only if duration > 0)
        // Duration of 0 or negative means don't auto-hide
        if (duration > 0)
        {
            StartCoroutine(AutoHide(duration));
        }
        // If duration is 0 or less, notification will stay visible until manually hidden
    }
    
    /// <summary>
    /// Setup a countdown timer
    /// </summary>
    public void SetupTimer(string label, float duration, System.Action onComplete = null)
    {
        isTimer = true;
        timerLabel = label;
        timerDuration = duration;
        timerRemaining = duration;
        onTimerComplete = onComplete;
        
        if (textComponent != null)
        {
            UpdateTimerText();
        }
        
        // Fade in
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeIn());
        
        // Start timer countdown
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(TimerCountdown());
    }
    
    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            canvasGroup.alpha = fadeCurve.Evaluate(t);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, fadeCurve.Evaluate(t));
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        Destroy(gameObject);
    }
    
    private IEnumerator AutoHide(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
    }
    
    private IEnumerator TimerCountdown()
    {
        while (timerRemaining > 0)
        {
            timerRemaining -= Time.deltaTime;
            if (timerRemaining < 0) timerRemaining = 0;
            
            UpdateTimerText();
            yield return null;
        }
        
        // Timer complete
        if (onTimerComplete != null)
        {
            onTimerComplete.Invoke();
        }
        
        Hide();
    }
    
    private void UpdateTimerText()
    {
        if (textComponent != null)
        {
            int seconds = Mathf.CeilToInt(timerRemaining);
            textComponent.text = $"{timerLabel}: {seconds}s";
            
            // Change color as time runs out
            if (timerRemaining < 5f)
            {
                textComponent.color = Color.red;
            }
            else if (timerRemaining < 10f)
            {
                textComponent.color = Color.yellow;
            }
            else
            {
                textComponent.color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// Manually hide this notification
    /// </summary>
    public void Hide()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeOut());
    }
    
    /// <summary>
    /// Update the notification message
    /// </summary>
    public void UpdateMessage(string newMessage)
    {
        if (textComponent != null)
        {
            textComponent.text = newMessage;
        }
    }
    
    /// <summary>
    /// Get remaining time (for timers)
    /// </summary>
    public float GetRemainingTime()
    {
        return isTimer ? timerRemaining : 0f;
    }
}

