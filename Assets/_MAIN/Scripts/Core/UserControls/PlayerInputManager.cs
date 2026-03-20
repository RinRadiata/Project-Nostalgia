using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using History;

namespace DIALOGUE
{
    public class PlayerInputManager : MonoBehaviour
    {
        private PlayerInput input;
        private List<(InputAction action, Action<InputAction.CallbackContext> command)> actions = new List<(InputAction action, Action<InputAction.CallbackContext> command)>();

        void Awake()
        {
            input = GetComponent<PlayerInput>();

            InitializeActions();
        }

        private void InitializeActions()
        {
            actions.Add((input.actions["Next"], OnNext));
            actions.Add((input.actions["HistoryBack"], OnHistoryBack));
            actions.Add((input.actions["HistoryForward"], OnHistoryForward));
            actions.Add((input.actions["HistoryLogs"], OnHistoryToggleLog));
        }

        private void OnEnable()
        {
            foreach (var inputAction in actions)
                inputAction.action.performed += inputAction.command;
        }

        private void OnDisable()
        {
            foreach (var inputAction in actions)
                inputAction.action.performed -= inputAction.command;
        }

        public void OnNext(InputAction.CallbackContext c)
        {
            DialogueSystem.instance.OnUserPrompt_Next();
        }

        public void OnHistoryBack(InputAction.CallbackContext c)
        {
            HistoryManager.instance.GoBack();
        }
        public void OnHistoryForward(InputAction.CallbackContext c)
        {
            HistoryManager.instance.GoForward();
        }

        public void OnHistoryToggleLog(InputAction.CallbackContext c)
        {
            var log = HistoryManager.instance.logManager;

            if (!log.isOpen)
                log.Open();
            else
                log.Close();
        }
    }
}