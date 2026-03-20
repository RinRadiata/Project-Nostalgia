using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using CHARACTERS;
using System.Linq;

namespace COMMANDS
{

    public class CMD_DatabaseExtension_Characters : CMD_DatabaseExtension
    {
        private static string[] PARAM_IMMEDIATE => new string[] { "-i", "-immediate" };
        private static string[] PARAM_ENABLE => new string[] { "-e", "-enable" };
        private static string[] PARAM_SPEED => new string[] { "-spd", "-speed" };
        private static string[] PARAM_SMOOTH => new string[] { "-sm", "-smooth" };
        private static string PARAM_XPOS => "-x";
        private static string PARAM_YPOS => "-y";

        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("createcharacter", new Action<string[]>(CreateCharacter));
            database.AddCommand("movecharacter", new Func<string[], IEnumerator>(MoveCharacter));
            database.AddCommand("show", new Func<string[], IEnumerator>(ShowAll));
            database.AddCommand("hide", new Func<string[], IEnumerator>(HideAll));
            database.AddCommand("sort", new Action<string[]>(Sort));
            database.AddCommand("highlight", new Func<string[], IEnumerator>(HighlightAll));
            database.AddCommand("unhighlight", new Func<string[], IEnumerator>(UnHighlightAll));

            //Add commands to characters
            CommandDatabase baseCommands = CommandManager.instance.CreateSubDatabase(CommandManager.DATABASE_CHARACTERS_BASE);
            baseCommands.AddCommand("move", new Func<string[], IEnumerator>(MoveCharacter));
            baseCommands.AddCommand("show", new Func<string[], IEnumerator>(Show));
            baseCommands.AddCommand("hide", new Func<string[], IEnumerator>(Hide));
            baseCommands.AddCommand("setpriority", new Action<string[]>(SetPriority));
            baseCommands.AddCommand("setColor", new Func<string[], IEnumerator>(SetColor));
            baseCommands.AddCommand("highlight", new Func<string[], IEnumerator>(Highlight));
            baseCommands.AddCommand("unhighlight", new Func<string[], IEnumerator>(UnHighlight));
            baseCommands.AddCommand("setposition", new Action<string[]>(SetPosition));
            baseCommands.AddCommand("animate", new Func<string[], IEnumerator>(Animate));

            //Add character specific databases
            CommandDatabase spriteCommands = CommandManager.instance.CreateSubDatabase(CommandManager.DATABASE_CHARACTERS_SPRITE);
            spriteCommands.AddCommand("setsprite", new Func<string[], IEnumerator>(SetSprite));

        }

        public static void CreateCharacter(string[] data)
        {
            string characterName = data[0];
            bool enable = false;
            bool immediate = false;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_ENABLE, out enable, defaultValue: false);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            Character character = CharacterManager.instance.CreateCharacter(characterName);

            if (!enable)
                return;

            if (immediate)
                character.isVisible = true;
            else
                character.Show();
        }

        private static void Sort(string[] data)
        {
            CharacterManager.instance.SortCharacters(data);
        }

        public static IEnumerator MoveCharacter(string[] data)
        {
            string characterName = data[0];
            Character character = CharacterManager.instance.GetCharacter(characterName);

            if (character == null)
                yield break;

            float x = 0, y = 0;
            float speed = 1;
            bool immediate = false;
            bool smooth = false;

            var parameters = ConvertDataToParameters(data);

            //try to get the x axis position
            parameters.TryGetValue(PARAM_XPOS, out x);

            //try to get the x axis position
            parameters.TryGetValue(PARAM_YPOS, out y);

            //try to get the speed
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1);

            //try to get the smoothing
            parameters.TryGetValue(PARAM_SMOOTH, out smooth, defaultValue: false);

            //try to get immediate setting of position
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            Vector2 position = new Vector2(x, y);

            if (immediate)
                character.SetPosition(position);
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { character?.SetPosition(position); });
                yield return character.MoveToPosition(position, speed, smooth);
            }

        }


        public static IEnumerator ShowAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            float speed = 1f;

            foreach (string s in data)
            {
                Character character = CharacterManager.instance.GetCharacter(s, createIfDoesNotExist: false);
                if (character != null)
                    characters.Add(character);
            }

            if (characters.Count == 0)
                yield break;

            //convert the data array to a parameter container
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            //call the logic of all the character
            foreach (Character character in characters)
            {
                if (immediate)
                    character.isVisible = true;
                else
                    character.Show(speed);
            }

            if (!immediate)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                        character.isVisible = true;
                });

                while (characters.Any(c => c.isRevealing))
                    yield return null;
            }
        }

        public static IEnumerator HideAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            float speed = 1f;

            foreach (string s in data)
            {
                Character character = CharacterManager.instance.GetCharacter(s, createIfDoesNotExist: false);
                if (character != null)
                    characters.Add(character);
            }

            if (characters.Count == 0)
                yield break;

            //convert the data array to a parameter container
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            //call the logic of all the character
            foreach (Character character in characters)
            {
                if (immediate)
                    character.isVisible = false;
                else
                    character.Hide(speed);
            }

            if (!immediate)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                        character.isVisible = false;
                });

                while (characters.Any(c => c.isHiding))
                    yield return null;
            }
        }

        public static IEnumerator HighlightAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            bool handleUnspecifiedCharacters = true;
            List<Character> unspecifiedCharacters = new List<Character>();

            //add any character specified to be highlighted
            for (int i = 0; i < data.Length; i++)
            {
                Character character = CharacterManager.instance.GetCharacter(data[i], createIfDoesNotExist: false);
                if (character != null)
                    characters.Add(character);
            }

            if (characters.Count == 0)
                yield break;

            //grab extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            parameters.TryGetValue(new string[] { "-i", "immediate" }, out immediate, defaultValue: false);
            parameters.TryGetValue(new string[] { "-o", "-only" }, out handleUnspecifiedCharacters, defaultValue: true);

            //make all character perform the logic
            foreach (Character character in characters)
                character.Highlight(immediate: immediate);

            //if we are forcing any unspecified characters to use the opposite highlighted status
            if (handleUnspecifiedCharacters)
            {
                foreach (Character character in CharacterManager.instance.allCharacters)
                {
                    if (characters.Contains(character))
                        continue;

                    unspecifiedCharacters.Add(character);
                    character.UnHighlight(immediate: immediate);
                }
            }

            //wait for all character to finish highlighting
            if (!immediate)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (var character in characters)
                        character.UnHighlight(immediate: true);

                    if (!handleUnspecifiedCharacters) return;

                    foreach (var character in unspecifiedCharacters)
                        character.UnHighlight(immediate: true);
                });

                while (characters.Any(c => c.isHighlighting) || (handleUnspecifiedCharacters && unspecifiedCharacters.Any(uc => uc.isUnHighlighting)))
                    yield return null;
            }
        }

        public static IEnumerator UnHighlightAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            bool handleUnspecifiedCharacters = true;
            List<Character> unspecifiedCharacters = new List<Character>();

            //add any character specified to be highlighted
            for (int i = 0; i < data.Length; i++)
            {
                Character character = CharacterManager.instance.GetCharacter(data[i], createIfDoesNotExist: false);
                if (character != null)
                    characters.Add(character);
            }

            if (characters.Count == 0)
                yield break;

            //grab extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            parameters.TryGetValue(new string[] { "-i", "immediate" }, out immediate, defaultValue: false);
            parameters.TryGetValue(new string[] { "-o", "-only" }, out handleUnspecifiedCharacters, defaultValue: true);

            //make all character perform the logic
            foreach (Character character in characters)
                character.Highlight(immediate: immediate);

            //if we are forcing any unspecified characters to use the opposite highlighted status
            if (handleUnspecifiedCharacters)
            {
                foreach (Character character in CharacterManager.instance.allCharacters)
                {
                    if (characters.Contains(character))
                        continue;

                    unspecifiedCharacters.Add(character);
                    character.UnHighlight(immediate: immediate);
                }
            }

            //wait for all character to finish highlighting
            if (!immediate)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (var character in characters)
                        character.UnHighlight(immediate: true);

                    if (!handleUnspecifiedCharacters) return;

                    foreach (var character in unspecifiedCharacters)
                        character.UnHighlight(immediate: true);
                });
                while (characters.Any(c => c.isUnHighlighting) || (handleUnspecifiedCharacters && unspecifiedCharacters.Any(uc => uc.isHighlighting)))
                    yield return null;
            }
        }


        #region BASE CHARACTER COMMANDS
        public static void SetPriority(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            int priority;

            if (character == null || data.Length < 2)
                return;

            if (!int.TryParse(data[1], out priority))
                priority = 0; //default priority if parsing fails

            character.SetPriority(priority);
        }

        public static IEnumerator SetColor(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            string colorName;
            float speed;
            bool immediate;

            if (character == null || data.Length < 2)
                yield break;

            //grab the extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            //try to get the color name
            parameters.TryGetValue(new string[] { "-c", "-color" }, out colorName);
            //try to get the speed
            bool specifiedSpeed = parameters.TryGetValue(new string[] { "-spd", "-speed" }, out speed, defaultValue: 1f);
            //try to get the instant value
            if (!specifiedSpeed)
                parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: true);
            else
                immediate = false;

            //get the color value from the name
            Color color = Color.white;
            color = color.GetColorFromName(colorName);

            if (immediate)
                character.SetColor(color);
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { character?.SetColor(color); });
                character.TransitionColor(color, speed);
            }

            yield break;
        }
        public static IEnumerator Show(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0]);

            if (character == null)
                yield break;

            bool immediate = false;
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.isVisible = true;
            else
            {
                //a long running process should have a stop action to cancel out the couroutine and run logic that should complete this command
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { if (character != null) character.isVisible = true; });

                yield return character.Show();
            }
        }

        public static IEnumerator Hide(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0]);

            if (character == null)
                yield break;

            bool immediate = false;
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.isVisible = false;
            else
            {
                //a long running process should have a stop action to cancel out the couroutine and run logic that should complete this command
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { if (character != null) character.isVisible = false; });

                yield return character.Hide();
            }
        }

        public static IEnumerator Highlight(string[] data)
        {
            //format: SetSprite(character sprite)
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false) as Character;

            if (character == null)
                yield break;

            bool immediate = false;

            //grab the extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.Highlight(immediate: true);
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { character?.Highlight(immediate: true); });
                yield return character.Highlight();
            }
        }

        public static IEnumerator UnHighlight(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false) as Character;

            if (character == null)
                yield break;

            bool immediate = false;
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.UnHighlight(immediate: true);
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { character?.UnHighlight(immediate: true); });
                yield return character.UnHighlight();
            }
        }

        public static void SetPosition(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            float x = 0, y = 0;

            if (character == null || data.Length < 2)
                return;

            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            parameters.TryGetValue(PARAM_XPOS, out x, defaultValue: 0);
            parameters.TryGetValue(PARAM_YPOS, out y, defaultValue: 0);

            character.SetPosition(new Vector2(x, y));
        }

        public static IEnumerator Animate(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);

            if (character == null || data.Length < 2)
                yield break;

            string animName;
            bool animState;

            // Convert remaining data to parameters starting from index 1 to skip character name
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            // Try to get the animation name parameter, if not found use the first unlabeled parameter
            if (!parameters.TryGetValue(new string[] { "-a", "-anim" }, out animName))
            {
                animName = data[1];
            }

            // character facing commands
            if (animName.Equals("FaceRight", StringComparison.OrdinalIgnoreCase))
            {
                yield return character.FaceRight();
                yield break;
            }
            else if (animName.Equals("FaceLeft", StringComparison.OrdinalIgnoreCase))
            {
                yield return character.FaceLeft();
                yield break;
            }

            else if (animName.Equals("Flip", StringComparison.OrdinalIgnoreCase))
            {
                yield return character.Flip();
                yield break;
            }

            // Check if a boolean state is provided
            if (parameters.TryGetValue(new string[] { "-s", "-state" }, out animState))
            {
                // Call animate with boolean parameter
                character.Animate(animName, animState);
            }
            else
            {
                // Call animate with just the trigger name
                character.Animate(animName);
            }

            yield break;
        }
        #endregion

        #region SPRITE CHARACTER COMMANDS
        public static IEnumerator SetSprite(string[] data)
        {
            //format: SetSprite(character sprite)
            Character_Sprite character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false) as Character_Sprite;
            int layer = 0;
            string spriteName;
            bool immediate = false;
            float speed;

            if (character == null || data.Length < 2)
                yield break;

            //grab the extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            //try to get the sprite name
            parameters.TryGetValue(new string[] { "-s", "-sprite" }, out spriteName);
            //try to get the layer
            parameters.TryGetValue(new string[] { "-l", "-layer" }, out layer, defaultValue: 0);

            //try to get the transition speed
            bool specifiedSpeed = parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 0.1f);

            //try to get whether this is an immediate transition or not
            if (!specifiedSpeed)
                parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: true);

            //run the logic
            Sprite sprite = character.GetSprite(spriteName);

            if (sprite == null)
                yield break;

            if (immediate)
            {
                character.SetSprite(sprite, layer);
            }
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { character?.SetSprite(sprite, layer); });
                yield return character.TransitionSrpite(sprite, layer, speed);

            }
        }
        #endregion

    }
}