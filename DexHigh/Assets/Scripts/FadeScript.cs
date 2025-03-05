using System.Collections;
using UnityEngine;

public class FadeScript : MonoBehaviour
{
    [SerializeField] CanvasGroup bgCanvasGroup;  
    [SerializeField] float fadeDuration = 1.5f; 
    private bool isFaded = false;      

    public void ToggleFade()
    {
        StopAllCoroutines();  
        StartCoroutine(FadeCanvas(bgCanvasGroup, isFaded ? 1f : 0f, isFaded ? 0f : 1f));
        isFaded = !isFaded;  
    }

    IEnumerator FadeCanvas(CanvasGroup canvas, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            yield return null;
        }
        canvas.alpha = endAlpha;  
    }
}
