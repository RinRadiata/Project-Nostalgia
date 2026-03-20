using UnityEngine;

[CreateAssetMenu(fileName = "ProfileDatabase", menuName = "VN/Profile Database")]
public class ProfileDatabase : ScriptableObject
{
    public CharacterProfile[] characters;

    public CharacterProfile GetCharacter(string id)
    {
        // "c" stand for single "character" thats already exist
        foreach (var c in characters)
        {
            if (c.characterID == id)
                return c;
        }

        Debug.LogWarning("Character not found: " + id);
        return null;
    }
}