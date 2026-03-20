using UnityEngine;
using CHARACTERS;
using TMPro;

namespace DIALOGUE
{
    [CreateAssetMenu(fileName = "Dialogue System Configuration", menuName = "Dialogue System/Dialogue Configuration Asset")]
    public class DialogueSystemConfigurationSO : ScriptableObject
    {
        public const float DEFAULT_FONTSIZE_DIALOGUE = 18f;
        public const float DEFAULT_FONTSIZE_NAME = 22f;

        public CharacterConfigSO characterConfigurationAsset;

        public Color defaultTextColor = Color.white;
        public TMP_FontAsset defaultFont;

        public float dialogueFontScale = 1.0f;
        public float defaultDialogueFontSize = DEFAULT_FONTSIZE_DIALOGUE;
        public float defaultNameFontSize = DEFAULT_FONTSIZE_NAME;
    }
}
