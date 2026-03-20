using UnityEngine;

[System.Serializable]
public class VN_Configuration
{
    public static VN_Configuration activeConfig;

    public const bool ENCRYPT = false;

    public static string filePath => $"{FilePaths.root}vnconfig.cfg";

    //general settings
    public bool display_fullscreen = true;
    public string display_resolution = "1920x1080";//Standard default resolution
    public bool continueSkippingAfterChoice = false;
    public float dialogueTextSpeed = 1f;
    public float dialogueAutoReadSpeed = 1f;

    //audio settings
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public float voicesVolume = 1f;
    public bool musicMute = false;
    public bool sfxMute = false;
    public bool voicesMute = false;

    //Other settings
    public float historyLogScale = 1f;

    public void Load()
    {
        var ui = ConfigMenu.instance.ui;

        //apply general settings
        //set window size
        ConfigMenu.instance.SetDisplayToFullScreen(display_fullscreen);
        ui.SetButtonColors(ui.fullscreen, ui.windowed, display_fullscreen);

        //set screen resolution
        int res_index = 0;
        for (int i = 0; i < ui.resolutions.options.Count; i++)
        {
            string resolution = ui.resolutions.options[i].text;
            if (resolution == display_resolution)
            {
                res_index = i;
                break;
            }
        }
        ui.resolutions.value = res_index;

        //set continue skipping after skipping choice
        ui.SetButtonColors(ui.skippingContinue, ui.skippingStop, continueSkippingAfterChoice);

        //set text architect speed and auto read speed
        ui.architectSpeed.value = dialogueTextSpeed;
        ui.autoReaderSpeed.value = dialogueAutoReadSpeed;

        //set audio mixer volumes
        ui.musicVolume.value = musicVolume;
        ui.sfxVolume.value = sfxVolume;
        ui.voicesVolume.value = voicesVolume;
        ui.musicMute.sprite = musicMute ? ui.mutedSymbol : ui.unmutedSymbol;
        ui.sfxMute.sprite = sfxMute ? ui.mutedSymbol : ui.unmutedSymbol;
        ui.voicesMute.sprite = voicesMute ? ui.mutedSymbol : ui.unmutedSymbol;

    }

    public void Save()
    {
        FileManager.Save(filePath, JsonUtility.ToJson(this), encrypt: ENCRYPT);
    }
}
