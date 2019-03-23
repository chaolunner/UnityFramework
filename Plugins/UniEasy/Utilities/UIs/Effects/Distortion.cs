using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

namespace UniEasy
{
    [AddComponentMenu("UI/Effects/Distortion")]
    public class Distortion : BaseMeshEffect
    {
        public AnimationCurve Top = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
        public AnimationCurve Bottom = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
        public float Scale = 1f;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || Top == null || Top.length < 2 || Bottom == null || Bottom.length < 2)
            {
                return;
            }

            var count = vh.currentVertCount;
            if (count == 0)
                return;

            var vertexs = new List<UIVertex>();
            for (int i = 0; i < count; i++)
            {
                var vertex = new UIVertex();
                vh.PopulateUIVertex(ref vertex, i);
                vertexs.Add(vertex);
            }

            var topLastTime = Top.keys.Last().time;
            var bottomLastTime = Bottom.keys.Last().time;
            var length = (int)(0.25f * vertexs.Count);
            var average = 0.5f / length;
            for (int i = 0; i < length; i++)
            {
                var time = (2 * i + 1) * average;
                for (int j = 0; j < 4; j++)
                {
                    var vertex = vertexs[4 * i + j];
                    var topEvaluate = topLastTime >= time ? Top.Evaluate(time) : Bottom.Evaluate(time);
                    var bottomEvaluate = bottomLastTime >= time ? Bottom.Evaluate(time) : Top.Evaluate(time);
                    vertex.position += Scale * (j > 1 ? bottomEvaluate : topEvaluate) * transform.up;
                    vh.SetUIVertex(vertex, 4 * i + j);
                }
            }
        }
    }
}
