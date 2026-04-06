using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PuzzleSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int slotID;

    private Image slotImage;

    private Color normalColor = new Color(1f, 1f, 1f, 0.3f);
    private Color hoverCorrectColor = new Color(0.2f, 1f, 0.4f, 0.6f);
    private Color hoverWrongColor = new Color(1f, 1f, 1f, 0.6f);
    private Color placedColor = new Color(0.2f, 1f, 0.4f, 0.3f);

    private static PuzzlePiece currentDraggingPiece;

    void Awake()
    {
        slotImage = GetComponent<Image>();
        slotImage.color = normalColor;
    }

    public static void SetDraggingPiece(PuzzlePiece piece)
    {
        currentDraggingPiece = piece;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentDraggingPiece == null) return;

        if (currentDraggingPiece.pieceID == slotID)
            slotImage.color = hoverCorrectColor;
        else
            slotImage.color = hoverWrongColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        slotImage.color = normalColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        slotImage.color = normalColor;

        PuzzlePiece piece = eventData.pointerDrag.GetComponent<PuzzlePiece>();
        if (piece != null)
            piece.TryPlace(this);
    }

    public void SetPlacedColor()
    {
        slotImage.color = placedColor;
    }
}