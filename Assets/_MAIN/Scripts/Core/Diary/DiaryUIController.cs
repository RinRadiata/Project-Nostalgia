using UnityEngine;
using System.Collections;

public class DiaryUIController : MonoBehaviour
{
    public GameObject diaryCanvas;
    public CanvasGroup diaryGroup;
    public DiaryPanelController diaryPanel;

    public string defaultCharacterID;

    private bool isOpen;
    private bool isAnimating;

    public void ToggleDiary()
    {
        if (isAnimating) return;

        isOpen = !isOpen;

        if (isOpen)
            OpenDiary();
        else
            CloseDiary();
    }

    public void OpenDiary()
    {
        isOpen = true;

        diaryCanvas.SetActive(true);

        foreach (var character in diaryPanel.profileDatabase.characters)
            AffectionSystem.AddAffection(character.characterID, 0);

        diaryPanel.RefreshCharacters();

        string idToLoad =
            diaryPanel.GetFirstUnlockedCharacter(defaultCharacterID);

        diaryPanel.LoadCharacter(idToLoad);

        StartCoroutine(Fade(0, 1, true));
    }

    public void CloseDiary()
    {
        StartCoroutine(Fade(1, 0, false));
    }

    IEnumerator Fade(float start, float end, bool interactable)
    {
        isAnimating = true;

        float t = 0;
        float duration = 0.25f;

        while (t < duration)
        {
            t += Time.deltaTime;
            diaryGroup.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }

        diaryGroup.interactable = interactable;
        diaryGroup.blocksRaycasts = interactable;

        if (!interactable)
            diaryCanvas.SetActive(false);

        isAnimating = false;
    }
}