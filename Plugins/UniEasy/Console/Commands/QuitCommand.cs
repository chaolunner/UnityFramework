using UnityEngine;

namespace UniEasy.Console
{
    public class QuitCommand
    {
        public static readonly string Name = "Quit";
        public static readonly string Description = "Quit the application.";
        public static readonly string Usage = "Quit";

        [Command("Quit", "Quit the application.", "Quit")]
        public static string Execute(params string[] args)
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            return "Quitting...";
        }
    }
}
