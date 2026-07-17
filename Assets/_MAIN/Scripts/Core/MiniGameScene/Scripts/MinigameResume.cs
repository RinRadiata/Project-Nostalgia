using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;

public class MinigameResume : MonoBehaviour
{
    IEnumerator Start()
    {
        // Check if there is a resume request from minigame
        if (!VariableStore.TryGetValue("Minigame.resumeFile", out object fileObj))
            yield break;
        if (!VariableStore.TryGetValue("Minigame.resumeProgress", out object progressObj))
            yield break;

        string file = fileObj?.ToString() ?? "";
        if (string.IsNullOrEmpty(file))
            yield break;

        int progress = 0;
        try { progress = System.Convert.ToInt32(progressObj); }
        catch { yield break; }

        // Clear resume request to avoid triggering it again next time
        VariableStore.TrySetValue("Minigame.resumeFile", "");
        VariableStore.TrySetValue("Minigame.resumeProgress", -1);

        // Wait for DialogueSystem and other systems to finish initialization
        yield return null;
        yield return null;

        // Load file using FileManager - same way VN framework loads dialogue
        // file format "Dialogue Files/Test_Main" matches resources_dialogueFiles
        List<string> lines = FileManager.ReadTextAsset(file, includeBlankLines: true);
        if (lines == null || lines.Count == 0)
        {
            Debug.LogError($"[MinigameResume] Could not read file: '{file}'");
            yield break;
        }

        // Resume from the NEXT line after playminigame
        int resumeFrom = progress + 1;
        if (resumeFrom >= lines.Count)
        {
            Debug.Log("[MinigameResume] Conversation ended after minigame.");
            yield break;
        }

        Conversation conversation = new Conversation(lines, progress: resumeFrom, file: file);

        if (DialogueSystem.instance != null)
        {
            DialogueSystem.instance.Show(immediate: true);
            DialogueSystem.instance.Say(conversation);
        }
    }
}