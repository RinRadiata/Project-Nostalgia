using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gắn vào CompletionPanel.
/// PuzzleController.ShowCompletion() sẽ:
///   1. Gọi MinigameSceneManager.FinishMinigame() — lưu variables
///   2. completionPanel.SetActive(true)
///   3. completionUI.Show() — đọc variables vừa lưu → hiển thị đúng
/// </summary>
public class PuzzleCompletionUI : MonoBehaviour
{
    [Header("Result Text")]
    public TMP_Text resultTitleText;
    public TMP_Text resultSubtitleText;

    [Header("Stars")]
    public GameObject[] stars;

    [Header("Memory Preview")]
    public Image memoryPreviewImage;
    public TMP_Text memoryTitleText;

    [Header("Time Display")]
    public TMP_Text timeText;
    public GameObject timeDisplay;

    [Header("Diary Unlock Notification")]
    public GameObject diaryUnlockNotif;
    public TMP_Text diaryUnlockText;

    [Header("Buttons")]
    public Button continueButton;
    public Button replayButton;

    [Header("Canvas Group (for panel fade)")]
    public CanvasGroup canvasGroup;

    [Header("Outro Dialogue (Optional)")]
    public List<string> outroLinesPerfect = new List<string>();
    public List<string> outroLinesNormal = new List<string>();
    public string outroResourcePathPerfect = "";
    public string outroResourcePathNormal = "";

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Hide buttons initially — will show after dialogue is done
        SetButtonsInteractable(false);
    }

    /// <summary>
    /// Call AFTER MinigameSceneManager.FinishMinigame() has run
    /// to read the correct values from VariableStore.
    /// </summary>
    public void Show(bool perfect, float timeInSeconds = 0f)
    {
        // Result text
        if (resultTitleText != null)
            resultTitleText.text = perfect ? "Perfect Memorize!" : "Completed!";

        if (resultSubtitleText != null)
            resultSubtitleText.text = perfect
                ? "You have perfectly restored the memory..."
                : "The memory has been restored, though it remains faint...";

        // Stars
        UpdateStars(perfect);

        // Memory info from VariableStore
        VariableStore.TryGetValue("Minigame.currentID", out object idObj);
        string memID = idObj?.ToString() ?? "";

        if (memoryTitleText != null)
            memoryTitleText.text = FormatTitle(memID);

        LoadPreviewImage(memID);

        // Timer
        if (timeDisplay != null)
            timeDisplay.SetActive(timeInSeconds > 0);

        if (timeText != null && timeInSeconds > 0)
        {
            int min = Mathf.FloorToInt(timeInSeconds / 60f);
            int sec = Mathf.FloorToInt(timeInSeconds % 60f);
            timeText.text = $"Time remaining: {min:00}:{sec:00}";
        }

        // Diary notification (FinishMinigame has set variables before)
        ShowDiaryNotification(memID);

        // Button listeners
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);
        if (replayButton != null)
            replayButton.onClick.AddListener(OnReplay);

        StartCoroutine(ShowSequence(perfect));
    }

    IEnumerator ShowSequence(bool perfect)
    {
        // Fade panel in
        yield return StartCoroutine(FadeIn());

        // Play outro dialogue if available
        List<string> outroLines = perfect ? outroLinesPerfect : outroLinesNormal;
        string outroPath = perfect ? outroResourcePathPerfect : outroResourcePathNormal;

        if (MinigameDialogueBridge.instance != null)
        {
            bool done = false;

            if (!string.IsNullOrEmpty(outroPath))
                MinigameDialogueBridge.instance.SayFromFile(outroPath, () => done = true);
            else if (outroLines != null && outroLines.Count > 0)
                MinigameDialogueBridge.instance.Say(outroLines, () => done = true);
            else
                done = true;

            while (!done) yield return null;
        }

        // Show buttons after dialogue is done
        SetButtonsInteractable(true);
    }

    void UpdateStars(bool perfect)
    {
        if (stars == null || stars.Length == 0) return;
        int starCount = perfect ? 3 : 2;
        for (int i = 0; i < stars.Length; i++)
            if (stars[i] != null) stars[i].SetActive(i < starCount);
    }

    void LoadPreviewImage(string memID)
    {
        if (memoryPreviewImage == null) return;

        VariableStore.TryGetValue("Minigame.currentCharacter", out object charObj);
        string charID = charObj?.ToString() ?? "";

        string folder = $"Minigame/{charID}/{memID}";

        // Find sprite with name starting with "preview_" in folder
        Sprite preview = null;
        Sprite[] all = Resources.LoadAll<Sprite>(folder);

        if (all != null && all.Length > 0)
        {
            foreach (Sprite s in all)
            {
                if (s.name.StartsWith("preview_", System.StringComparison.OrdinalIgnoreCase))
                {
                    preview = s;
                    break;
                }
            }
        }

        if (preview != null)
            memoryPreviewImage.sprite = preview;
        else
            Debug.LogWarning($"[PuzzleCompletionUI] Could not find sprite 'preview_*' at: {folder}");
    }

    void ShowDiaryNotification(string memID)
    {
        if (diaryUnlockNotif == null) return;

        VariableStore.TryGetValue("Minigame.currentCharacter", out object charObj);
        string charID = charObj?.ToString() ?? "";

        // Đọc flag mà FinishMinigame() đã set
        bool unlocked = false;
        if (VariableStore.TryGetValue($"{charID}.diary.{memID}.unlocked", out object val))
            unlocked = val is bool b && b;

        diaryUnlockNotif.SetActive(unlocked);

        if (unlocked && diaryUnlockText != null)
            diaryUnlockText.text = $"New diary entry unlocked:\n\"{FormatTitle(memID)}\"";
    }

    string FormatTitle(string memID)
    {
        if (string.IsNullOrEmpty(memID)) return "Memory";
        string result = char.ToUpper(memID[0]) + memID.Substring(1);
        for (int i = 1; i < result.Length; i++)
            if (char.IsDigit(result[i]) && !char.IsDigit(result[i - 1]))
                result = result.Insert(i, " ");
        return result;
    }

    void SetButtonsInteractable(bool state)
    {
        if (continueButton != null)
        {
            continueButton.interactable = state;
            // Show/hide using alpha instead of SetActive to avoid conflict with Animator if any
            var cg = continueButton.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = state ? 1 : 0; cg.blocksRaycasts = state; }
            else continueButton.gameObject.SetActive(state);
        }

        if (replayButton != null)
        {
            replayButton.interactable = state;
            var cg = replayButton.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = state ? 1 : 0; cg.blocksRaycasts = state; }
            else replayButton.gameObject.SetActive(state);
        }
    }

    IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        float duration = 0.4f, t = 0;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / duration);
            yield return null;
        }

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    void OnContinue()
    {
        VariableStore.TryGetValue("Minigame.returnScene", out object returnObj);
        string returnScene = returnObj?.ToString() ?? "";
        if (!string.IsNullOrEmpty(returnScene))
            UnityEngine.SceneManagement.SceneManager.LoadScene(returnScene);
        else
            Debug.LogError("[PuzzleCompletionUI] returnScene not set in VariableStore!");
    }

    void OnReplay()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}