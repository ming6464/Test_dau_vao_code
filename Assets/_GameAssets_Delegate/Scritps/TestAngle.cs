using System;
using UnityEngine;

public class TestAngle : MonoBehaviour
{
    public float radiusA;   // Bán kính của hình tròn A
    public float radiusB;   // Bán kính của hình tròn B
    public Obstacle Circle1;
    public Obstacle Circle2;
    public Color color1;
    public Color color2;

    public Obstacle Circle;
    public Transform target1;
    public Transform target2;
    public int Point;
    
    void Update()
    {
        Vector3 centerA = Circle1.transform.position;
        Vector3 centerB = Circle2.transform.position;
        radiusA = Circle1.Radius;
        radiusB = Circle2.Radius;
        if (Vector3.Distance(centerA, centerB) < (radiusA + radiusB))
        {
        
            Vector3 pointC = Vector3.zero;
            Vector3 pointD = Vector3.zero;
            CalculateCircleIntersection(centerA, radiusA, centerB, radiusB, out pointC, out pointD);
            Debug.DrawLine(pointC,centerB,Color.blue);
            Debug.DrawLine(pointD,centerB,Color.blue);
            Debug.DrawLine(pointC,pointD,Color.blue);
            Debug.DrawLine(centerA,centerB,Color.blue);
            
            Vector3 AToB = Circle2.transform.position - Circle1.transform.position;
            Vector3 AToC = pointC - Circle1.transform.position;
            Vector3 AToD = pointD - Circle1.transform.position;
        
            if (Angle180AxisXClockwise(AToB, AToD) > 0)
            {
                Debug.DrawLine(centerA,pointC,color2);
                Debug.DrawLine(centerA,pointD,color1);
            }
            else
            {
                Debug.DrawLine(centerA, pointC, color1);
                Debug.DrawLine(centerA,pointD,color2);
            }
           
        }

        Vector2 point1 = new Vector2(target1.position.x,target1.position.z);
        Vector2 point2 = new Vector2(target2.position.x,target2.position.z);
        Vector2 point3 = new Vector2(Circle.transform.position.x,Circle.transform.position.z);
        
        Debug.DrawLine(target1.position,target2.position, Color.magenta);
        
        Debug.Log($"Đường thẳng {IsLineIntersectCircle(Circle.transform.position,Circle.Radius,target2.position,target1.position)} Cắt");
        
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