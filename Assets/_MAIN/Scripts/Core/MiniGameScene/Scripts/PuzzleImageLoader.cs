using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PuzzleImageLoader : MonoBehaviour
{
    public Image[] pieces;

    void Start()
    {
        LoadImages();
    }

    void LoadImages()
    {
        VariableStore.TryGetValue("Minigame.currentCharacter", out object charObj);
        VariableStore.TryGetValue("Minigame.currentID", out object idObj);

        if (charObj == null || idObj == null)
        {
            Debug.LogError("Minigame data missing!");
            return;
        }

        string characterID = charObj.ToString();
        string memoryID = idObj.ToString();

        string path = $"Minigame/{characterID}/{memoryID}";
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);

        if (sprites.Length == 0)
        {
            Debug.LogError("No sprites found at: " + path);
            return;
        }

        System.Array.Sort(sprites, (a, b) => a.name.CompareTo(b.name));

        for (int i = 0; i < pieces.Length; i++)
        {
            pieces[i].sprite = sprites[i];
        }
    }

}