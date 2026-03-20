using UnityEngine;

[System.Serializable]
public class CharacterProfile
{
    public string characterID;
    public string characterName;
    public int age;

    [TextArea]
    public string description;

    public Sprite portrait;
    public Sprite lockedIcon;
    public Sprite unlockedIcon;

    public DiaryEntryData[] diaryEntries;

    public int maxAffection = 100;
}