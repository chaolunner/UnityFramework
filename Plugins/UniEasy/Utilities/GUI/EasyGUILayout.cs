using UnityEditor;
using UnityEngine;

namespace UniEasy
{
    public partial class EasyGUILayout
    {
        private static Color DarkGray = new Color(0.4f, .4f, .4f);
        private static Color LightGray = new Color(.9f, .9f, .9f);

        public static int Tabs(int selected, ref Vector2 scrollPosition, params string[] options)
        {
            var color = GUI.backgroundColor;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.padding.bottom = 8;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(35));
            GUILayout.BeginHorizontal();
            {
                for (int i = 0; i < options.Length; ++i)
                {
                    GUI.backgroundColor = i == selected ? LightGray : DarkGray;
                    if (GUILayout.Button(options[i], buttonStyle))
                    {
                        selected = i;
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            GUI.backgroundColor = color;

            return selected;
        }

        public static bool Foldout(bool foldout, GUIContent content, int width, out Rect rect)
        {
            var foldoutRect = new Rect(EditorGUILayout.GetControlRect());
            foldoutRect.xMax = foldoutRect.xMin + width;
            rect = new Rect(EditorGUILayout.GetControlRect());
            rect.xMin = foldoutRect.xMax;

            return EditorGUI.Foldout(foldoutRect, foldout, content);
        }
    }
}
