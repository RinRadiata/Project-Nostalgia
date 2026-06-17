using CHARACTERS;
using System.Collections.Generic;
using UnityEngine;
using static History.CharacterData.AnimationData;

namespace History
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public string castingName;
        public string displayName;
        public bool enabled;
        public Color color;
        public int priority;
        public bool isHighlighted;
        public bool isFacingLeft;
        public Vector2 position;
        public CharacterConfigCache characterConfig;

        public string animationJSON;
        public string dataJSON;

        [System.Serializable]
        public class CharacterConfigCache
        {
            public string name;
            public string alias;

            public Character.CharacterType characterType;

            public Color nameColor;
            public Color dialogueColor;

            public string nameFont;
            public string dialogueFont;

            public float nameFontScale = 1f;
            public float dialogueFontScale = 1f;

            public CharacterConfigCache(CharacterConfigData reference)
            {
                name = reference.name;
                alias = reference.alias;
                characterType = reference.characterType;

                nameColor = reference.nameColor;
                dialogueColor = reference.dialogueColor;

                nameFont = FilePaths.resources_font + reference.nameFont.name;
                dialogueFont = FilePaths.resources_font + reference.dialogueFont.name;

                nameFontScale = reference.nameFontScale;
                dialogueFontScale = reference.dialogueFontScale;
            }
        }

        public static List<CharacterData> Capture()
        {
            List<CharacterData> characters = new List<CharacterData>();

            foreach (var character in CharacterManager.instance.allCharacters)
            {
                if (!character.isVisible)
                    continue;

                CharacterData entry = new CharacterData();

                entry.characterName = character.name;
                entry.castingName = character.castingName;
                entry.displayName = character.displayName;
                entry.enabled = character.isVisible;
                entry.color = character.color;
                entry.priority = character.priority;
                entry.isHighlighted = character.highlighted;
                entry.position = character.targetPosition;
                entry.isFacingLeft = character.isFacingLeft;
                entry.characterConfig = new CharacterConfigCache(character.config);
                entry.animationJSON = GetAnimationData(character);

                switch (character.config.characterType)
                {
                    case Character.CharacterType.Sprite:
                    case Character.CharacterType.SpriteSheet:
                        SpriteData sData = new SpriteData();
                        sData.layers = new List<SpriteData.LayerData>();

                        Character_Sprite sc = character as Character_Sprite;

                        if (sc != null)
                        {
                            foreach (var layer in sc.layers)
                            {
                                var layerData = new SpriteData.LayerData();
                                layerData.color = layer.renderer.color;
                                layerData.spriteName = layer.renderer.sprite.name;
                                sData.layers.Add(layerData);
                            }
                        }

                        entry.dataJSON = JsonUtility.ToJson(sData);
                        break;

                    case Character.CharacterType.Live2D:
                        Live2DData l2Data = new Live2DData();
                        Character_Live2D lc = character as Character_Live2D;

                        if (lc != null)
                        {
                            l2Data.expression = lc.activeExpression;
                            l2Data.motion = lc.activeMotion;
                        }

                        entry.dataJSON = JsonUtility.ToJson(l2Data);
                        break;

                    case Character.CharacterType.Model3D:
                        Model3DData m3Data = new Model3DData();
                        Character_Model3D mc = character as Character_Model3D;

                        if (mc != null)
                        {
                            m3Data.position = mc.model.position;
                            m3Data.rotation = mc.model.rotation;
                        }

                        entry.dataJSON = JsonUtility.ToJson(m3Data);
                        break;
                }

                characters.Add(entry);
            }

            return characters;
        }

        public static void Apply(List<CharacterData> data)
        {
            if (data == null)
                return;

            List<string> cache = new List<string>();

            foreach (CharacterData characterData in data)
            {
                if (characterData == null)
                    continue;

                Character character = null;

                if (string.IsNullOrEmpty(characterData.castingName))
                {
                    character = CharacterManager.instance.GetCharacter(characterData.characterName, createIfDoesNotExist: true);
                }
                else
                {
                    character = CharacterManager.instance.GetCharacter(characterData.characterName, createIfDoesNotExist: false);

                    if (character == null)
                    {
                        string castingName = $"{characterData.characterName}{CharacterManager.CHARACER_CASTING_ID}{characterData.castingName}";
                        character = CharacterManager.instance.CreateCharacter(castingName);
                    }
                }

                if (character == null)
                    continue;

                character.displayName = characterData.displayName;
                character.SetColor(characterData.color);

                if (characterData.isHighlighted)
                    character.Highlight(immediate: true);
                else
                    character.UnHighlight(immediate: true);

                character.SetPriority(characterData.priority);

                if (characterData.isFacingLeft)
                    character.FaceLeft(immediate: true);
                else
                    character.FaceRight(immediate: true);

                character.SetPosition(characterData.position);

                character.isVisible = characterData.enabled;

                AnimationData animationData = null;

                if (!string.IsNullOrWhiteSpace(characterData.animationJSON))
                    animationData = JsonUtility.FromJson<AnimationData>(characterData.animationJSON);

                // Live2D use it's own motion system via Character_Live2D.SetMotion().
                // Do not force Animator Refresh for Live2D because Live2D Animator may not have a "Refresh" parameter.
                if (character.config.characterType != Character.CharacterType.Live2D)
                    ApplyAnimationData(character, animationData);

                switch (character.config.characterType)
                {
                    case Character.CharacterType.Sprite:
                    case Character.CharacterType.SpriteSheet:
                        SpriteData sData = null;

                        if (!string.IsNullOrWhiteSpace(characterData.dataJSON))
                            sData = JsonUtility.FromJson<SpriteData>(characterData.dataJSON);

                        Character_Sprite sc = character as Character_Sprite;

                        if (sData != null && sData.layers != null && sc != null && sc.layers != null)
                        {
                            int count = Mathf.Min(sData.layers.Count, sc.layers.Count);

                            for (int i = 0; i < count; i++)
                            {
                                var layer = sData.layers[i];

                                if (layer == null || string.IsNullOrWhiteSpace(layer.spriteName))
                                    continue;

                                if (sc.layers[i].renderer.sprite != null &&
                                    sc.layers[i].renderer.sprite.name != layer.spriteName)
                                {
                                    Sprite sprite = sc.GetSprite(layer.spriteName);

                                    if (sprite != null)
                                        sc.SetSprite(sprite, i);
                                    else
                                        Debug.LogWarning($"History state: Could NOT load sprite: '{layer.spriteName}'.");
                                }
                            }
                        }
                        break;

                    case Character.CharacterType.Live2D:
                        Live2DData l2Data = null;

                        if (!string.IsNullOrWhiteSpace(characterData.dataJSON))
                            l2Data = JsonUtility.FromJson<Live2DData>(characterData.dataJSON);

                        Character_Live2D lc = character as Character_Live2D;

                        if (l2Data != null && lc != null)
                        {
                            if (!string.IsNullOrWhiteSpace(l2Data.expression) &&
                                lc.activeExpression != l2Data.expression)
                            {
                                lc.SetExpression(l2Data.expression);
                            }

                            if (!string.IsNullOrWhiteSpace(l2Data.motion) &&
                                lc.activeMotion != l2Data.motion)
                            {
                                lc.SetMotion(l2Data.motion);
                            }
                        }
                        break;

                    case Character.CharacterType.Model3D:
                        Model3DData m3Data = null;

                        if (!string.IsNullOrWhiteSpace(characterData.dataJSON))
                            m3Data = JsonUtility.FromJson<Model3DData>(characterData.dataJSON);

                        Character_Model3D mc = character as Character_Model3D;

                        if (m3Data != null && mc != null)
                        {
                            mc.model.position = m3Data.position;
                            mc.model.rotation = m3Data.rotation;
                        }
                        break;
                }

                cache.Add(character.name);
            }

            foreach (Character character in CharacterManager.instance.allCharacters)
            {
                if (!cache.Contains(character.name))
                    character.isVisible = false;
            }
        }

        private static string GetAnimationData(Character character)
        {
            AnimationData data = new AnimationData();

            if (character == null || character.animator == null)
                return JsonUtility.ToJson(data);

            Animator animator = character.animator;

            foreach (var param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                    continue;

                AnimationParameter pData = new AnimationParameter { name = param.name };

                switch (param.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        pData.type = "Bool";
                        pData.value = animator.GetBool(param.name).ToString();
                        break;

                    case AnimatorControllerParameterType.Float:
                        pData.type = "Float";
                        pData.value = animator.GetFloat(param.name).ToString();
                        break;

                    case AnimatorControllerParameterType.Int:
                        pData.type = "Int";
                        pData.value = animator.GetInteger(param.name).ToString();
                        break;
                }

                data.parameters.Add(pData);
            }

            return JsonUtility.ToJson(data);
        }

        private static void ApplyAnimationData(Character character, AnimationData data)
        {
            if (character == null || data == null || data.parameters == null)
                return;

            Animator animator = character.animator;

            if (animator == null)
                return;

            foreach (var param in data.parameters)
            {
                if (param == null || string.IsNullOrWhiteSpace(param.name))
                    continue;

                switch (param.type)
                {
                    case "Bool":
                        if (HasAnimatorParameter(animator, param.name, AnimatorControllerParameterType.Bool) &&
                            bool.TryParse(param.value, out bool boolValue))
                        {
                            animator.SetBool(param.name, boolValue);
                        }
                        break;

                    case "Float":
                        if (HasAnimatorParameter(animator, param.name, AnimatorControllerParameterType.Float) &&
                            float.TryParse(param.value, out float floatValue))
                        {
                            animator.SetFloat(param.name, floatValue);
                        }
                        break;

                    case "Int":
                        if (HasAnimatorParameter(animator, param.name, AnimatorControllerParameterType.Int) &&
                            int.TryParse(param.value, out int intValue))
                        {
                            animator.SetInteger(param.name, intValue);
                        }
                        break;
                }
            }

            if (HasAnimatorParameter(animator, Character.ANIMATION_REFRESH_TRIGGER, AnimatorControllerParameterType.Trigger))
                animator.SetTrigger(Character.ANIMATION_REFRESH_TRIGGER);
        }

        private static bool HasAnimatorParameter(Animator animator, string parameterName, AnimatorControllerParameterType type)
        {
            if (animator == null || string.IsNullOrWhiteSpace(parameterName))
                return false;

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == parameterName && parameter.type == type)
                    return true;
            }

            return false;
        }

        [System.Serializable]
        public class AnimationData
        {
            public List<AnimationParameter> parameters = new List<AnimationParameter>();

            [System.Serializable]
            public class AnimationParameter
            {
                public string name;
                public string type;
                public string value;
            }
        }

        [System.Serializable]
        public class SpriteData
        {
            public List<LayerData> layers;

            [System.Serializable]
            public class LayerData
            {
                public string spriteName;
                public Color color;
            }
        }

        [System.Serializable]
        public class Live2DData
        {
            public string expression;
            public string motion;
        }

        [System.Serializable]
        public class Model3DData
        {
            public Vector3 position;
            public Quaternion rotation;
        }
    }
}