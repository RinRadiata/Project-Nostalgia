using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PuzzleTitleUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text characterNameText;
    public TMP_Text instructionText;
    public Button startButton;
    public CanvasGroup canvasGroup;

    [Header("Controller")]
    public PuzzleController puzzleController;

    [Header("Intro Dialogue (Optional)")]
    [Tooltip("Lines of dialogue to display before starting. Format: 'SpeakerName \"dialogue\"'")]
    public List<string> introLines = new List<string>();

    [Tooltip("Or load from Resources file (leave empty if using introLines directly)")]
    public string introResourcePath = "";

    void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        LoadInfo();

        if (startButton != null)
            startButton.onClick.AddListener(OnStart);

        StartCoroutine(FadeIn());
    }

    void LoadInfo()
    {
        VariableStore.TryGetValue("Minigame.currentCharacter", out object charObj);
        VariableStore.TryGetValue("Minigame.currentID", out object idObj);

        string charID = charObj?.ToString() ?? "";
        string memID = idObj?.ToString() ?? "";

        if (characterNameText != null)
            characterNameText.text = charID;

        if (titleText != null)
            titleText.text = FormatTitle(memID);

        if (instructionText != null)
            instructionText.text = "Find the correct memories fragments\nTo restore our memories...";
    }

    string FormatTitle(string memID)
    {
        if (string.IsNullOrEmpty(memID)) return "Memories";
        string result = char.ToUpper(memID[0]) + memID.Substring(1);
        for (int i = 1; i < result.Length; i++)
        {
            if (char.IsDigit(result[i]) && !char.IsDigit(result[i - 1]))
                result = result.Insert(i, " ");
        }
        return result;
    }

    void OnStart()
    {
        // if intro lines exist, disable start button and wait for dialogue to finish before starting puzzle
        if (MinigameDialogueBridge.instance != null && (introLines.Count > 0 || !string.IsNullOrEmpty(introResourcePath)))
        {
            startButton.interactable = false;

            if (!string.IsNullOrEmpty(introResourcePath))
                MinigameDialogueBridge.instance.SayFromFile(introResourcePath, BeginPuzzle);
            else
                MinigameDialogueBridge.instance.Say(introLines, BeginPuzzle);
        }
        else
        {
            BeginPuzzle();
        }
    }

    void BeginPuzzle()
    {
        StartCoroutine(FadeOutAndStart());
    }

    IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        float t = 0;
        canvasGroup.alpha = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / 0.5f);
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    IEnumerator FadeOutAndStart()
    {
        if (canvasGroup != null)
        {
            float t = 0;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1, 0, t / 0.3f);
                yield return null;
            }
        }

        gameObject.SetActive(false);

        if (puzzleController != null)
            puzzleController.StartPuzzle();
    }
}