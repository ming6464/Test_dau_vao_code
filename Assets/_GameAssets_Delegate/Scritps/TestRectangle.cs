
using System;
using UnityEngine;

public class TestRectangle : MonoBehaviour
{
    [Header("Point")] public Transform PointSet;
    [Header("Rectangle")]
    public Transform Pivot;
    public float X;
    public float Y;
    public Transform Point1;
    public Transform Point2;
    public Transform Point3;
    public Transform Point4;

    private void OnValidate()
    {
        Point1.localPosition = new Vector3(-X, 0, -Y);
        Point2.localPosition =  new Vector3(-X, 0, Y);
        Point3.localPosition = new Vector3(X, 0, Y);
        Point4.localPosition = new Vector3(X, 0, -Y);
    }

    private void Update()
    {
        Debug.DrawLine(Point1.position,Point2.position,Color.blue);
        Debug.DrawLine(Point2.position,Point3.position,Color.blue);
        Debug.DrawLine(Point3.position,Point4.position,Color.blue);
        Debug.DrawLine(Point4.position,Point1.position,Color.blue);
        Debug.Log(check(Pivot,X,Y,PointSet));
        Debug.DrawLine(Pivot.position,PointSet.position,Color.green);
    }

    private bool check(Transform rectangle,float x,float y, Transform point)
    {
        Vector3 absPoint = rectangle.InverseTransformPoint(point.position);
        absPoint = new Vector3(Mathf.Abs(absPoint.x), Mathf.Abs(absPoint.y), Mathf.Abs(absPoint.z));
        float d = Mathf.Sqrt(Mathf.Pow(Mathf.Max(absPoint.x - x,0), 2) + Mathf.Pow(Mathf.Max(absPoint.z - y,0), 2));
        return d > 0;
    }
}
