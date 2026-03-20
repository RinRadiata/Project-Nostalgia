using UnityEngine;

[System.Serializable]
public class DiaryEntryData
{
    public string title;

    [TextArea(3, 10)]
    public string content;

    [Header("Unlock Condition")]
    public int requiredAffection;
}