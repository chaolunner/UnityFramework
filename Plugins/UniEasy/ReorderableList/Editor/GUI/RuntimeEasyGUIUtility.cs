namespace UniEasy.Editor
{
    public class RuntimeEasyGUIUtility
    {
        #region Static Methods

        public static void ResetGUIState()
        {
            RuntimeEasyGUI.ClearStacks();
            EditorGUIUtilityHelper.SetBoldDefaultFont(false);
            RuntimeScriptAttributeUtility.PropertyHandlerCache = null;
        }

        #endregion
    }
}
