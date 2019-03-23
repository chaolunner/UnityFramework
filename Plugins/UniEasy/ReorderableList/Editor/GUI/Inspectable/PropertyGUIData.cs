using UnityEngine;

namespace UniEasy.Editor
{
    public struct PropertyGUIData
    {
        public InspectableProperty Property;
        public Rect TotalPosition;
        public bool WasBoldDefaultFont;
        public bool WasEnabled;
        public Color Color;

        public PropertyGUIData(InspectableProperty property, Rect totalPosition, bool wasBoldDefaultFont, bool wasEnabled, Color color)
        {
            Property = property;
            TotalPosition = totalPosition;
            WasBoldDefaultFont = wasBoldDefaultFont;
            WasEnabled = wasEnabled;
            Color = color;
        }
    }
}
