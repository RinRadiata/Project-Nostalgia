using UnityEngine;
using UnityEngine.UI;

public class CharacterIconUI : MonoBehaviour
{
    public Image iconImage;
    public GameObject lockOverlay;
    public Button button;

    private CharacterProfile profile;
    private DiaryPanelController panel;

    public void Setup(CharacterProfile data, DiaryPanelController owner)
    {
        profile = data;
        panel = owner;

        Refresh();
    }

    public void Refresh()
    {
        bool unlocked = AffectionSystem.IsDiaryUnlocked(profile.characterID);

        iconImage.sprite = unlocked ? profile.unlockedIcon : profile.lockedIcon;

        if (lockOverlay != null)
            lockOverlay.SetActive(!unlocked);

        if (button != null)
            button.interactable = unlocked;
    }

    public void OnClick()
    {
        if (!AffectionSystem.IsDiaryUnlocked(profile.characterID))
            return;

        panel.LoadCharacter(profile.characterID);
    }
}