using UnityEngine;

public static class DiaryProgress
{
    public static string MinigameCompletedKey(string characterID, string minigameID)
    {
        return $"{characterID}.minigame.{minigameID}.completed";
    }

    public static string MinigamePerfectKey(string characterID, string minigameID)
    {
        return $"{characterID}.minigame.{minigameID}.perfect";
    }

    public static string DiaryUnlockedKey(string characterID, string entryID)
    {
        return $"{characterID}.diary.{entryID}.unlocked";
    }

    public static string DiaryUnlockDateKey(string characterID, string entryID)
    {
        return $"{characterID}.diary.{entryID}.unlockDate";
    }

    public static void MarkMinigameCompleted(string characterID, string minigameID, bool perfect)
    {
        if (string.IsNullOrWhiteSpace(characterID) || string.IsNullOrWhiteSpace(minigameID))
        {
            Debug.LogError("[DiaryProgress] Missing characterID or minigameID.");
            return;
        }

        VariableStore.TrySetValue(MinigameCompletedKey(characterID, minigameID), true);
        VariableStore.TrySetValue(MinigamePerfectKey(characterID, minigameID), perfect);

        UnlockDiaryEntry(characterID, minigameID);
    }

    public static void UnlockDiaryEntry(string characterID, string entryID)
    {
        if (string.IsNullOrWhiteSpace(characterID) || string.IsNullOrWhiteSpace(entryID))
        {
            Debug.LogError("[DiaryProgress] Missing characterID or entryID.");
            return;
        }

        VariableStore.TrySetValue(DiaryUnlockedKey(characterID, entryID), true);

        string dateKey = DiaryUnlockDateKey(characterID, entryID);

        if (!VariableStore.HasVariable(dateKey))
        {
            string date = System.DateTime.Now.ToString("MMM dd, yyyy HH:mm");
            VariableStore.CreateVariable<string>(dateKey, date);
        }
    }

    public static bool IsDiaryEntryUnlocked(string characterID, string entryID)
    {
        if (string.IsNullOrWhiteSpace(characterID) || string.IsNullOrWhiteSpace(entryID))
            return false;

        if (VariableStore.TryGetValue(DiaryUnlockedKey(characterID, entryID), out object value))
        {
            if (value is bool b)
                return b;

            if (bool.TryParse(value?.ToString(), out bool parsed))
                return parsed;
        }

        return false;
    }

    public static string GetDiaryUnlockDate(string characterID, string entryID)
    {
        if (string.IsNullOrWhiteSpace(characterID) || string.IsNullOrWhiteSpace(entryID))
            return "";

        if (VariableStore.TryGetValue(DiaryUnlockDateKey(characterID, entryID), out object value))
            return value?.ToString() ?? "";

        return "";
    }
}