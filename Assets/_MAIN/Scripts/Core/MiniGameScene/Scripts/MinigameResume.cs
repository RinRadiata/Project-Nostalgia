using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;

/// <summary>
/// Gắn vào một GameObject trong VN scene.
/// Tự detect khi quay về từ minigame và resume conversation
/// từ đúng dòng sau lệnh playminigame.
/// </summary>
public class MinigameResume : MonoBehaviour
{
    IEnumerator Start()
    {
        // Check nếu có resume request từ minigame
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

        // Xóa resume request để không trigger lại lần sau
        VariableStore.TrySetValue("Minigame.resumeFile", "");
        VariableStore.TrySetValue("Minigame.resumeProgress", -1);

        // Đợi DialogueSystem và các system khác init xong
        yield return null;
        yield return null;

        // Load file dùng FileManager — giống cách VN framework load dialogue
        // file có dạng "Dialogue Files/Test_Main" khớp với resources_dialogueFiles
        List<string> lines = FileManager.ReadTextAsset(file, includeBlankLines: true);
        if (lines == null || lines.Count == 0)
        {
            Debug.LogError($"[MinigameResume] Không đọc được file: '{file}'");
            yield break;
        }

        // Resume từ dòng TIẾP THEO sau playminigame
        int resumeFrom = progress + 1;
        if (resumeFrom >= lines.Count)
        {
            Debug.Log("[MinigameResume] Conversation đã kết thúc sau minigame.");
            yield break;
        }

        Debug.Log($"[MinigameResume] Resuming '{file}' từ dòng {resumeFrom}: '{lines[resumeFrom]}'");

        Conversation conversation = new Conversation(lines, progress: resumeFrom, file: file);

        if (DialogueSystem.instance != null)
        {
            DialogueSystem.instance.Show(immediate: true);
            DialogueSystem.instance.Say(conversation);
        }
    }
}