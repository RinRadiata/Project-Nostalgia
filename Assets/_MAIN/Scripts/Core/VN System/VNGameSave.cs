using DIALOGUE;
using History;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VISUALNOVEL
{
    [System.Serializable]
    public class VNGameSave
    {
        public static VNGameSave activeFile = null;

        public const string FILE_TYPE = ".vns";
        public const string SCREENSHOT_FILE_TYPE = ".jpg";
        public const bool ENCRYPT = true;
        public const float SCREENSHOT_DOWNSCALE_AMOUNT = 0.25f;

        public string filePath => $"{FilePaths.gameSaves}{slotNumber}{FILE_TYPE}";
        public string screenshotPath => $"{FilePaths.gameSaves}{slotNumber}{SCREENSHOT_FILE_TYPE}";

        public string playerName;
        public int slotNumber = 1;
        public bool newGame = true;

        public string[] activeConversations;
        public HistoryState activeState;
        public HistoryState[] historyLogs;

        public VN_VariableData[] variables;

        public string timestamp;
        public static VNGameSave Load(string filePath, bool activateOnLoad = false)
        {
            VNGameSave save = FileManager.Load<VNGameSave>(filePath, ENCRYPT);

            activeFile = save;

            if (activateOnLoad && save != null)
                save.Activate();

            return save;
        }

        public void Save()
        {
            newGame = false;

            activeState = HistoryState.Capture();
            historyLogs = HistoryManager.instance.history.ToArray();
            activeConversations = GetConversationData();

            // Save ALL VN variables (including affection, diary, minigame)
            variables = GetVariableData();

            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            ScreenshotMaster.CaptureScreenshot(
                VNManager.instance.mainCamera,
                Screen.width,
                Screen.height,
                SCREENSHOT_DOWNSCALE_AMOUNT,
                screenshotPath
            );

            FileManager.Save(filePath, JsonUtility.ToJson(this), ENCRYPT);
        }

        public void Activate()
        {
            if (activeState != null)
                activeState.Load();

            HistoryManager.instance.history = historyLogs?.ToList() ?? new List<HistoryState>();
            HistoryManager.instance.logManager.Clear();
            HistoryManager.instance.logManager.Rebuild();

            // Restore all VN variables (affection included)
            SetVariableData();

            SetConversationData();

            DialogueSystem.instance.prompt.Hide();
        }

        private VN_VariableData[] GetVariableData()
        {
            List<VN_VariableData> retData = new();

            if (VariableStore.databases == null)
                return retData.ToArray();

            foreach (var db in VariableStore.databases.Values)
            {
                if (db == null || db.variables == null)
                    continue;

                foreach (var v in db.variables)
                {
                    if (v.Value == null)
                        continue;

                    VN_VariableData d = new();
                    d.name = $"{db.name}.{v.Key}";
                    var value = v.Value.Get();
                    d.value = value != null ? $"{value}" : string.Empty;
                    d.type = value != null ? value.GetType().ToString() : "null";

                    retData.Add(d);
                }
            }

            return retData.ToArray();
        }

        private void SetVariableData()
        {
            if (variables == null)
                return;

            foreach (var variable in variables)
            {
                // Ensure database exists
                string[] parts = variable.name.Split('.');
                if (parts.Length > 1)
                    VariableStore.CreateDatabase(parts[0]);

                // If variable doesn't exist, create it first
                if (!VariableStore.HasVariable(variable.name))
                {
                    switch (variable.type)
                    {
                        case "System.Boolean":
                            VariableStore.CreateVariable<bool>(variable.name, false);
                            break;

                        case "System.Int32":
                            VariableStore.CreateVariable<int>(variable.name, 0);
                            break;

                        case "System.Single":
                            VariableStore.CreateVariable<float>(variable.name, 0f);
                            break;

                        case "System.String":
                            VariableStore.CreateVariable<string>(variable.name, "");
                            break;
                    }
                }

                // Now safely set value
                switch (variable.type)
                {
                    case "System.Boolean":
                        if (bool.TryParse(variable.value, out bool b))
                            VariableStore.TrySetValue(variable.name, b);
                        break;

                    case "System.Int32":
                        if (int.TryParse(variable.value, out int i))
                            VariableStore.TrySetValue(variable.name, i);
                        break;

                    case "System.Single":
                        if (float.TryParse(variable.value, out float f))
                            VariableStore.TrySetValue(variable.name, f);
                        break;

                    case "System.String":
                        VariableStore.TrySetValue(variable.name, variable.value);
                        break;
                }
            }
        }

        private string[] GetConversationData()
        {
            List<string> retData = new();
            var conversations = DialogueSystem.instance.conversationManager.GetConversationQueue();

            foreach (var conversation in conversations)
            {
                string data = "";

                if (!string.IsNullOrEmpty(conversation.file))
                {
                    var compressedData = new VN_ConversationDataCompressed
                    {
                        fileName = conversation.file,
                        progress = conversation.GetProgress(),
                        startIndex = conversation.fileStartIndex,
                        endIndex = conversation.fileEndIndex
                    };

                    data = JsonUtility.ToJson(compressedData);
                }
                else
                {
                    var fullData = new VN_ConversationData
                    {
                        conversation = conversation.GetLines(),
                        progress = conversation.GetProgress()
                    };

                    data = JsonUtility.ToJson(fullData);
                }

                retData.Add(data);
            }

            return retData.ToArray();
        }

        private void SetConversationData()
        {
            if (activeConversations == null)
                return;

            for (int i = 0; i < activeConversations.Length; i++)
            {
                try
                {
                    string data = activeConversations[i];
                    Conversation conversation = null;

                    var fullData = JsonUtility.FromJson<VN_ConversationData>(data);

                    if (fullData != null && fullData.conversation != null && fullData.conversation.Count > 0)
                    {
                        conversation = new Conversation(fullData.conversation, fullData.progress);
                    }
                    else
                    {
                        var compressedData = JsonUtility.FromJson<VN_ConversationDataCompressed>(data);

                        if (compressedData != null && !string.IsNullOrEmpty(compressedData.fileName))
                        {
                            TextAsset file = Resources.Load<TextAsset>(compressedData.fileName);

                            int count = compressedData.endIndex - compressedData.startIndex;

                            List<string> lines = FileManager
                                .ReadTextAsset(file)
                                .Skip(compressedData.startIndex)
                                .Take(count + 1)
                                .ToList();

                            conversation = new Conversation(
                                lines,
                                compressedData.progress,
                                compressedData.fileName,
                                compressedData.startIndex,
                                compressedData.endIndex
                            );
                        }
                    }

                    if (conversation != null && conversation.GetLines().Count > 0)
                    {
                        if (i == 0)
                            DialogueSystem.instance.conversationManager.StartConversation(conversation);
                        else
                            DialogueSystem.instance.conversationManager.Enqueue(conversation);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error restoring conversation: {e}");
                }
            }
        }
    }
}