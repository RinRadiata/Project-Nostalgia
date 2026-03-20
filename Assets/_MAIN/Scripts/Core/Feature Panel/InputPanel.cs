using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputPanel : MonoBehaviour
{
    public static InputPanel instance { get; private set; } = null;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private TMP_InputField inputField;

    private CanvasGroupController cg;

    public string lastInput { get; private set; } = string.Empty;
    public bool isWaitingOnUserInput { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        cg = new CanvasGroupController(this, canvasGroup);
        
        cg.alpha = 0f;
        cg.SetInteractableState(active: false);
        acceptButton.gameObject.SetActive(false);

        inputField.onValueChanged.AddListener(OnInputChanged);
        acceptButton.onClick.AddListener(OnAcceptInput);
    }

    public void Show(string title)
    {
        titleText.text = title;
        inputField.text = string.Empty;
        cg.Show();
        cg.SetInteractableState(active: true);
        isWaitingOnUserInput = true;
    }

    public void Hide()
    {
        cg.Hide();
        cg.SetInteractableState(active: false);
        isWaitingOnUserInput = false;
    }

    public void OnAcceptInput()
    {
        if (inputField.text == string.Empty)
            return;

        string input = inputField.text;
        if (CensorManager.Censor(ref input))
        {
            UIConfirmationMenu.instance.Show("Your input was not accepted due to a profanity filler! Please try again!",
                new UIConfirmationMenu.ConfirmationButton(title: "OK", () => inputField.text = ""));
        }
        else
        {
            lastInput = inputField.text;
            Hide();
        }
    }

    public void OnInputChanged(string value)
    {
        acceptButton.gameObject.SetActive(hasValidText());
    }

    private bool hasValidText()
    {
        return inputField.text != string.Empty;
    }
}
