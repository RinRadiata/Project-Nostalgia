using UnityEngine;
using TMPro;
using System.Collections;


namespace DIALOGUE
{
    public class AutoReader : MonoBehaviour
    {
        private const int DEFAULT_CHARACTERS_READ_PER_SECOND = 6;
        private const float READ_TIME_PADDING = 0.5f; // padding to ensure the last line is read completely
        private const float MAXIMUM_READ_TIME = 99f; // maximum time to read a line
        private const float MINIMUM_READ_TIME = 1f; // minimum time to read a line
        private const string STATUS_TEXT_AUTO = "Auto";
        private const string STATUS_TEXT_SKIP = "Skipping";

        private ConversationManager conversationManager;
        private TextArchitect architect => conversationManager.architect;

        public bool skip { get; set; } = false;
        public float speed { get; set; } = 1f;

        public bool isON => co_running != null;
        private Coroutine co_running = null;

        [SerializeField] private TextMeshProUGUI statusText;
        [HideInInspector] public bool allowToggle = true;

        public void Initialize(ConversationManager conversationManager)
        {
            this.conversationManager = conversationManager;

            statusText.text = string.Empty;
        }

        public void Enable()
        {
            if (isON)
                return;

            co_running = StartCoroutine(AutoRead());
        }

        public void Disable()
        {
            if (!isON)
                return;

            StopCoroutine(co_running);
            skip = false;
            co_running = null;
            statusText.text = string.Empty; // reset status text
        }

        private IEnumerator AutoRead()
        {
            //do nothing if there is no conversation to monitor
            if (!conversationManager.isRunning)
            {
                Disable();
                yield break;
            }
            
            if (!architect.isBuilding && architect.currentText != string.Empty)
                DialogueSystem.instance.OnSystemPrompt_Next();

            while (conversationManager.isRunning)
            {
                //read and wait
                if (!skip)
                {
                    while (!architect.isBuilding && !conversationManager.isWaitingOnAutoTimer)
                        yield return null;

                    float timeStarted = Time.time;

                    while (architect.isBuilding || conversationManager.isWaitingOnAutoTimer)
                        yield return null;

                    float timeToRead = Mathf.Clamp(((float)architect.tmpro.textInfo.characterCount / DEFAULT_CHARACTERS_READ_PER_SECOND), MINIMUM_READ_TIME, MAXIMUM_READ_TIME);
                    timeToRead = Mathf.Max(timeToRead - (Time.time - timeStarted), MINIMUM_READ_TIME);
                    timeToRead = Mathf.Min(timeToRead / speed + READ_TIME_PADDING, MAXIMUM_READ_TIME);

                    yield return new WaitForSeconds(timeToRead);
                }
                //skip
                else
                {
                    architect.ForceComplete();
                    yield return new WaitForSeconds(0.05f);
                }

                DialogueSystem.instance.OnSystemPrompt_Next();//move to the next line
            }

            Disable();
        }

        public void Toggle_Auto()
        {
            if (!allowToggle)
                return;

            bool previousState = skip;
            skip = false;

            if (previousState)
                Enable();

            else
            {
                if (!isON)
                    Enable();
                else
                    Disable();
            }

            if (isON)
                statusText.text = STATUS_TEXT_AUTO;
        }

        public void Toggle_Skip()
        {
            if(!allowToggle)
                return;

            bool previousState = skip;
            skip = true;

            if (!previousState)
                Enable();

            else
            {
                if (!isON)
                    Enable();
                else
                    Disable();
            }

            if (isON)
                statusText.text = STATUS_TEXT_SKIP;
        }
    }
}