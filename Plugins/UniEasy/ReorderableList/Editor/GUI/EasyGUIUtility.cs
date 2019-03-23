namespace UniEasy.Editor
{
    public class EasyGUIUtility
    {
        #region Static Methods

        public static void ResetGUIState()
        {
            EasyGUI.ClearStacks();
            EditorGUIUtilityHelper.SetBoldDefaultFont(false);
            ScriptAttributeUtility.PropertyHandlerCache = null;
            InspectableAttributeUtility.InspectableHandlerCache = null;
        }

        #endregion
    }
}
