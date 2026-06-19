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
        if (data == null)
            return;

        if (titleText != null)
            titleText.text = data.title;

        if (dateText != null)
            dateText.text = date;

        if (unlocked)
        {
            if (contentText != null)
                contentText.text = data.content;

            if (lockOverlay != null)
                lockOverlay.SetActive(false);

            if (darkMask != null)
                darkMask.SetActive(false);

            if (lockText != null)
                lockText.gameObject.SetActive(false);
        }
        else
        {
            if (contentText != null)
                contentText.text = "";

            if (lockOverlay != null)
                lockOverlay.SetActive(true);

            if (darkMask != null)
                darkMask.SetActive(true);

            if (lockText != null)
            {
                lockText.gameObject.SetActive(true);
                lockText.text = data.GetLockMessage(currentAffection);
            }
        }
    }
}