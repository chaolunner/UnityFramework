using UnityEngine;

namespace UniEasy
{
    public class BackgroundColorAttribute : PropertyAttribute
    {
        public Color Color { get; protected set; }

        public BackgroundColorAttribute(Color color)
        {
            Color = color;
        }

        public BackgroundColorAttribute(Color32 color)
        {
            Color = color;
        }

        public BackgroundColorAttribute(string htmlString)
        {
            Color color = Color;
            if (ColorUtility.TryParseHtmlString(htmlString, out color))
            {
                Color = color;
            }
        }
    }
}
