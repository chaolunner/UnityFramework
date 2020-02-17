using UnityEngine.UI;
using UnityEngine;

namespace UniEasy.Console
{
    public class LogElement : MonoBehaviour, IFastScrollElement<LogData>
    {
        public Image MessageBackground;
        public Text MessageText;
        public Image CountBackground;
        public Text CountText;

        private LogData logData;

        private const int Size = 14;
        private const string LogStyle = "<b><size={0}><color=#ffffffff>{1}</color></size></b>";
        private const string WarningStyle = "<b><size={0}><color=#ffff00ff>{1}</color></size></b>";
        private const string ErrorStyle = "<b><size={0}><color=#ff0000ff>{1}</color></size></b>";

        private static readonly Color32 MsgBgColorLight = new Color32(0x00, 0x00, 0x00, 0x24);
        private static readonly Color32 CountBgColorLight = new Color32(0x42, 0x42, 0x42, 0x80);
        private static readonly Color32 MsgBgColorDark = new Color32(0x00, 0x00, 0x00, 0x48);
        private static readonly Color32 CountBgColorDark = new Color32(0xA9, 0xA9, 0xA9, 0x80);

        public LogData GetData()
        {
            return logData;
        }

        public void Scroll(int index, LogData data, float size, int constraintCount, bool visible)
        {
            var rectTransform = transform as RectTransform;

            rectTransform.anchoredPosition = new Vector2(0, -(index / constraintCount) * size);
            float min = index % constraintCount;
            float max = (index + 1) % constraintCount;
            if (max < min) { max = constraintCount; }
            rectTransform.anchorMin = new Vector2(min / constraintCount, 1);
            rectTransform.anchorMax = new Vector2(max / constraintCount, 1);

            if (visible)
            {
                if (index % 2 == 0)
                {
                    MessageBackground.color = MsgBgColorLight;
                    CountBackground.color = CountBgColorLight;
                }
                else
                {
                    MessageBackground.color = MsgBgColorDark;
                    CountBackground.color = CountBgColorDark;
                }

                logData = data;

                if (data.LogType == LogType.Warning)
                {
                    MessageText.text = string.Format(WarningStyle, Size, data.Message);
                }
                else if (data.LogType == LogType.Error)
                {
                    MessageText.text = string.Format(ErrorStyle, Size, data.Message);
                }
                else
                {
                    MessageText.text = string.Format(LogStyle, Size, data.Message);
                }
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
