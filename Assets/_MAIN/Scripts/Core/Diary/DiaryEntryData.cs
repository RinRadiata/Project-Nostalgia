using UnityEngine;

[System.Serializable]
public class DiaryEntryData
{
    public string title;

    [TextArea(3, 8)]
    public string content;

    [Header("Affection Unlock")]
    public int requiredAffection = 0;

    [Header("Minigame Unlock")]
    public bool unlockByMinigame = false;

    [Tooltip("MUST match minigameID on 'playminigame' command. Ex: memory1")]
    public string minigameID = "";

    public bool IsUnlocked(string characterID, int currentAffection)
    {
        if (unlockByMinigame)
        {
            if (string.IsNullOrWhiteSpace(minigameID))
            {
                Debug.LogWarning($"[DiaryEntryData] Entry '{title}' is set to unlock by minigame but minigameID is empty.");
                return false;
            }

            return DiaryProgress.IsDiaryEntryUnlocked(characterID, minigameID);
        }

        return currentAffection >= requiredAffection;
    }

    public string GetUnlockDate(string characterID)
    {
        if (!unlockByMinigame || string.IsNullOrWhiteSpace(minigameID))
            return "";

        return DiaryProgress.GetDiaryUnlockDate(characterID, minigameID);
    }

    public string GetLockMessage(int currentAffection)
    {
        if (unlockByMinigame)
        {
            if (string.IsNullOrWhiteSpace(minigameID))
                return "Complete the required memory.";

            return $"Complete memory: {minigameID}";
        }

        return "Unlock at " + requiredAffection + "\n(Current: " + currentAffection + ")";
    }
}