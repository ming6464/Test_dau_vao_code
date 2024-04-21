using Unity.Mathematics;
using UnityEngine;

namespace GameAssets_Navmesh.Scripts
{
    public static class DrawShape
    {
        public static void DrawCircle(float3 center, float radius, float angle, float3 forward, Color color,
            float detail = 5)
        {
            try
            {
                if (forward.Equals(float3.zero)) return;
                if (angle > 360)
                {
                    angle %= 360;
                }

                Gizmos.color = color;
                angle = math.abs(angle);
                detail = math.max(detail, 5);
                float angleReal = angle / 2f;
                float3 vt0 = math.normalize(forward) * radius;
                float3 point1 = MathJob.Rotate(vt0, new float3(0, angleReal, 0)) + center;
                float3 point2 = MathJob.Rotate(vt0, new float3(0, -angleReal, 0)) + center;
                float3 point3 = default;
                float3 point4 = default;

                if (angle < 360)
                {
                    Gizmos.DrawLine(center, point1);
                    Gizmos.DrawLine(center, point2);
                }

                float add = angleReal / detail;
                bool check = false;
                for (float i = 1; i <= detail; i++)
                {
                    float angleReal2 = angleReal - i * add;
                    point3 = MathJob.Rotate(vt0, new float3(0, angleReal2, 0)) + center;
                    point4 = MathJob.Rotate(vt0, new float3(0, -angleReal2, 0)) + center;
                    Gizmos.DrawLine(point1, point3);
                    Gizmos.DrawLine(point2, point4);
                    point1 = point3;
                    point2 = point4;
                }
            }
            catch
            {
                //ignored
            }
        }
    }
}