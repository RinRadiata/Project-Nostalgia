#if UNITY_EDITOR

using DIALOGUE;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING {

    public class TestRunFiles : MonoBehaviour
    {
        [SerializeField] private TextAsset file;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            LoadFile();
        }

        // Update is called once per frame
        void LoadFile()
        {
            List<string> lines = FileManager.ReadTextAsset(file);
            Conversation conversation = new Conversation(lines);
            DialogueSystem.instance.Say(conversation);
        }
    }
}
#endif