using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class MathJob
{
    #region Position

    /// <summary>
    /// Chuyển tọa độ điểm từ hệ tọa độ thế giới về hệ tọa độ của transform
    /// </summary>
    /// <param name="transformPosition"> vị trí của transform </param>
    /// <param name="point"> vị trí của điểm trong thế giới </param>
    /// <param name="forward"> hướng trước của transform </param>
    /// <param name="right"> hướng bên phải của transform </param>
    /// <param name="up"> hướng bên trên của transform </param>
    /// <returns></returns>
    public static float3 InvertTransformPoint(float3 transformPosition, float3 point, float3 forward, float3 right,
        float3 up)
    {
        var vector = point - transformPosition;

        var x = math.dot(vector, right);
        var y = math.dot(vector, up);
        var z = math.dot(vector, forward);

        return new float3(x, y, z);
    }

    /// <summary>
    /// Chuyển tọa độ điểm từ hệ tọa độ của transform về hệ tọa độ thế giới
    /// </summary>
    /// <param name="transformPosition"> vị trí của transform </param>
    /// <param name="point"> vị trí của điểm trong thế giới </param>
    /// <param name="forward"> hướng trước của transform </param>
    /// <param name="right"> hướng bên phải của transform </param>
    /// <param name="up"> hướng bên trên của transform </param>
    /// <returns></returns>
    public static float3 TransformPoint(float3 transformPosition, float3 point, float3 forward, float3 right, float3 up)
    {
        return transformPosition + right * point.x + up * point.y + forward * point.z;
    }

    /// <summary>
    /// Hình chiếu điểm lên mặt phẳng
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="planeNormal"></param>
    /// <returns></returns>
    public static float3 ProjectOnPlane(float3 vector, float3 planeNormal)
    {
        return vector - math.dot(vector, planeNormal) * planeNormal;
    }

    /// <summary>
    /// Khoảng cách từ điểm đến đường thẳng
    /// </summary>
    /// <param name="point"></param>
    /// <param name="linePoint"></param>
    /// <param name="lineDirection"></param>
    /// <returns></returns>
    public static float DistanceToLine(float3 point, float3 linePoint, float3 lineDirection)
    {
        return math.length(math.cross(point - linePoint, lineDirection));
    }

    /// <summary>
    /// Vị trí hình chiếu của điểm trên đường thẳng
    /// </summary>
    /// <param name="point"></param>
    /// <param name="linePoint"></param>
    /// <param name="lineDirection"></param>
    /// <returns></returns>
    public static float3 ClosestPointOnLine(float3 point, float3 linePoint, float3 lineDirection)
    {
        return linePoint + lineDirection * math.dot(point - linePoint, lineDirection);
    }

    #endregion


    #region Rotation

    /// <summary>
    /// Xoay điểm quanh pivot theo các góc euler
    /// </summary>
    /// <param name="point"></param>
    /// <param name="pivot"></param>
    /// <param name="angles">góc euler tính bằng radian</param>
    /// <returns></returns>
    public static float3 RotatePointAroundPivot(float3 point, float3 pivot, float3 angles)
    {
        var dir = point - pivot;
        dir = math.mul(quaternion.Euler(angles), dir);
        return dir + pivot;
    }

    /// <summary>
    /// Quaternion tạo bởi forward và up
    /// </summary>
    /// <param name="forward"></param>
    /// <param name="up"></param>
    /// <returns></returns>
    public static quaternion LookRotation(float3 forward, float3 up)
    {
        var right = math.cross(up, forward);
        up = math.cross(forward, right);
        return quaternion.LookRotation(forward, up);
    }

    /// <summary>
    /// Quaternion tạo bởi forward
    /// </summary>
    /// <param name="forward"></param>
    /// <returns></returns>
    public static quaternion LookRotation(float3 forward)
    {
        return quaternion.LookRotation(forward, new float3(0, 1, 0));
    }

    /// <summary>
    /// Xoay điểm theo các góc euler
    /// </summary>
    /// <param name="point"></param>
    /// <param name="angles"> góc euler tính theo độ hoặc radian </param>
    /// <param name="useDegree"> useDegree = true thì angles được tính theo độ, false thì tính theo radian </param>
    /// <returns></returns>
    public static float3 Rotate(float3 point, float3 angles, bool useDegree = true)
    {
        if (useDegree)
        {
            angles = math.radians(angles);
        }
        
        return math.mul(quaternion.Euler(angles), point);
    }

    /// <summary>
    /// Xoay điểm theo quaternion
    /// </summary>
    /// <param name="point"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public static float3 Rotate(float3 point, quaternion rotation)
    {
        return math.mul(rotation, point);
    }

    /// <summary>
    /// Xoay điểm theo trục và góc
    /// </summary>
    /// <param name="point"></param>
    /// <param name="axis"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static float3 Rotate(float3 point, float3 axis, float angle,bool useDegree = true)
    {
        if (useDegree)
        {
            angle = math.radians(angle);
        }
        return math.mul(quaternion.AxisAngle(axis, angle), point);
    }
    
    public static float DegreeToRadians(float degree)
    {
        return degree * math.PI / 180f;
    }

    public static float RadiansToDegree(float radians)
    {
        return radians * 180f / math.PI;
    }

    public static float Angle(float3 a, float3 b)
    {
        return RadiansToDegree(math.acos(math.dot(a, math.normalize(b)) / math.length(a)));
    }

    #endregion
}