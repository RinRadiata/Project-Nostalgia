using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DIALOGUE
{

    public class DL_DIALOGUE_DATA
    {
        public string rawData { get; private set; } = string.Empty;
        public List<DIALOGUE_SEGMENT> segments;
        private const string segmentIdentifierPattern = @"\{[ca]\}|\{w[ca]\s\d*\.?\d*\}"; // Regex pattern to match segments

        public DL_DIALOGUE_DATA(string rawDialogue)
        {
            this.rawData = rawDialogue;
            segments = RipSegments(rawDialogue);
        }

        public List<DIALOGUE_SEGMENT> RipSegments(string rawDialogue)
        {
            List<DIALOGUE_SEGMENT> segments = new List<DIALOGUE_SEGMENT>();
            MatchCollection matches = Regex.Matches(rawDialogue, segmentIdentifierPattern);

            int LastIndex = 0;
            //find the first and only segment in the file
            DIALOGUE_SEGMENT segment = new DIALOGUE_SEGMENT();
            segment.dialogue = (matches.Count == 0 ? rawDialogue : rawDialogue.Substring(0, matches[0].Index));
            segment.startSignal = DIALOGUE_SEGMENT.StartSignal.NONE;
            segment.signalDelay = 0;
            segments.Add(segment);

            if (matches.Count == 0)
                return segments;
            else
                LastIndex = matches[0].Index;

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                segment = new DIALOGUE_SEGMENT();

                // get the start signal for the segment
                string signalMatch = match.Value;// get the signal match {A}
                signalMatch = signalMatch.Substring(1, match.Length - 2); // remove the brackets
                string[] signalSplit = signalMatch.Split(' '); // split the signal into parts

                segment.startSignal = (DIALOGUE_SEGMENT.StartSignal)Enum.Parse(typeof(DIALOGUE_SEGMENT.StartSignal), signalSplit[0].ToUpper());

                // get the signal delay for the segment
                if (signalSplit.Length > 1)
                    float.TryParse(signalSplit[1], out segment.signalDelay);

                //get the dialogue for the segment
                int NextIndex = i + 1 < matches.Count ? matches[i + 1].Index : rawDialogue.Length;
                segment.dialogue = rawDialogue.Substring(LastIndex + match.Length, NextIndex - (LastIndex + match.Length));
                LastIndex = NextIndex;

                segments.Add(segment);
            }

            return segments;
        }


        public struct DIALOGUE_SEGMENT
        {
            public string dialogue;
            public StartSignal startSignal;
            public float signalDelay;

            public enum StartSignal { NONE, C, A, WA, WC } //c = clear, a = append, wa = wait append, wc = wait clear

            public bool appendText => (startSignal == StartSignal.A || startSignal == StartSignal.WA);
        }
    }
}