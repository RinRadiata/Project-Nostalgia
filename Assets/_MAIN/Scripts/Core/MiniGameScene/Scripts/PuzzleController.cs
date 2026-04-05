using UnityEngine;

public class PuzzleController : MonoBehaviour
{
    public int totalPieces = 4;
    private int correctPieces = 0;

    public void PiecePlacedCorrect()
    {
        correctPieces++;

        if (correctPieces >= totalPieces)
        {
            CompletePuzzle();
        }
    }

    void CompletePuzzle()
    {
        MinigameSceneManager.instance.FinishMinigame(true);
    }
}