using System.Collections;
using static DIALOGUE.LogicalLines.LogicalLineUtils.Encapsulation;
using static DIALOGUE.LogicalLines.LogicalLineUtils.Conditions;

namespace DIALOGUE.LogicalLines
{
    public class LL_Condition : ILogicalLine
    {
        public string keyword => "if";
        private const string ELSE = "else";
        private readonly string[] CONTAINERS = new string[] { "(", ")" };


        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            string rawCondition = ExtractCondition(line.rawData.Trim());
            bool conditionResult = EvaluateCondition(rawCondition);

            Conversation currentConversation = DialogueSystem.instance.conversationManager.conversation;
            int currentProgress = DialogueSystem.instance.conversationManager.conversationProgress;

            EncapsulatedData ifData = RipEncapsulationData(currentConversation, currentProgress, false, parentStartingIndex: currentConversation.fileStartIndex);
            EncapsulatedData elseData = new EncapsulatedData();

            if (ifData.endingIndex + 1 < currentConversation.Count)
            {
                string nextLine = currentConversation.GetLines()[ifData.endingIndex + 1].Trim();
                if (nextLine == ELSE)
                {
                    elseData = RipEncapsulationData(currentConversation, ifData.endingIndex + 1, false, parentStartingIndex: currentConversation.fileStartIndex);
                    ifData.endingIndex = elseData.endingIndex;
                }
            }

            //so that the choices in dialogue can identify only thier ending point from each choice
            //this also affect the save and load to navigate progress of the choice we made so that when we load saved file, it's only take the logs of that choice
            currentConversation.SetProgress(elseData.isNull ? ifData.endingIndex : elseData.endingIndex);

            EncapsulatedData selectedData = conditionResult ? ifData : elseData;

            if (!selectedData.isNull && selectedData.lines.Count > 0)
            {
                //remove the header and encapsulator lines from the conversation indexes
                selectedData.startingIndex += 2;//remove the header and encapsulator
                selectedData.endingIndex -= 1;//Remove ending Encapsulator

                Conversation newConversation = new Conversation(selectedData.lines, file: currentConversation.file, fileStartIndex: selectedData.startingIndex, fileEndIndex: selectedData.endingIndex);
                DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);
            }

            yield return null;
        }

        public bool Matches(DIALOGUE_LINE line)
        {
            return line.rawData.Trim().StartsWith(keyword);
        }

        private string ExtractCondition(string line)
        {
            int startIndex = line.IndexOf(CONTAINERS[0]) + 1;
            int endIndex = line.IndexOf(CONTAINERS[1]);

            return line.Substring(startIndex, endIndex - startIndex).Trim();
        }
    }
}

