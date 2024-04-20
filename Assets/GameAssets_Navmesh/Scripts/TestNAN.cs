using System;
using Unity.Mathematics;
using UnityEngine;

public class TestNAN : MonoBehaviour
{
    public Transform point1;
    public Transform point2;

    public float angle1;
    public float angle2;


    private void Update()
    {
        Vector3 myPosToPoint1 = point1.position - transform.position;
        Vector3 myPosToPoint2 = point2.position - transform.position;
        angle1 = Angle180Clockwise(myPosToPoint1, myPosToPoint2);
        angle2 = MathJob.Angle(myPosToPoint1, myPosToPoint2);
    }
    
    private float Angle180Clockwise(Vector3 start, Vector3 dir)
    {
        return Vector2.SignedAngle(new Vector2(start.x, start.z), new Vector2(dir.x, dir.z));
    }
}