using UnityEngine;

namespace UniEasy
{
    public class EasyPainter : MonoBehaviour
    {
        [SerializeField] Texture2D canvas = null;
        [SerializeField] Texture2D brush = null;
        [SerializeField] Color color = new Color(1, 1, 1, 1);
        [SerializeField, Range(0f, 1f)] float scale = 1f;
        private EasyPaint paint;
        private RaycastHit hit;
        private Vector2 start, end;

        void Start()
        {
            paint = new EasyPaint(canvas, brush, color);
        }

        void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                start = Vector2.zero;
            }
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#if UNITY_EDITOR
                paint.Color = color;
                paint.BrushScale = scale;
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.green);
#endif
                if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.collider != null)
                    {
                        end = hit.textureCoord;
                        if (start == Vector2.zero)
                        {
                            start = end;
                        }
                        // here you can make a length limit to improve performance.
                        if (Vector2.Distance(start, end) > 0.1f)
                        {
                            start = end;
                        }
                        paint.Draw(start, end);
                        // here you can make a time interval to improve performance.
                        paint.Refresh();
                        start = end;
                    }
                }
            }
        }
    }
}
