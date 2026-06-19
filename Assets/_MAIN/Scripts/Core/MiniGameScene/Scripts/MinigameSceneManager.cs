using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameSceneManager : MonoBehaviour
{
    public static MinigameSceneManager instance;

    void Awake()
    {
        instance = this;
    }

    public void FinishMinigame(bool perfect)
    {
        VariableStore.TryGetValue("Minigame.currentCharacter", out object charObj);
        VariableStore.TryGetValue("Minigame.currentID", out object idObj);

        string characterID = charObj?.ToString() ?? "";
        string minigameID = idObj?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(characterID) || string.IsNullOrWhiteSpace(minigameID))
        {
            Debug.LogError("[MinigameSceneManager] Missing characterID or minigameID. Check playminigame command.");
            return;
        }

        DiaryProgress.MarkMinigameCompleted(characterID, minigameID, perfect);

        AffectionSystem.AddAffection(characterID, perfect ? 10 : 5);

        Debug.Log(
            $"[MinigameSceneManager] Finished minigame '{minigameID}' for '{characterID}'. " +
            $"Perfect: {perfect}. Diary key: {DiaryProgress.DiaryUnlockedKey(characterID, minigameID)}"
        );
    }

    public void FailMinigame()
    {
        VariableStore.TryGetValue("Minigame.returnScene", out object returnSceneObj);
        string returnScene = returnSceneObj?.ToString() ?? "";

        if (!string.IsNullOrWhiteSpace(returnScene))
            SceneManager.LoadScene(returnScene);
        else
            Debug.LogError("[MinigameSceneManager] Return scene not set!");
    }
}