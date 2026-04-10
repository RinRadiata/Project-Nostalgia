using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameSceneManager : MonoBehaviour
{
    public static MinigameSceneManager instance;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Gọi khi xếp hình hoàn thành. Lưu tất cả dữ liệu vào VariableStore.
    /// KHÔNG tự động chuyển scene — PuzzleCompletionUI sẽ xử lý việc đó khi người chơi nhấn Continue.
    /// </summary>
    public void FinishMinigame(bool perfect)
    {
        VariableStore.TryGetValue("Minigame.currentCharacter", out object charObj);
        VariableStore.TryGetValue("Minigame.currentID", out object idObj);

        string characterID = charObj?.ToString() ?? "";
        string minigameID = idObj?.ToString() ?? "";

        if (string.IsNullOrEmpty(characterID) || string.IsNullOrEmpty(minigameID))
        {
            Debug.LogError("[MinigameSceneManager] Missing character or minigame ID!");
            return;
        }

        // --- Lưu trạng thái hoàn thành ---
        VariableStore.TrySetValue($"{characterID}.minigame.{minigameID}.completed", true);
        VariableStore.TrySetValue($"{characterID}.minigame.{minigameID}.perfect", perfect);

        // --- Cộng affection ---
        AffectionSystem.AddAffection(characterID, perfect ? 10 : 5);

        // --- Mở khóa diary entry tương ứng ---
        VariableStore.TrySetValue($"{characterID}.diary.{minigameID}.unlocked", true);

        // Lưu ngày giờ mở khóa để hiển thị trong DiaryEntryUI
        string dateKey = $"{characterID}.diary.{minigameID}.unlockDate";
        if (!VariableStore.HasVariable(dateKey))
        {
            string date = System.DateTime.Now.ToString("MMM dd, yyyy HH:mm");
            VariableStore.CreateVariable<string>(dateKey, date);
        }

        Debug.Log($"[MinigameSceneManager] Minigame '{minigameID}' finished. Perfect: {perfect}. " +
                  $"Affection +{(perfect ? 10 : 5)} for '{characterID}'.");
    }

    /// <summary>
    /// Gọi khi người chơi thua / bỏ qua minigame. Chuyển thẳng về scene VN.
    /// </summary>
    public void FailMinigame()
    {
        VariableStore.TryGetValue("Minigame.returnScene", out object returnSceneObj);
        string returnScene = returnSceneObj?.ToString() ?? "";

        if (!string.IsNullOrEmpty(returnScene))
            SceneManager.LoadScene(returnScene);
        else
            Debug.LogError("[MinigameSceneManager] Return scene not set!");
    }
}