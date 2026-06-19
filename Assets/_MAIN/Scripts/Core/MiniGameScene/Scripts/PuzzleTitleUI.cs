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

    [Header("Intro Dialogue")]
    [Tooltip("default off for Intro Dialogue to avoid accidentally reading the main dialogue file.")]
    public bool playIntroDialogue = false;

    [Tooltip("Lines format: Nerine \"dialogue\"")]
    public List<string> introLines = new List<string>();

    [Tooltip("Resources path only. Example: Dialogue Files/MinigameIntro_Lune")]
    public string introResourcePath = "";

    private bool started = false;

    void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        LoadInfo();

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStart);
            startButton.onClick.AddListener(OnStart);
            startButton.interactable = true;
        }

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
        if (string.IsNullOrEmpty(memID))
            return "Memories";

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
        if (started)
            return;

        started = true;

        if (startButton != null)
            startButton.interactable = false;

        bool hasIntroLines = introLines != null && introLines.Count > 0;
        bool hasIntroFile = !string.IsNullOrWhiteSpace(introResourcePath);

        if (playIntroDialogue &&
            MinigameDialogueBridge.instance != null &&
            (hasIntroLines || hasIntroFile))
        {
            if (hasIntroFile)
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
        if (canvasGroup == null)
            yield break;

        float t = 0f;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        while (t < 0.5f)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.5f);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOutAndStart()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float t = 0f;

            while (t < 0.3f)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.3f);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        gameObject.SetActive(false);

        if (puzzleController != null)
            puzzleController.StartPuzzle();
        else
            Debug.LogError("[PuzzleTitleUI] PuzzleController is not assigned.");
    }
}