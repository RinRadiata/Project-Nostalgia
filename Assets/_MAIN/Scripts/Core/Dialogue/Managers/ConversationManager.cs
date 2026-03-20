using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using COMMANDS;
using CHARACTERS;
using DIALOGUE.LogicalLines;

namespace DIALOGUE
{
    public class ConversationManager
    {
        private DialogueSystem dialogueSystem => DialogueSystem.instance;

        private Coroutine process = null;
        public bool isRunning => process != null;
        public bool isOnLogicalLine { get; private set; } = false;

        public TextArchitect architect = null;

        private bool userPrompt = false;

        private LogicalLineManager logicalLineManager;

        public Conversation conversation => (conversationQueue.IsEmpty() ? null : conversationQueue.top);
        public int conversationProgress => (conversationQueue.IsEmpty() ? -1 : conversationQueue.top.GetProgress());
        private ConversationQueue conversationQueue;

        public bool allowUserPrompts = true;

        public ConversationManager(TextArchitect architect)
        {
            this.architect = architect;
            dialogueSystem.onUserPrompt_Next += OnUserPrompt_Next;

            logicalLineManager = new LogicalLineManager();

            conversationQueue = new ConversationQueue();
        }

        public Conversation[] GetConversationQueue() => conversationQueue.GetReadOnly();

        public void Enqueue(Conversation conversation) => conversationQueue.Enqueue(conversation);
        public void EnqueuePriority(Conversation conversation) => conversationQueue.EnqueuePriority(conversation);

        private void OnUserPrompt_Next()
        {
            if (allowUserPrompts)
                userPrompt = true;
        }

        public Coroutine StartConversation(Conversation conversation)
        {
            StopConversation();
            conversationQueue.Clear();

            Enqueue(conversation);

            process = dialogueSystem.StartCoroutine(RunningConversation());

            return process;
        }

        public void StopConversation()
        {
            if (!isRunning)
            {
                return;
            }

            dialogueSystem.StopCoroutine(process);
            process = null;
        }

        IEnumerator RunningConversation()
        {
            while (!conversationQueue.IsEmpty())
            {
                Conversation currentConversation = conversation;

                if (currentConversation.HasReachedEnd())
                {
                    conversationQueue.Dequeue();
                    continue;
                }

                string rawLine = currentConversation.CurrentLine();
                // Skip blank lines
                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    TryAdvanceConversation(currentConversation);
                    continue;
                }
                    

                DIALOGUE_LINE line = DialogueParser.Parse(rawLine);

                if (logicalLineManager.TryGetLogic(line, out Coroutine logic))
                {
                    isOnLogicalLine = true;
                    yield return logic; // Wait for the logical line to complete
                }
                else
                {
                    // Show dialogue
                    if (line.hasDialogue)
                        yield return Line_RunDialogue(line);

                    // Run any commands
                    if (line.hasCommands)
                        yield return Line_RunCommands(line);

                    //wait for user input if dialogue was in this line
                    if (line.hasDialogue)
                    {
                        // Wait for user input
                        yield return WaitForUserInput();

                        CommandManager.instance.StopAllProcesses();

                        dialogueSystem.OnSystemPrompt_Clear();
                    }
                }

                TryAdvanceConversation(currentConversation);
                isOnLogicalLine = false;
            }

            process = null; // Reset the process
        }

        private void TryAdvanceConversation(Conversation conversation)
        {
            conversation.IncrementProgress();

            if (conversation != conversationQueue.top)
                return;

            if (conversation.HasReachedEnd())
                conversationQueue.Dequeue();
        }

        IEnumerator Line_RunDialogue(DIALOGUE_LINE line)
        {
            //show or hide the speaker name if there is one present
            if (line.hasSpeaker)
                HandleSpeakerLogic(line.speakerData);

            //to make sure the dialogue box is visible if it not visible automatically
            if (!dialogueSystem.dialogueContainer.isVisible)
                dialogueSystem.dialogueContainer.Show();

            //build dialogue
            yield return BuildLineSegments(line.dialogueData);

        }

        private void HandleSpeakerLogic(DL_SPEAKER_DATA speakerData)
        {
            bool characterMustBeCreated = (speakerData.makeCharacterEnter || speakerData.isCastingPosition || speakerData.isCastingExpressions);

            Character character = CharacterManager.instance.GetCharacter(speakerData.name, createIfDoesNotExist: characterMustBeCreated);

            if (speakerData.makeCharacterEnter && (!character.isVisible && !character.isRevealing))
                character.Show();

            //add the character name to the UI
            dialogueSystem.ShowSpeakerName(TagManager.Inject(speakerData.displayname));

            //now customize the dialogue for this character - if applicable
            DialogueSystem.instance.ApplySpeakerDataToDialogueContainer(speakerData.name);

            //cast position
            if (speakerData.isCastingPosition)
                character.MoveToPosition(speakerData.castPosition);

            //cast expressions
            if (speakerData.isCastingExpressions)
            {
                //ce = cast expression
                foreach (var ce in speakerData.CastExpressions)
                    character.OnReceiveCastingExpression(ce.layer, ce.expression);
            }     

        }

        IEnumerator Line_RunCommands(DIALOGUE_LINE line)
        {
            List<DL_COMMAND_DATA.Command> commands = line.commandData.commands;

            foreach (DL_COMMAND_DATA.Command command in commands)
            {
                if (command.waitForCompletion || command.name == "wait")
                {
                    CoroutineWrapper cw = CommandManager.instance.Execute(command.name, command.arguments);
                    while (!cw.IsDone)
                    {
                        if (userPrompt)
                        {
                            CommandManager.instance.StopCurrentProcess();
                            userPrompt = false;
                        }
                        yield return null;
                    }
                }
                     
                else
                    CommandManager.instance.Execute(command.name, command.arguments);
            }

            yield return null;
        }

        IEnumerator BuildLineSegments(DL_DIALOGUE_DATA line)
        {
            for (int i = 0; i < line.segments.Count; i++)
            {
                DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment = line.segments[i];

                yield return WaitForDialogueSegmentToBeTriggered(segment);

                yield return BuildDialogue(segment.dialogue, segment.appendText);
            }

        }

        public bool isWaitingOnAutoTimer { get; private set; } = false;
        IEnumerator WaitForDialogueSegmentToBeTriggered(DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment)
        {
            switch(segment.startSignal)
            {
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.C:
                    yield return WaitForUserInput();
                    dialogueSystem.OnSystemPrompt_Clear();
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.A:
                    yield return WaitForUserInput();
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WC:
                    isWaitingOnAutoTimer = true;
                    yield return new WaitForSeconds(segment.signalDelay);
                    isWaitingOnAutoTimer = false;
                    dialogueSystem.OnSystemPrompt_Clear();
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WA:
                    isWaitingOnAutoTimer = true;
                    yield return new WaitForSeconds(segment.signalDelay);
                    isWaitingOnAutoTimer = false;
                    break;
                default:
                    break;

            }
        }

        IEnumerator BuildDialogue(string dialogue, bool append)
        {
            dialogue = TagManager.Inject(dialogue);//inject tags into the dialogue

            //build the dialogue
            if (!append)
                architect.Build(dialogue);
            else
                architect.Append(dialogue);

            //wait for the dialogue to complete
            while (architect.isBuilding)
                {
                    if (userPrompt)
                    {
                        if (architect.hurryUp)
                        {
                            architect.hurryUp = true;
                        }
                        else
                        {
                            architect.ForceComplete();
                        }

                        userPrompt = false;
                    }
                    yield return null;
                }
        }

        IEnumerator WaitForUserInput()
        {
            dialogueSystem.prompt.Show();

            while (!userPrompt)
                yield return null;

            dialogueSystem.prompt.Hide();

            userPrompt = false;
        }
    }
}
