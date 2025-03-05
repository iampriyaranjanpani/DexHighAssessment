using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class Element : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] UiAnimation uiAnimation;
    public GameObject tab;
    public int buttonIndex;
    private Button button;
    private void Start()
    {
        button = gameObject.GetComponent<Button>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (uiAnimation.isActivated)
        {
        if (buttonIndex == 2) return; 

        int shiftAmount = 2 - buttonIndex; 
            StartCoroutine(ShiftWithLock(shiftAmount));
        }

    }

    private IEnumerator ShiftWithLock(int shiftAmount)
    {
        button.interactable = false; 
        uiAnimation.ShiftOrder(shiftAmount);

        yield return new WaitForSeconds(0.5f); 

        button.interactable = true; 
        uiAnimation.UpdateTabState(buttonIndex,this);

    }
}
