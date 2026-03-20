using System;

namespace COMMANDS
{
    public class CMD_DatabaseExtension_VisualNovel : CMD_DatabaseExtension
    {
        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("SetPlayerName", new Action<string>(SetPlayerNameVariable));
        }

        private static void SetPlayerNameVariable(string data)
        {
            VISUALNOVEL.VNGameSave.activeFile.playerName = data;
        }
    }
}