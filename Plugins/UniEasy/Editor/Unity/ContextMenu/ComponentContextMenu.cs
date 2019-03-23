using UnityEditorInternal;
using UnityEngine;
using UnityEditor;

public class ComponentContextMenu
{
    private static Component[] Clipboard;

    [MenuItem("CONTEXT/Component/Copy All Components", false, 501)]
    private static void CopyAllComponents()
    {
        var selection = Selection.activeGameObject;
        if (selection != null)
        {
            Clipboard = selection.GetComponents<Component>();
        }
    }

    [MenuItem("CONTEXT/Component/Paste All Components", false, 502)]
    static void PasteAllComponents()
    {
        var selection = Selection.activeGameObject;
        if (selection != null)
        {
            for (int i = 0; i < Clipboard.Length; i++)
            {
                ComponentUtility.CopyComponent(Clipboard[i]);
                var component = selection.GetComponent(Clipboard[i].GetType());
                if (component != null)
                {
                    ComponentUtility.PasteComponentValues(component);
                }
                else
                {
                    ComponentUtility.PasteComponentAsNew(selection);
                }
            }
        }
    }

    [MenuItem("CONTEXT/Component/Paste All Components", true)]
    static bool CheckClipboardNotEmpty()
    {
        if (Clipboard != null && Clipboard.Length > 0)
        {
            return true;
        }
        return false;
    }
}
