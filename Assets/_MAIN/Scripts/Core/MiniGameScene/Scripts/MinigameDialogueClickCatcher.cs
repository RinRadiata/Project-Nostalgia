using UnityEngine;
using UnityEngine.EventSystems;

public class MinigameDialogueClickCatcher : MonoBehaviour, IPointerDownHandler
{
    public MinigameDialogueBridge bridge;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (bridge == null)
            return;

        eventData.Use();
        bridge.OnPointerContinue();
    }
}