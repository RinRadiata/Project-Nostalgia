using UnityEngine;
using System.Collections.Generic;
using DIALOGUE;
using System.Linq;

namespace CHARACTERS
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager instance { get; private set; }

        public Character[] allCharacters => characters.Values.ToArray();
        private Dictionary<string, Character> characters = new Dictionary<string, Character>();

        private CharacterConfigSO config => DialogueSystem.instance.config.characterConfigurationAsset;

        public const string CHARACER_CASTING_ID = " as ";

        private const string CHARACTER_NAME_ID = "<charname>";
        public string characterRootPathFormat => $"Characters/{CHARACTER_NAME_ID}";
        public string characterPrefabNameFormat => $"Character - [{CHARACTER_NAME_ID}]";
        public string characterPrefabPathFormat => $"{characterRootPathFormat}/{characterPrefabNameFormat}";

        [SerializeField] private RectTransform _characterpanel = null;
        [SerializeField] private RectTransform _characterpanel_live2D = null;
        [SerializeField] private RectTransform _characterpanel_model3D = null;
        public RectTransform characterPanel => _characterpanel;
        public RectTransform characterPanelLive2D => _characterpanel_live2D;
        public RectTransform characterPanelModel3D => _characterpanel_model3D;

        private void Awake()
        {
            instance = this;
        }

        public CharacterConfigData GetCharacterConfig(string characterName, bool getOriginal = false)
        {
            if (!getOriginal)
            {
                Character character = GetCharacter(characterName);
                if (character != null)
                    return character.config;
            }           

            return config.GetConfig(characterName);
        }

        public Character GetCharacter(string characterName, bool createIfDoesNotExist = false)
        {
            if (characters.ContainsKey(characterName.ToLower()))
                return characters[characterName.ToLower()];
            else if (createIfDoesNotExist)
                return CreateCharacter(characterName);

            return null;
        }

        public bool HasCharacter(string characterName) => characters.ContainsKey(characterName.ToLower());

        public Character CreateCharacter(string characterName, bool revealAfterCreation = false)
        {
            if (characters.ContainsKey(characterName.ToLower()))
            {
                Debug.LogWarning($"a character called '{characterName}' already exists. Didn't create a character");
                return null;
            }

            CHARACTER_INFO info = GetCharacterInfo(characterName);

            Character character = CreateCharacterFromInfo(info);

            if (info.castingName != info.name)
                character.name = info.castingName; // so that the character is keeping track of who they were casted as

            characters.Add(info.name.ToLower(), character);

            if (revealAfterCreation)
                character.Show();

            return character;
        }

        private CHARACTER_INFO GetCharacterInfo(string characterName)
        {
            CHARACTER_INFO result = new CHARACTER_INFO();

            // split the name by " as " to get the character name and casting name
            string[] nameData = characterName.Split(CHARACER_CASTING_ID, System.StringSplitOptions.RemoveEmptyEntries);

            result.name = nameData[0];

            result.castingName = nameData.Length > 1 ? nameData[1] : result.name; // if no casting name is given, use the character name

            result.config = config.GetConfig(result.castingName);

            result.prefab = GetPrefabForCharacter(result.castingName);

            result.rootCharacterFolder = FormatCharacterPath(characterRootPathFormat, result.castingName);

            return result;

        }


        private GameObject GetPrefabForCharacter(string characterName)
        {
            string prefabPath = FormatCharacterPath(characterPrefabPathFormat, characterName);
            return Resources.Load<GameObject>(prefabPath);
        }

        public string FormatCharacterPath(string path, string characterName) => path.Replace(CHARACTER_NAME_ID, characterName);

        private Character CreateCharacterFromInfo(CHARACTER_INFO info)
        {
            CharacterConfigData config = info.config;

            switch (config.characterType)
            {

                case Character.CharacterType.Text:
                    return new Character_Text(info.name, config);

                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    return new Character_Sprite(info.name, config, info.prefab, info.rootCharacterFolder);

                case Character.CharacterType.Live2D:
                   return new Character_Live2D(info.name, config, info.prefab, info.rootCharacterFolder);

                case Character.CharacterType.Model3D:
                    return new Character_Model3D(info.name, config, info.prefab, info.rootCharacterFolder);

                default:
                    return null;
            }

        }

        public void SortCharacters()
        {
            // sort the characters by their order in the character panel when they are active and visible
            List<Character> activeCharacters = characters.Values.Where(c => c.root.gameObject.activeInHierarchy && c.isVisible).ToList();
            // sort the characters by their order in the character panel when they are inactive
            List<Character> inactiveCharacters = characters.Values.Except(activeCharacters).ToList();

            // return a number that defines whether a is less/greater than b, so if get a negative number, first character a is less than the priority of b
            activeCharacters.Sort((a, b) => a.priority.CompareTo(b.priority));
            activeCharacters.Concat(inactiveCharacters);

            SortCharacters(activeCharacters);
        }

        // Sorts the characters in the order of the names given in the array(list of existing characters)
        // The characters that are not in the array will be sorted by their priority.
        public void SortCharacters(string[] characterNames)
        {
            List<Character> sortedCharacters = new List<Character>();

            sortedCharacters = characterNames
                .Select(name => GetCharacter(name))
                .Where(character => character != null)
                .ToList();

            List<Character> remainingCharacters = characters.Values
                .Except(sortedCharacters)
                .OrderBy(character => character.priority)
                .ToList();
            sortedCharacters.Reverse(); // sort the characters in reverse order so that the first character in the list is on top

            // set the priority of the characters in the sorted list to be higher than the remaining characters
            int startingPriority = remainingCharacters.Count > 0 ? remainingCharacters.Max(c => c.priority) : 0;
            for (int i = 0; i < sortedCharacters.Count; i++)
            {
                Character character = sortedCharacters[i];
                character.SetPriority(startingPriority + i + 1, autoSortCharactersOnUI: false);
            }

            List<Character> allCharacters = remainingCharacters.Concat(sortedCharacters).ToList(); // combine the two lists of characters (active & inactive) 
            SortCharacters(allCharacters);
        }
        private void SortCharacters(List<Character> charactersSortingOrder)
        {
            int i = 0;
            foreach (Character character in charactersSortingOrder)
            {
                //Debug.Log($"'{character.name} priority is {character.priority}'");
                character.root.SetSiblingIndex(i++);
                character.OnSort(i);
            }
        }

        private class CHARACTER_INFO
        {
            public string name = "";

            public string castingName = "";

            public string rootCharacterFolder = "";

            public CharacterConfigData config = null;

            public GameObject prefab = null;
        }

    }
}
