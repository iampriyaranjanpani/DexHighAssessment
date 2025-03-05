using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UiAnimation : MonoBehaviour
{
    [SerializeField] private Button mainButton;
    private RectTransform mainButtonTransform;

    [SerializeField] private Button[] elementButtons;
    private RectTransformData[] defaultTransform = new RectTransformData[5];
    [SerializeField] private RectTransform[] activatedTransform;

    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private FadeScript fadeScript;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject[] objectsToToggle;

    private bool isOpen = false;
    public bool isActivated = false;

    private List<Button> key;
    int matchedIndex;
    private bool isAnimating = false; // Prevents multiple animations
    private Dictionary<Button, RectTransform> buttonTransformData = new Dictionary<Button, RectTransform>();
    private static Element[] allElements; // Static list to track all elements

    [System.Serializable]
    public struct RectTransformData
    {
        public Vector2 anchoredPosition;
        public Vector3 localScale;
        public Quaternion localRotation;

        public RectTransformData(RectTransform rectTransform)
        {
            anchoredPosition = rectTransform.anchoredPosition;
            localScale = rectTransform.localScale;
            localRotation = rectTransform.localRotation;
        }
    }

    void Start()
    {
        if (fadeScript == null) Debug.LogError("FadeScript is missing!");
        if (elementButtons.Length != activatedTransform.Length) Debug.LogError("Button and transform arrays must match!");

        mainButtonTransform = mainButton.GetComponent<RectTransform>();
        defaultTransform = new RectTransformData[elementButtons.Length];
        key = new List<Button>(elementButtons);

        for (int i = 0; i < elementButtons.Length; i++)
        {
            buttonTransformData[elementButtons[i]] = activatedTransform[i];
            defaultTransform[i] = new RectTransformData(elementButtons[i].GetComponent<RectTransform>());
        }

        // Initialize the list of all elements if not already set
        if (allElements == null)
            allElements = FindObjectsOfType<Element>();
    }

    public void ToggleAnimationPlay()
    {
        if (!mainButton.interactable)
            return;

        mainButton.interactable = false;
        StopAllCoroutines();

        if (isOpen)
        {
            animator.SetBool("IsOpen", false);
            ToggleObjects(false);
            StartCoroutine(AnimateCircular(false));
        }
        else
        {
            animator.SetBool("IsOpen", true);
            ToggleObjects(true);
            StartCoroutine(AnimateCircular(true));
        }

        isOpen = !isOpen;
    }


    private IEnumerator AnimateCircular(bool activating)
    {
        float time = 0;
        Vector2[] startPositions = new Vector2[elementButtons.Length];
        Vector2[] targetPositions = new Vector2[elementButtons.Length];
        Vector3[] startScales = new Vector3[elementButtons.Length];
        Vector3[] targetScales = new Vector3[elementButtons.Length];
        RectTransform[] buttonTransforms = new RectTransform[elementButtons.Length];
        for (int i = 0; i < elementButtons.Length; i++)
        {
            buttonTransforms[i] = elementButtons[i].GetComponent<RectTransform>();
            var buttonRect = elementButtons[i].GetComponent<RectTransform>();
            var savedTransform = buttonTransformData[elementButtons[i]];

            startPositions[i] = activating ? defaultTransform[i].anchoredPosition : savedTransform.anchoredPosition;
            targetPositions[i] = activating ? savedTransform.anchoredPosition : defaultTransform[i].anchoredPosition;
            startScales[i] = activating ? defaultTransform[i].localScale : savedTransform.localScale;
            targetScales[i] = activating ? savedTransform.localScale : defaultTransform[i].localScale;
        }

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, time / animationDuration);

            for (int i = 0; i < elementButtons.Length; i++)
            {
                Vector2 startOffset = startPositions[i] - (Vector2)mainButtonTransform.localPosition;
                Vector2 targetOffset = targetPositions[i] - (Vector2)mainButtonTransform.localPosition;

                float angle = Mathf.LerpAngle(
                    Mathf.Atan2(startOffset.y, startOffset.x) * Mathf.Rad2Deg,
                    Mathf.Atan2(targetOffset.y, targetOffset.x) * Mathf.Rad2Deg, t);

                float radius = Mathf.Lerp(startOffset.magnitude, targetOffset.magnitude, t);

                Vector2 newPos = (Vector2)mainButtonTransform.localPosition +
                                 new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;

                buttonTransforms[i].localPosition = newPos;
                buttonTransforms[i].localScale = Vector3.Lerp(startScales[i], targetScales[i], t);
            }

            yield return null;
        }

        isActivated = activating;
        mainButton.interactable = true;
    }

       

    private void ToggleObjects(bool activateObjects)
    {
        fadeScript.ToggleFade();
        if (activateObjects)
        {
            foreach (GameObject obj in objectsToToggle)
                obj.SetActive(true);
        }
        else
        {
            StartCoroutine(DelayedDeactivation(0.5f));
        }
    }

    private IEnumerator DelayedDeactivation(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (GameObject obj in objectsToToggle)
            obj.SetActive(false);
    }

    public void ShiftOrder(int shiftAmount)
    {
        if (isAnimating || shiftAmount == 0) return; 

        StopAllCoroutines();
        isAnimating = true; // Lock interaction

        int direction = shiftAmount > 0 ? 1 : -1; // Determine direction
        shiftAmount = Mathf.Abs(shiftAmount);

        StartCoroutine(AnimateCircularShift(direction, shiftAmount));
        isAnimating = false; // Unlock after all shifts are done
    }

    private IEnumerator AnimateCircularShift(int direction,int shiftAmount)
    {
        Debug.Log("Animation Started");

        float duration = 0.5f;  
        float time = 0;
        bool isClockWise = direction > 0;

        List<RectTransform> buttonRects = new List<RectTransform>();
        List<Vector2> startPositions = new List<Vector2>();
        List<Vector2> endPositions = new List<Vector2>();

        for (int i = 0; i < key.Count; i++)
        {
            RectTransform rect = key[i].GetComponent<RectTransform>();
            buttonRects.Add(rect);
            startPositions.Add(rect.localPosition);
        }

        int count = key.Count;
        for (int i = 0; i < count; i++)
        {
            //----------
            int newIndex = (i + (direction * shiftAmount) + count) % count;    // Handles wrapping around------------------------IMP
            //----------
            endPositions.Add(startPositions[newIndex]);
        }

        //  midpoints
        Vector2[] midPositions = new Vector2[startPositions.Count];
        for (int i = 0; i < startPositions.Count; i++)
        {
            Vector2 center = (startPositions[i] + endPositions[i]) / 2;
            Vector2 directionVector = endPositions[i] - startPositions[i];
            Vector2 perpendicular = new Vector2(-directionVector.y, directionVector.x).normalized;
            midPositions[i] = center + (perpendicular * Vector2.Distance(startPositions[i], endPositions[i]) / 2 * -direction);
        }


        // Animate button movement
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, time / duration); // smooth

            for (int i = 0; i < buttonRects.Count; i++)
            {
                Vector2 newPos = BezierCurve(startPositions[i], midPositions[i], endPositions[i], t);
                buttonRects[i].localPosition = newPos;
                matchedIndex = FindClosestIndex(newPos);               
                buttonRects[i].localScale = Vector3.Lerp(buttonRects[i].localScale, activatedTransform[matchedIndex].localScale, t);
            }

            yield return null;
        }

        for (int i = 0; i < buttonRects.Count; i++)
        {
            matchedIndex = FindClosestIndex(buttonRects[i].localPosition);
            buttonRects[i].gameObject.GetComponent<Element>().buttonIndex = matchedIndex;

            buttonTransformData[key[i]] = activatedTransform[matchedIndex]; // Save final position-------------------------------
        }

        Debug.Log("Animation Completed");
    }

    private Vector2 BezierCurve(Vector2 start, Vector2 mid, Vector2 end, float t)
    {
        return (1 - t) * (1 - t) * start   // Influence of start point
             + 2 * (1 - t) * t * mid       // Influence of mid point
             + t * t * end;                // Influence of end point
    }

    int FindClosestIndex(Vector2 position)
    {
        int closestIndex = -1;
        float minDistance = float.MaxValue;

        for (int i = 0; i < activatedTransform.Length; i++)
        {
            float distance = Vector2.Distance(position, activatedTransform[i].localPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    public void UpdateTabState(int index , Element gameObject)
    {
        // Deactivate all tabs first
        foreach (Element element in allElements)
        {
            element.tab.SetActive(false);
        }

        // Activate only the tab of the element with buttonIndex == 2
        if (index == 2)
        {
            gameObject.tab.SetActive(true);
        }
    }
}
