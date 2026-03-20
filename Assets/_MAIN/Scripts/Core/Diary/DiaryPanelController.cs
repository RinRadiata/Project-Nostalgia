using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiaryPanelController : MonoBehaviour
{
    [Header("Database")]
    public ProfileDatabase profileDatabase;

    [Header("Character Scroll")]
    public Transform characterContent;
    public GameObject characterIconPrefab;

    [Header("Main Info")]
    public Image portraitImage;
    public TMP_Text nameText;
    public TMP_Text ageText;
    public TMP_Text descriptionText;

    [Header("Affection")]
    public Image affectionFill;
    public TMP_Text affectionText;

    [Header("Diary Entries")]
    public Transform diaryContent;
    public GameObject diaryEntryPrefab;

    private CharacterProfile currentCharacter;

    void Start()
    {
        RefreshCharacters();
    }

    public void RefreshCharacters()
    {
        foreach (Transform child in characterContent)
            Destroy(child.gameObject);

        foreach (CharacterProfile character in profileDatabase.characters)
        {
            GameObject iconObj = Instantiate(characterIconPrefab, characterContent);
            iconObj.GetComponent<CharacterIconUI>().Setup(character, this);
        }
    }

    public void LoadCharacter(string id)
    {
        currentCharacter = profileDatabase.GetCharacter(id);
        if (currentCharacter == null)
            return;

        portraitImage.sprite = currentCharacter.portrait;
        nameText.text = currentCharacter.characterName;
        ageText.text = "Age: " + currentCharacter.age;
        descriptionText.text = currentCharacter.description;

        UpdateAffection();
        LoadEntries();
    }

    void UpdateAffection()
    {
        int affection = AffectionSystem.GetAffection(currentCharacter.characterID);

        float percent = Mathf.Clamp01((float)affection / currentCharacter.maxAffection);
        affectionFill.fillAmount = percent;

        affectionText.text = affection + " / " + currentCharacter.maxAffection;
    }

    void LoadEntries()
    {
        foreach (Transform child in diaryContent)
            Destroy(child.gameObject);

        int affection = AffectionSystem.GetAffection(currentCharacter.characterID);

        for (int i = 0; i < currentCharacter.diaryEntries.Length; i++)
        {
            DiaryEntryData entry = currentCharacter.diaryEntries[i];

            GameObject obj = Instantiate(diaryEntryPrefab, diaryContent);
            bool unlocked = affection >= entry.requiredAffection;

            // fetch date (if unlocked) or create date (if first time unlock) automatically
            string date = GetEntryDate(currentCharacter.characterID, i, unlocked);

            obj.GetComponent<DiaryEntryUI>()
                .Setup(entry, unlocked, affection, date);
        }
    }

    string GetEntryDate(string characterID, int index, bool unlocked)
    {
        string varName = characterID + ".entry_" + index + "_date";

        if (!unlocked)
            return "Locked";

        // if first time unlock then create variable with current date
        if (!VariableStore.HasVariable(varName))
        {
            string time = System.DateTime.Now.ToString("MMM dd, yyyy HH:mm");
            VariableStore.CreateVariable<string>(varName, time);
            return time;
        }

        // if already unlocked then fetch the existed date of that entry
        if (VariableStore.TryGetValue(varName, out object value))
            return value.ToString();

        return "";
    }


    public string GetFirstUnlockedCharacter(string fallbackID)
    {
        foreach (CharacterProfile character in profileDatabase.characters)
        {
            if (AffectionSystem.IsDiaryUnlocked(character.characterID))
                return character.characterID;
        }

        return fallbackID;
    }
}