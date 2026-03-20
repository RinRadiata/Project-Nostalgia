using UnityEngine;
using TMPro;

public class DiaryEntryUI : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text dateText;
    public TMP_Text contentText;
    public TMP_Text lockText;

    public GameObject lockOverlay;
    public GameObject darkMask;

    public void Setup(DiaryEntryData data, bool unlocked, int currentAffection, string date)
    {
        titleText.text = data.title;
        dateText.text = date;

        if (unlocked)
        {
            contentText.text = data.content;

            lockOverlay.SetActive(false);
            darkMask.SetActive(false);
            lockText.gameObject.SetActive(false);
        }
        else
        {
            contentText.text = "";

            lockOverlay.SetActive(true);
            darkMask.SetActive(true);

            lockText.gameObject.SetActive(true);
            lockText.text =
                "Unlock at " + data.requiredAffection +
                "\n(Current: " + currentAffection + ")";
        }
    }
}