using UnityEngine;

namespace UniEasy.Editor
{
    public struct RuntimePropertyGUIData
    {
        public RuntimeSerializedProperty Property;
        public Rect TotalPosition;
        public bool WasBoldDefaultFont;
        public bool WasEnabled;
        public Color Color;

        public RuntimePropertyGUIData(RuntimeSerializedProperty property, Rect totalPosition, bool wasBoldDefaultFont, bool wasEnabled, Color color)
        {
            Property = property;
            TotalPosition = totalPosition;
            WasBoldDefaultFont = wasBoldDefaultFont;
            WasEnabled = wasEnabled;
            Color = color;
        }
    }
}
