using UnityEngine;
using DIALOGUE;

public class MinigameSceneGuard : MonoBehaviour
{
    private PlayerInputManager[] vnInputManagers;

    void Awake()
    {
        DisableVNDialogueSystem();
    }

    void OnEnable()
    {
        DisableVNDialogueSystem();
    }

    void OnDestroy()
    {
        RestoreVNInput();
    }

    void DisableVNDialogueSystem()
    {
        if (DialogueSystem.instance != null)
        {
            DialogueSystem.instance.conversationManager.StopConversation();
            DialogueSystem.instance.conversationManager.allowUserPrompts = false;

            if (DialogueSystem.instance.prompt != null)
                DialogueSystem.instance.prompt.Hide();

            if (DialogueSystem.instance.dialogueContainer != null)
            {
                DialogueSystem.instance.dialogueContainer.Hide(immediate: true);

                if (DialogueSystem.instance.dialogueContainer.root != null)
                    DialogueSystem.instance.dialogueContainer.root.SetActive(false);
            }

            DialogueSystem.instance.Hide(immediate: true);
            DialogueSystem.instance.OnSystemPrompt_Clear();
        }

        vnInputManagers = Object.FindObjectsByType<PlayerInputManager>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var input in vnInputManagers)
        {
            if (input != null)
                input.enabled = false;
        }
    }

    void RestoreVNInput()
    {
        if (DialogueSystem.instance != null)
            DialogueSystem.instance.conversationManager.allowUserPrompts = true;

        if (vnInputManagers == null)
            return;

        foreach (var input in vnInputManagers)
        {
            if (input != null)
                input.enabled = true;
        }
    }
}