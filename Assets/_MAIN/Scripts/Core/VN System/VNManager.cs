using DIALOGUE;
using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    public class VNManager : MonoBehaviour
    {
        public static VNManager instance { get; private set; }

        [SerializeField] private VisualNovelSO config;
        public Camera mainCamera;

        private void Awake()
        {
            //instance = this;

            //VNDatabaseLinkSetup linkSetup = GetComponent<VNDatabaseLinkSetup>();
            //linkSetup.SetupExternalLinks();


            //if (VNGameSave.activeFile == null)
            //    VNGameSave.activeFile = new VNGameSave();
        
            instance = this;

            if (VNGameSave.activeFile == null)
                VNGameSave.activeFile = new VNGameSave();
        }

        public void Start()
        {
            Loadgame();
        }

        private void Loadgame()
        {
            if (VNGameSave.activeFile.newGame)
            {
                List<string> lines = FileManager.ReadTextAsset(config.startingFile);
                Conversation start = new Conversation(lines);
                DialogueSystem.instance.Say(start);
            }
            else
            {
                VNGameSave.activeFile.Activate();
            }
        }
    }
}