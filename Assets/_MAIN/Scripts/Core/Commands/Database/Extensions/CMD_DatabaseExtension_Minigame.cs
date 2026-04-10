using DIALOGUE;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COMMANDS
{
    public class CMD_DatabaseExtension_Minigame : CMD_DatabaseExtension
    {
        private static string[] PARAM_CHARACTER = new string[] { "-c", "-char" };
        private static string[] PARAM_SCENE = new string[] { "-sc", "-scene" };

        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("playminigame", new Func<string[], IEnumerator>(PlayMinigame));
        }

        private static IEnumerator PlayMinigame(string[] data)
        {
            string minigameID = data[0];
            string characterID = "";
            string sceneName = "";

            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_CHARACTER, out characterID);
            parameters.TryGetValue(PARAM_SCENE, out sceneName);

            if (sceneName == "")
            {
                Debug.LogError("Minigame scene not specified!");
                yield break;
            }

            // stop conversation coroutine - Hide() only hides the canvas, it doesn't stop RunningConversation()
            if (DialogueSystem.instance != null)
            {
                // Save file path + progress to resume after returning
                var cm = DialogueSystem.instance.conversationManager;
                if (cm.conversation != null)
                {
                    VariableStore.TrySetValue("Minigame.resumeFile", cm.conversation.file);
                    VariableStore.TrySetValue("Minigame.resumeProgress", cm.conversation.GetProgress());
                }

                cm.StopConversation();
                DialogueSystem.instance.Hide(immediate: true);
            }

            // Save variables
            VariableStore.TrySetValue("Minigame.returnScene", SceneManager.GetActiveScene().name);
            VariableStore.TrySetValue("Minigame.currentCharacter", characterID);
            VariableStore.TrySetValue("Minigame.currentID", minigameID);

            SceneManager.LoadScene(sceneName);
            yield break;
        }
    }
}