using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int pieceID;
    public PuzzleController controller;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private LayoutElement layout;

    private Transform originalParent;
    private bool isPlaced = false;

    public bool IsPlaced => isPlaced;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        layout = GetComponent<LayoutElement>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        PuzzleSlot.SetDraggingPiece(this);

        originalParent = transform.parent;

        canvasGroup.blocksRaycasts = false;
        layout.ignoreLayout = true;

        transform.SetParent(controller.dragLayer, true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            controller.canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        rectTransform.localPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        PuzzleSlot.SetDraggingPiece(null);

        if (!isPlaced)
        {
            transform.SetParent(originalParent, false);
            layout.ignoreLayout = false;
        }
    }

    public void TryPlace(PuzzleSlot slot)
    {
        if (slot.slotID == pieceID)
        {
            isPlaced = true;

            transform.SetParent(slot.transform, false);
            transform.SetAsLastSibling();

            RectTransform rt = GetComponent<RectTransform>();

            //reset the pieces rect transform to the slot's rect transform so it snaps perfectly into place, worth 4hours to find a solotion for this fk 
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = Vector2.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;

            canvasGroup.blocksRaycasts = true;

            slot.SetPlacedColor();
            controller.PiecePlacedCorrect();
        }
    }
}