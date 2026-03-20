using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class CensorManager
{
    private static Dictionary<string, string> badWords = new Dictionary<string, string>()
    {
        { "badword1", "b[a@4]dw[o0]rd1" },
        { "stinking", "[s\\$]t[i1]nk[i1]ng" }
    };

    private static Dictionary<string, string> hardBlocks = new Dictionary<string, string>()
    {
        { "tofu", "t[o0]fu"}
    };

    public static bool Censor(ref string text)
    {
        bool isCensored = false;

        foreach (var pair in hardBlocks)
        {
            Regex regex = new Regex(pair.Value, RegexOptions.IgnoreCase);

            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, match => new string('*', match.Length));
                isCensored = true;
            }
        }

        foreach (var pair in badWords)
        {
            string parttern = $"(?<=\\W|^){pair.Value}(?=\\W|$)"; // Word boundary using lookbehind and lookahead (no word back & front)
            Regex regex = new Regex(parttern, RegexOptions.IgnoreCase);

            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, match => new string('*', match.Length));
                isCensored = true;
            }
        }

        return isCensored;
    }
}
