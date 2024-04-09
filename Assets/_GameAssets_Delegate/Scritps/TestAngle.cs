using System;
using UnityEngine;

public class TestAngle : MonoBehaviour
{

    public Transform target;
    public Obstacle circle1;
    public Obstacle circle2;

    [Space(10)] 
    public bool IsColliderCircle2;
    public bool IsCircle1ColliderCircle2;

    public float Angle_circle1circle2_circle1target;
    
    private void Update()
    {
        Debug.DrawLine(circle1.transform.position,target.position,Color.blue);
        Debug.DrawLine(circle1.transform.position,circle2.transform.position,Color.blue);

        Vector3 circle1Circle2 = circle2.transform.position - circle1.transform.position;
        Vector3 circle1Target = target.position - circle1.transform.position;

        Angle_circle1circle2_circle1target = Angle180AxisXClockwise(circle1Circle2, circle1Target);
        IsColliderCircle2 = IsLineIntersectCircle(circle2.transform.position, circle2.Radius,
            circle1.transform.position, target.position); 
        IsCircle1ColliderCircle2 = IsLineIntersectCircle(circle2.transform.position, circle2.Radius + circle1.Radius,
            circle1.transform.position, target.position);
    }

    private bool IsLineIntersectCircle(Vector3 O, float R, Vector3 A, Vector3 B)
    {
        Vector2 point1 = new Vector2(A.x, A.z);
        Vector2 point2 = new Vector2(B.x, B.z);
        Vector2 point3 = new Vector2(O.x, O.z);

        float d = Mathf.Abs(
            ((point2.x - point1.x) * (point1.y - point3.y) - (point1.x - point3.x) * (point2.y - point1.y)) /
            (Vector2.Distance(point1, point2)));

        if (d <= R)
        {
            return true;
        }

        return false;
    }

    private float Angle360AxisXClockwise(Vector3 start,Vector3 dir)
    {
        float angle = Angle180AxisXClockwise(start, dir);
        if (angle < 0)
        {
            angle += 360;
        }
        return angle;
    }
    
    private float Angle180AxisXClockwise(Vector3 start,Vector3 dir)
    {
        float angle = Vector2.SignedAngle(new Vector2(start.x, start.z), new Vector2(dir.x, dir.z));
        return angle;
    }
    
    bool CalculateCircleIntersection(Vector3 center1, float radius1, Vector3 center2, float radius2, out Vector3 intersection1, out Vector3 intersection2)
    {
        float distance = Vector3.Distance(center1, center2);
        if (distance > radius1 + radius2 || distance < Mathf.Abs(radius1 - radius2))
        {
            intersection1 = Vector3.zero;
            intersection2 = Vector3.zero;
            return false;
        }

        float a = (radius1 * radius1 - radius2 * radius2 + distance * distance) / (2 * distance);
        float h = Mathf.Sqrt(radius1 * radius1 - a * a);
        Vector3 p2 = center1 + (center2 - center1) * (a / distance);
        intersection1 = new Vector3(p2.x + h * (center2.z - center1.z) / distance,0, p2.z - h * (center2.x - center1.x) / distance);
        intersection2 = new Vector3(p2.x - h * (center2.z - center1.z) / distance,0, p2.z + h * (center2.x - center1.x) / distance);
        return true;
    }

}