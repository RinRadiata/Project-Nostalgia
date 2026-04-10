using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MinigameDialogueBridge : MonoBehaviour
{
    public static MinigameDialogueBridge instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Root panel of the dialogue box (will be shown/hidden)")]
    public GameObject dialogueRoot;

    [Tooltip("Text displaying the dialogue")]
    public TextMeshProUGUI dialogueText;

    [Tooltip("Root of the name box")]
    public GameObject nameRoot;

    [Tooltip("Text displaying the character's name")]
    public TextMeshProUGUI nameText;

    [Tooltip("Button to proceed to the next line (click to continue)")]
    public Button continueButton;

    [Tooltip("Icon blinking 'press to continue'")]
    public GameObject continuePrompt;

    [Header("Typewriter Settings")]
    public float typewriterSpeed = 40f; // characters per second
    [Header("Canvas Group (optional - used for fade)")]
    public CanvasGroup canvasGroup;

    // State
    private Queue<DialogueLine> lineQueue = new Queue<DialogueLine>();
    private Coroutine typewriterCo;
    private bool isTyping = false;
    private bool waitingForInput = false;
    private System.Action onComplete;

    private struct DialogueLine
    {
        public string speaker;   // "" = narrator
        public string dialogue;
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Hide dialogue box initially
        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        if (continuePrompt != null)
            continuePrompt.SetActive(false);
    }

    void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void Say(List<string> lines, System.Action onComplete = null)
    {
        this.onComplete = onComplete;
        lineQueue.Clear();

        foreach (string raw in lines)
        {
            var parsed = ParseLine(raw);
            lineQueue.Enqueue(parsed);
        }

        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        StartCoroutine(RunQueue());
    }

    /// <summary>
    /// Shortcut: Show a single line from the narrator.
    /// </summary>
    public void SayNarrator(string text, System.Action onComplete = null)
    {
        Say(new List<string> { $"\"{text}\"" }, onComplete);
    }

    /// <summary>
    /// Shortcut: Show a single line from a specific character.
    /// </summary>
    public void SayCharacter(string character, string text, System.Action onComplete = null)
    {
        Say(new List<string> { $"{character} \"{text}\"" }, onComplete);
    }

    /// <summary>
    /// Read lines from a text file (using Resources.Load).
    /// Format similar to MinigameTest.txt — only reads dialogue lines (with quotation marks).
    /// </summary>
    public void SayFromFile(string resourcePath, System.Action onComplete = null)
    {
        TextAsset file = Resources.Load<TextAsset>(resourcePath);
        if (file == null)
        {
            Debug.LogError($"[MinigameDialogueBridge] File not found: {resourcePath}");
            onComplete?.Invoke();
            return;
        }

        var lines = new List<string>();
        foreach (string line in file.text.Split('\n'))
        {
            string trimmed = line.Trim();
            // Only take lines with dialogue (with quotation marks " ") — ignore commands, if, choice
            if (trimmed.Contains('"') && !trimmed.StartsWith("//"))
                lines.Add(trimmed);
        }

        Say(lines, onComplete);
    }

    public void Hide(bool immediate = false)
    {
        StopAllCoroutines();
        isTyping = false;
        waitingForInput = false;

        if (immediate || canvasGroup == null)
        {
            if (dialogueRoot != null) dialogueRoot.SetActive(false);
        }
        else
        {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator RunQueue()
    {
        // Wait 1 frame to avoid capturing Start click immediately when DialogueRoot is just activated
        yield return null;

        while (lineQueue.Count > 0)
        {
            DialogueLine line = lineQueue.Dequeue();

            bool hasName = !string.IsNullOrEmpty(line.speaker);
            if (nameRoot != null) nameRoot.SetActive(hasName);
            if (nameText != null) nameText.text = hasName ? InjectText(line.speaker) : "";

            string fullText = InjectText(line.dialogue);
            yield return StartCoroutine(Typewrite(fullText));

            if (continuePrompt != null) continuePrompt.SetActive(true);

            // Wait 1 frame before accepting input to avoid click-through
            yield return null;
            waitingForInput = true;
            while (waitingForInput)
                yield return null;

            if (continuePrompt != null) continuePrompt.SetActive(false);
        }

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        onComplete?.Invoke();
    }

    private IEnumerator Typewrite(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        float interval = 1f / typewriterSpeed;
        for (int i = 0; i <= text.Length; i++)
        {
            dialogueText.text = text.Substring(0, i);
            yield return new WaitForSeconds(interval);
        }

        isTyping = false;
    }

    private void OnContinueClicked()
    {
        if (isTyping)
        {
            // Pressed once while typing = skip typewriter
            if (typewriterCo != null)
            {
                StopCoroutine(typewriterCo);
                typewriterCo = null;
            }
            dialogueText.text = GetCurrentFullText();
            isTyping = false;
        }
        else if (waitingForInput)
        {
            waitingForInput = false;
        }
    }

    // Input from keyboard / screen tap
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (dialogueRoot != null && dialogueRoot.activeSelf)
                OnContinueClicked();
        }
    }

    private string currentFullText = "";
    private string GetCurrentFullText() => currentFullText;

    private IEnumerator TypewriteInternal(string text)
    {
        currentFullText = text;
        isTyping = true;
        dialogueText.text = "";

        float interval = 1f / typewriterSpeed;

        // Count through each character but need to skip TMP rich text tags
        int visibleCount = 0;
        int totalVisible = GetVisibleLength(text);

        string displayText = text;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.text = displayText;
        dialogueText.ForceMeshUpdate();

        while (visibleCount <= totalVisible)
        {
            dialogueText.maxVisibleCharacters = visibleCount;
            visibleCount++;
            yield return new WaitForSeconds(interval);
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
    }

    private int GetVisibleLength(string text)
    {
        // Count actual characters (ignore TMP tags <...>)
        int count = 0;
        bool inTag = false;
        foreach (char c in text)
        {
            if (c == '<') inTag = true;
            else if (c == '>') inTag = false;
            else if (!inTag) count++;
        }
        return count;
    }

    private DialogueLine ParseLine(string raw)
    {
        // Find the first quotation mark
        int start = raw.IndexOf('"');
        if (start == -1) return new DialogueLine { speaker = "", dialogue = raw };

        int end = raw.LastIndexOf('"');
        string speaker = raw.Substring(0, start).Trim();
        string dialogue = (start < end) ? raw.Substring(start + 1, end - start - 1) : raw.Substring(start + 1);

        return new DialogueLine { speaker = speaker, dialogue = dialogue };
    }

    /// <summary>
    /// Inject VariableStore variables and Tags into text.
    /// Use TagManager.Inject if available — fallback manually if not.
    /// </summary>
    private string InjectText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Use TagManager (already in the project)
        try { text = TagManager.Inject(text); }
        catch { /* TagManager not available in this scene */ }

        return text;
    }

    private IEnumerator FadeOut()
    {
        float t = 0.25f;
        float elapsed = 0;
        float start = canvasGroup.alpha;

        while (elapsed < t)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, 0, elapsed / t);
            yield return null;
        }

        dialogueRoot.SetActive(false);
        canvasGroup.alpha = 1;
    }
}