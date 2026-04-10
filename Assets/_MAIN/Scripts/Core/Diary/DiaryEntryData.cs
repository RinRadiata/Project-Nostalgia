//using UnityEngine;

//[System.Serializable]
//public class DiaryEntryData
//{
//    public string title;

//    [TextArea(3, 10)]
//    public string content;

//    [Header("Unlock Condition")]
//    public int requiredAffection;
//}
using UnityEngine;

[System.Serializable]
public class DiaryEntryData
{
    public string title;

    [TextArea(3, 10)]
    public string content;

    [Header("Unlock Condition")]
    public int requiredAffection;

    [Header("Minigame Unlock")]
    [Tooltip("if filled, this entry will only unlock when the corresponding minigame is completed. " +
             "Leave empty if only affection is required.")]
    public string requiredMinigameID;  // e.g., "memory1" — matches Minigame.currentID

    /// <summary>
    /// Checks if this entry is unlocked based on both affection and minigame completion.
    /// </summary>
    public bool IsUnlocked(string characterID, int currentAffection)
    {
        // Affection condition
        bool affectionMet = currentAffection >= requiredAffection;

        // Minigame condition (if has any)
        bool minigameMet = true;
        if (!string.IsNullOrEmpty(requiredMinigameID))
        {
            string key = $"{characterID}.diary.{requiredMinigameID}.unlocked";
            if (VariableStore.TryGetValue(key, out object val))
                minigameMet = val is bool b && b;
            else
                minigameMet = false;
        }

        return affectionMet && minigameMet;
    }

    /// <summary>
    /// Gets the unlock date from VariableStore (if has any).
    /// </summary>
    public string GetUnlockDate(string characterID)
    {
        if (string.IsNullOrEmpty(requiredMinigameID)) return "";

        string key = $"{characterID}.diary.{requiredMinigameID}.unlockDate";
        if (VariableStore.TryGetValue(key, out object val))
            return val?.ToString() ?? "";

        return "";
    }
}