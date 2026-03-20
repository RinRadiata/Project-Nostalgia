#if UNITY_EDITOR
using UnityEngine;

namespace TESTING
{
    public class TestCensor : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Check("this line has a badword1 in it?");
            Check("this should be clear of any bad words!");
            Check("This astinking line should be bad as well.");
            Check("I want some fried tofu with chili-sauce!. And make it extratofu pls.");
        }

        void Check(string line)
        {
            if (CensorManager.Censor(ref line))
                Debug.Log($"<color=red>'{line}'");
            else
                Debug.Log($"<color=green>'{line}'");

        }
    }
}
#endif