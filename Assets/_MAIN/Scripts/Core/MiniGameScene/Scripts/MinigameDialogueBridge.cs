using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MinigameDialogueBridge : MonoBehaviour
{
    public static MinigameDialogueBridge instance { get; private set; }

    [Header("UI References")]
    public GameObject dialogueRoot;
    public TextMeshProUGUI dialogueText;

    public GameObject nameRoot;
    public TextMeshProUGUI nameText;

    [Tooltip("Optional. Nếu không gán, script sẽ tự tạo click button trên DialogueRoot.")]
    public Button continueButton;

    public GameObject continuePrompt;

    [Header("Typewriter")]
    public float typewriterSpeed = 40f;

    [Header("Canvas Group")]
    public CanvasGroup canvasGroup;

    [Header("Click Fix")]
    public bool autoCreateDialogueClickArea = true;

    private readonly Queue<DialogueLine> lineQueue = new Queue<DialogueLine>();

    private Coroutine runQueueCo;
    private Coroutine typewriterCo;

    private bool isTyping;
    private bool waitingForInput;

    private string currentFullText = "";
    private System.Action onComplete;

    private Button rootClickButton;

    private struct DialogueLine
    {
        public string speaker;
        public string dialogue;
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (canvasGroup == null && dialogueRoot != null)
            canvasGroup = dialogueRoot.GetComponent<CanvasGroup>();

        SetupRootClickButton();
        HideImmediate();
    }

    void OnEnable()
    {
        RegisterButtons();
    }

    void OnDisable()
    {
        UnregisterButtons();
    }

    void Update()
    {
        if (dialogueRoot == null || !dialogueRoot.activeSelf)
            return;

        if (WasContinuePressed())
            OnContinueClicked();
    }

    public void Say(List<string> lines, System.Action onComplete = null)
    {
        if (lines == null || lines.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        StopCurrentDialogue();

        this.onComplete = onComplete;
        lineQueue.Clear();

        foreach (string raw in lines)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            lineQueue.Enqueue(ParseLine(raw.Trim()));
        }

        if (lineQueue.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        ShowRoot();
        runQueueCo = StartCoroutine(RunQueue());
    }

    public void SayNarrator(string text, System.Action onComplete = null)
    {
        Say(new List<string> { $"\"{text}\"" }, onComplete);
    }

    public void SayCharacter(string character, string text, System.Action onComplete = null)
    {
        Say(new List<string> { $"{character} \"{text}\"" }, onComplete);
    }

    public void SayFromFile(string resourcePath, System.Action onComplete = null)
    {
        string cleanPath = NormalizeResourcePath(resourcePath);

        TextAsset file = Resources.Load<TextAsset>(cleanPath);

        if (file == null)
        {
            Debug.LogError($"[MinigameDialogueBridge] File not found in Resources: {cleanPath}");
            onComplete?.Invoke();
            return;
        }

        List<string> lines = new List<string>();

        foreach (string rawLine in file.text.Split('\n'))
        {
            string line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("//"))
                continue;

            if (line.Contains("\""))
                lines.Add(line);
        }

        Say(lines, onComplete);
    }

    public void Hide(bool immediate = false)
    {
        StopCurrentDialogue();
        HideImmediate();
    }

    private IEnumerator RunQueue()
    {
        yield return null;

        while (lineQueue.Count > 0)
        {
            DialogueLine line = lineQueue.Dequeue();

            bool hasName = !string.IsNullOrWhiteSpace(line.speaker);

            if (nameRoot != null)
                nameRoot.SetActive(hasName);

            if (nameText != null)
                nameText.text = hasName ? InjectText(line.speaker) : "";

            currentFullText = InjectText(line.dialogue);

            if (continuePrompt != null)
                continuePrompt.SetActive(false);

            typewriterCo = StartCoroutine(Typewrite(currentFullText));
            yield return typewriterCo;
            typewriterCo = null;

            isTyping = false;

            if (dialogueText != null)
                dialogueText.maxVisibleCharacters = int.MaxValue;

            if (continuePrompt != null)
                continuePrompt.SetActive(true);

            yield return null;

            waitingForInput = true;

            while (waitingForInput)
                yield return null;

            if (continuePrompt != null)
                continuePrompt.SetActive(false);
        }

        HideImmediate();

        System.Action completeCallback = onComplete;
        onComplete = null;
        runQueueCo = null;

        completeCallback?.Invoke();
    }

    private IEnumerator Typewrite(string text)
    {
        isTyping = true;

        if (dialogueText == null)
        {
            isTyping = false;
            yield break;
        }

        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalVisible = dialogueText.textInfo.characterCount;
        int visibleCount = 0;

        float interval = typewriterSpeed <= 0 ? 0f : 1f / typewriterSpeed;

        while (visibleCount <= totalVisible)
        {
            dialogueText.maxVisibleCharacters = visibleCount;
            visibleCount++;

            if (interval > 0f)
                yield return new WaitForSeconds(interval);
            else
                yield return null;
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
    }

    private void OnContinueClicked()
    {
        if (dialogueRoot == null || !dialogueRoot.activeSelf)
            return;

        if (isTyping)
        {
            if (typewriterCo != null)
            {
                StopCoroutine(typewriterCo);
                typewriterCo = null;
            }

            if (dialogueText != null)
            {
                dialogueText.text = currentFullText;
                dialogueText.maxVisibleCharacters = int.MaxValue;
            }

            isTyping = false;
            return;
        }

        if (waitingForInput)
            waitingForInput = false;
    }

    public void OnPointerContinue()
    {
        OnContinueClicked();
    }

    private void SetupRootClickButton()
    {
        if (!autoCreateDialogueClickArea || dialogueRoot == null)
            return;

        Image image = dialogueRoot.GetComponent<Image>();

        if (image == null)
            image = dialogueRoot.AddComponent<Image>();

        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;

        MinigameDialogueClickCatcher catcher =
            dialogueRoot.GetComponent<MinigameDialogueClickCatcher>();

        if (catcher == null)
            catcher = dialogueRoot.AddComponent<MinigameDialogueClickCatcher>();

        catcher.bridge = this;
    }

    private void RegisterButtons()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (rootClickButton != null && rootClickButton != continueButton)
        {
            rootClickButton.onClick.RemoveListener(OnContinueClicked);
            rootClickButton.onClick.AddListener(OnContinueClicked);
        }
    }

    private void UnregisterButtons()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);

        if (rootClickButton != null)
            rootClickButton.onClick.RemoveListener(OnContinueClicked);
    }

    private void ShowRoot()
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (continuePrompt != null)
            continuePrompt.SetActive(false);
    }

    private void HideImmediate()
    {
        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        if (nameRoot != null)
            nameRoot.SetActive(false);

        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        if (nameText != null)
            nameText.text = "";

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        isTyping = false;
        waitingForInput = false;
        currentFullText = "";
    }

    private void StopCurrentDialogue()
    {
        if (typewriterCo != null)
        {
            StopCoroutine(typewriterCo);
            typewriterCo = null;
        }

        if (runQueueCo != null)
        {
            StopCoroutine(runQueueCo);
            runQueueCo = null;
        }

        lineQueue.Clear();

        isTyping = false;
        waitingForInput = false;
    }

    private bool WasContinuePressed()
    {
#if ENABLE_INPUT_SYSTEM
        bool spacePressed =
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame;

        bool enterPressed =
            Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame ||
             Keyboard.current.numpadEnterKey.wasPressedThisFrame);

        if (spacePressed || enterPressed)
            return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return))
            return true;
#endif

        return false;
    }

    private DialogueLine ParseLine(string raw)
    {
        int start = raw.IndexOf('"');

        if (start < 0)
        {
            return new DialogueLine
            {
                speaker = "",
                dialogue = raw
            };
        }

        int end = raw.LastIndexOf('"');

        string speaker = raw.Substring(0, start).Trim();

        string dialogue = start < end
            ? raw.Substring(start + 1, end - start - 1)
            : raw.Substring(start + 1);

        if (speaker.Equals("narrator", System.StringComparison.OrdinalIgnoreCase))
            speaker = "";

        return new DialogueLine
        {
            speaker = speaker,
            dialogue = dialogue
        };
    }

    private string InjectText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        try
        {
            text = TagManager.Inject(text);
        }
        catch
        {
        }

        return text;
    }

    private string NormalizeResourcePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "";

        path = path.Trim();
        path = path.Replace("\\", "/");

        const string resourcesMarker = "/Resources/";
        int index = path.IndexOf(resourcesMarker, System.StringComparison.OrdinalIgnoreCase);

        if (index >= 0)
            path = path.Substring(index + resourcesMarker.Length);

        if (path.StartsWith("Resources/", System.StringComparison.OrdinalIgnoreCase))
            path = path.Substring("Resources/".Length);

        if (path.EndsWith(".txt", System.StringComparison.OrdinalIgnoreCase))
            path = path.Substring(0, path.Length - ".txt".Length);

        return path;
    }
}