using UnityEngine;
using System;

public static class AffectionSystem
{
    private const int UNLOCK_THRESHOLD = 10;

    public static void AddAffection(string characterID, int amount)
    {
        EnsureVariables(characterID);

        int current = GetAffection(characterID);
        current += amount;

        VariableStore.TrySetValue(characterID + ".affection", current);

        if (current >= UNLOCK_THRESHOLD)
            VariableStore.TrySetValue(characterID + ".diaryUnlocked", true);
    }

    public static int GetAffection(string characterID)
    {
        EnsureVariables(characterID);

        if (VariableStore.TryGetValue(characterID + ".affection", out object value))
            return Convert.ToInt32(value);

        return 0;
    }

    public static bool IsDiaryUnlocked(string characterID)
    {
        EnsureVariables(characterID);

        if (VariableStore.TryGetValue(characterID + ".diaryUnlocked", out object value))
            return Convert.ToBoolean(value);

        return false;
    }

    private static void EnsureVariables(string id)
    {
        VariableStore.CreateDatabase(id);

        if (!VariableStore.HasVariable(id + ".affection"))
            VariableStore.CreateVariable<int>(id + ".affection", 0);

        if (!VariableStore.HasVariable(id + ".diaryUnlocked"))
            VariableStore.CreateVariable<bool>(id + ".diaryUnlocked", false);
    }
}