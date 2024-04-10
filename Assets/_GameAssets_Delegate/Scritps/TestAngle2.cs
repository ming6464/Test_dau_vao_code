using System;
using Unity.Mathematics;
using UnityEngine;

public class TestAngle2 : MonoBehaviour
{

    public Transform t1;
    public Transform t2;
    public float AngleDefault;
    public float AngleNew;

    private void Update()
    {
        Vector3 vt1 = t1.position - transform.position;
        Vector3 vt2 = t2.position - transform.position;
        Debug.DrawLine(transform.position,t1.position,Color.red);
        Debug.DrawLine(transform.position,t2.position,Color.red);
        AngleDefault = Angle180AxisXClockwise(vt1, vt2);
        AngleNew = Angle180Clockwise(vt1, vt2);
    }

    private float Angle180AxisXClockwise(Vector3 start,Vector3 dir)
    {
        float angle = Vector2.SignedAngle(new Vector2(start.x, start.z), new Vector2(dir.x, dir.z));
        return angle;
    }
    
    float Angle180Clockwise(float3 start, float3 dir)
    {
        float2 vectorA = new float2(start.x, start.z);
        float2 vectorB = new float2(dir.x, dir.z);
        float angle = math.degrees(math.atan2(vectorB.y, vectorB.x) - math.atan2(vectorA.y, vectorA.x));
        float cross = vectorA.x * vectorB.y - vectorA.y * vectorB.x;
        float sign = math.sign(cross);
        angle = math.fmod(angle, 180) - 180;
        angle *= sign;
        if (angle < 0)
        {
            angle += 180f;
        }
        else
        {
            angle = 180 - angle;
        }
        return angle;
    }
    
    private float Angle180AxisXClockwise1(float3 start, float3 dir)
    {
        // Tính vector hướng của đường thẳng từ start đến dir
        float2 lineDir = math.normalize(new float2(dir.x - start.x, dir.z - start.z));

        // Sử dụng atan2 để tính góc giữa vector hướng đó và trục X
        float angle = math.degrees(math.atan2(lineDir.y, lineDir.x));

        // Đảm bảo góc nằm trong khoảng từ -180 đến 180 độ
        angle = math.fmod(angle + 180f, 360f) - 180f;

        return angle;
    }
    
    private float Angle180AxisXClockwise4(float3 start, float3 dir)
    {
        float angle = math.degrees(math.atan2(dir.z - start.z, dir.x - start.x));
        return angle;
    }
}