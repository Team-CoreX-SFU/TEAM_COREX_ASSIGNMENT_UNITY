using UnityEngine;
using System.Collections;

public class HintCanvasTriggerSingle : MonoBehaviour
{
    [Header("The Hint Canvas related to this trigger")]
    public CanvasGroup hintCanvas;

    [Header("Fade Settings")]
    public float fadeInTime = 0.5f;
    public float fadeOutTime = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(FadeCanvas(hintCanvas, 1f, fadeInTime));  // Fade in
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(FadeCanvas(hintCanvas, 0f, fadeOutTime)); // Fade out
        }
    }

    private IEnumerator FadeCanvas(CanvasGroup cg, float targetAlpha, float duration)
    {
        float startAlpha = cg.alpha;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null;
        }

        cg.alpha = targetAlpha;
    }
}
