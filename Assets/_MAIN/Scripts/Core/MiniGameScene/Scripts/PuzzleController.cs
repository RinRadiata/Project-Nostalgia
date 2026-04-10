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

    [Header("UI Panels")]
    public GameObject titlePanel;
    public GameObject completionPanel;
    public PuzzleCompletionUI completionUI;
    public GameObject puzzleArea;

    [Header("Timer (Optional)")]
    public bool useTimer = false;
    public float timeLimit = 120f; // in seconds
    private float elapsedTime = 0f;
    private bool timerRunning = false;

    private int correctPieces = 0;

    // perfect = done in under half the time limit, or if no timer is used = always perfect
    private bool isPerfect => !useTimer || elapsedTime <= timeLimit / 2f;

    void Start()
    {
        // show the title panel first and hide the completion panel and puzzle area at the start
        if (titlePanel != null) titlePanel.SetActive(true);
        if (completionPanel != null) completionPanel.SetActive(false);
        if (puzzleArea != null) puzzleArea.SetActive(false);  // hide the puzzle area until StartPuzzle() is called

        SetPiecesInteractable(false);

        StartCoroutine(DelayedShuffle());
    }

    void Update()
    {
        if (timerRunning && useTimer)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= timeLimit)
            {
                timerRunning = false;
                // Hết giờ = fail
                MinigameSceneManager.instance.FailMinigame();
            }
        }
    }

    // call from PuzzleTitleUI when start button clicked
    public void StartPuzzle()
    {
        if (titlePanel != null) titlePanel.SetActive(false);
        if (puzzleArea != null) puzzleArea.SetActive(true);
        SetPiecesInteractable(true);
        timerRunning = useTimer;
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
            timerRunning = false;
            StartCoroutine(ShowCompletion());
        }
        else
        {
            ShuffleRemainingPieces();
        }
    }

    IEnumerator ShowCompletion()
    {
        yield return new WaitForSeconds(0.8f);

        // save data BEFORE setting active completion panel, in case Show() needs to read from VariableStore to determine what to show
        MinigameSceneManager.instance.FinishMinigame(isPerfect);

        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
            if (completionUI != null)
                completionUI.Show(isPerfect, elapsedTime);
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

    void SetPiecesInteractable(bool state)
    {
        foreach (var piece in allPieces)
        {
            var cg = piece.GetComponent<CanvasGroup>();
            if (cg != null) cg.blocksRaycasts = state;
        }
    }
}