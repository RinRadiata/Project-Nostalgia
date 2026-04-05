using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameSceneManager : MonoBehaviour
{
    public static MinigameSceneManager instance;

    void Awake()
    {
        instance = this;
    }

    public void FinishMinigame(bool perfect)
    {
        VariableStore.TryGetValue("Minigame.currentCharacter", out object charObj);
        VariableStore.TryGetValue("Minigame.currentID", out object idObj);
        VariableStore.TryGetValue("Minigame.returnScene", out object returnSceneObj);

        string characterID = (string)charObj;
        string minigameID = (string)idObj;
        string returnScene = (string)returnSceneObj;

        //save flags
        VariableStore.TrySetValue($"{characterID}.minigame.{minigameID}.completed", true);
        VariableStore.TrySetValue($"{characterID}.minigame.{minigameID}.perfect", perfect);

        if (perfect)
            AffectionSystem.AddAffection(characterID, 10);
        else
            AffectionSystem.AddAffection(characterID, 5);

        VariableStore.TrySetValue($"{characterID}.diary.{minigameID}.unlocked", true);

        SceneManager.LoadScene(returnScene); //return to current active dialogue scene
    }

    public void FailMinigame()
    {
        VariableStore.TryGetValue("Minigame.returnScene", out object returnSceneObj);
        string returnScene = (string)returnSceneObj;

        SceneManager.LoadScene(returnScene);
    }
}