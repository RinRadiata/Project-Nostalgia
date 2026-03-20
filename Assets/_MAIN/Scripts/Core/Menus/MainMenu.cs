using DIALOGUE;
using History;
using System.Collections;
using UnityEngine;
using VISUALNOVEL;

public class MainMenu : MonoBehaviour
{
    public const string MAIN_MENU_SCENE = "Main Menu";

    public static MainMenu instance { get; private set; }

    public AudioClip menuMusic;
    public CanvasGroup mainPanel;
    private CanvasGroupController mainCG;

    private UIConfirmationMenu uiChoiceMenu => UIConfirmationMenu.instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCG = new CanvasGroupController(this, mainPanel);

        AudioManager.instance.StopAllSoundEffects();
        AudioManager.instance.StopAllTracks();

        AudioManager.instance.PlayTrack(menuMusic, channel: 0, startingVolume: 1);
    }

    public void Click_StartNewGame()
    {
        uiChoiceMenu.Show("Start a new game?", new UIConfirmationMenu.ConfirmationButton("Yes", StartNewGame), new UIConfirmationMenu.ConfirmationButton("NO", null));
    }

    public void LoadGame(VNGameSave file)
    {
        ResetRuntime();
        VNGameSave.activeFile = file;
        StartCoroutine(StartingGame());
    }

    private void StartNewGame()
    {
        ResetRuntime();
        VNGameSave.activeFile = new VNGameSave();
        StartCoroutine(StartingGame());
    }

    private IEnumerator StartingGame()
    {
        mainCG.Hide(speed: 0.3f);
        AudioManager.instance.StopTrack(0);

        while (mainCG.isVisible)
            yield return null;

        VN_Configuration.activeConfig.Save();

        UnityEngine.SceneManagement.SceneManager.LoadScene("Nostalgia");
    }

    private void ResetRuntime()
    {
        // Reset all variables in the variable store
        VariableStore.RemoveAllVariables();

        // Reset history
        if (HistoryManager.instance != null)
        {
            HistoryManager.instance.history.Clear();
            HistoryManager.instance.logManager.Clear();
        }

        //// Reset conversations
        //if (DialogueSystem.instance != null)
        //{
        //    DialogueSystem.instance.conversationManager.Clear();
        //}
    }
}
