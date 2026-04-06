using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PuzzleController : MonoBehaviour
{
    public Transform pieceGrid;
    public Transform dragLayer;
    public RectTransform canvasRect;

    public PuzzlePiece[] allPieces;
    public int totalPieces = 9;

    private int correctPieces = 0;

    void Start()
    {
        StartCoroutine(DelayedShuffle());
    }

    IEnumerator DelayedShuffle()
    {
        yield return null;
        ShufflePieces();
    }

    public void ShufflePieces()
    {
        List<Transform> list = new List<Transform>();

        foreach (Transform t in pieceGrid)
            list.Add(t);

        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            Transform temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }

        for (int i = 0; i < list.Count; i++)
            list[i].SetSiblingIndex(i);

        LayoutRebuilder.ForceRebuildLayoutImmediate(pieceGrid as RectTransform);
    }

    public void PiecePlacedCorrect()
    {
        correctPieces++;

        if (correctPieces >= totalPieces)
        {
            Debug.Log("done!");
        }
        else
        {
            ShuffleRemainingPieces();
        }
    }

    void ShuffleRemainingPieces()
    {
        List<PuzzlePiece> remaining = new List<PuzzlePiece>();

        foreach (var p in allPieces)
        {
            if (!p.IsPlaced)
                remaining.Add(p);
        }

        for (int i = remaining.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);

            Transform a = remaining[i].transform;
            Transform b = remaining[rand].transform;

            int indexA = a.GetSiblingIndex();
            int indexB = b.GetSiblingIndex();

            a.SetSiblingIndex(indexB);
            b.SetSiblingIndex(indexA);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(pieceGrid as RectTransform);
    }
}