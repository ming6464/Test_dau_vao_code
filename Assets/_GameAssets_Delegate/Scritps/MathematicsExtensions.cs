using Unity.Mathematics;

public static class MathematicsExtensions
{
    public static float3 Rotation(this quaternion quaternion, float3 point) =>
        Rotation(quaternion, point.x, point.y, point.z);

    public static float3 Rotation(this quaternion quaternion, float x, float y, float z)
    {
        float4 rotation = quaternion.value;
        float num1 = rotation.x * 2f;
        float num2 = rotation.y * 2f;
        float num3 = rotation.z * 2f;
        float num4 = rotation.x * num1;
        float num5 = rotation.y * num2;
        float num6 = rotation.z * num3;
        float num7 = rotation.x * num2;
        float num8 = rotation.x * num3;
        float num9 = rotation.y * num3;
        float num10 = rotation.w * num1;
        float num11 = rotation.w * num2;
        float num12 = rotation.w * num3;
        float3 vector3;
        vector3.x = (float)((1.0 - ((double)num5 + (double)num6)) * (double)x +
                            ((double)num7 - (double)num12) * (double)y + ((double)num8 + (double)num11) * (double)z);
        vector3.y = (float)(((double)num7 + (double)num12) * (double)x +
                            (1.0 - ((double)num4 + (double)num6)) * (double)y +
                            ((double)num9 - (double)num10) * (double)z);
        vector3.z = (float)(((double)num8 - (double)num11) * (double)x + ((double)num9 + (double)num10) * (double)y +
                            (1.0 - ((double)num4 + (double)num5)) * (double)z);
        return vector3;
    }
}