
using System;
using UnityEngine;

public class TestRectangle : MonoBehaviour
{
    [Header("Point")] public Obstacle PointSet;
    [Header("Rectangle")]
    public Transform Pivot;
    public float X;
    public float Y;
    public Transform Point1;
    public Transform Point2;
    public Transform Point3;
    public Transform Point4;
    [Space(10)]
    public float CheckWeight;
    public Transform PointCheck1;
    public Transform PointCheck2;
    public Transform PointCheck3;
    public Transform PointCheck4;
    [Header("Color")] 
    public Color ColorRectangleDefault;
    public Color ColorRectangleCollision;
    public Color ColorRectangle2Default;
    public Color ColorRectangle2Collision;
    [Header("Calculator")] 
    public float DistanceToRectangle;
    public float DistanceToRectangleCheck;
    public float ThrustPower;
    public float AngleThrust;
    public bool Convert;
    private Color _color;
    private Color _colorCheck;
    private Vector3 _pointForce;

    private void OnValidate()
    {
        Point1.localPosition = new Vector3(-X, 0, -Y);
        Point2.localPosition =  new Vector3(-X, 0, Y);
        Point3.localPosition = new Vector3(X, 0, Y);
        Point4.localPosition = new Vector3(X, 0, -Y);
        
        PointCheck1.localPosition = new Vector3(-X - CheckWeight, 0, -Y - CheckWeight);
        PointCheck2.localPosition =  new Vector3(-X - CheckWeight, 0, Y + CheckWeight);
        PointCheck3.localPosition = new Vector3(X + CheckWeight, 0, Y + CheckWeight);
        PointCheck4.localPosition = new Vector3(X + CheckWeight ,0, -Y - CheckWeight);
    }

    private void Update()
    {
        Calculator();
        DrawPoly();
        SetPosPoint();
    }

    private void SetPosPoint()
    {
        Vector3 posCircle = Pivot.InverseTransformPoint(PointSet.transform.position);
        float x = Mathf.Clamp(posCircle.x, -X, X);
        float y = Mathf.Clamp(posCircle.z, -Y, Y);
        _pointForce = new Vector3(x, 0, y);
        _pointForce = Pivot.TransformPoint(_pointForce);
    }

    private void Calculator()
    {
        //------------------
        DistanceToRectangleCheck = GetDistanceToRectangle(Pivot, X + CheckWeight, Y + CheckWeight, PointSet.transform);
        //-------------------
        DistanceToRectangle = GetDistanceToRectangle(Pivot, X, Y , PointSet.transform);
        //-------------------
        ThrustPower = Mathf.Clamp(1 -((DistanceToRectangle - PointSet.Radius)/CheckWeight), 0f, 1f);
        AngleThrust = 90.0f * ThrustPower;
    }

    private void DrawPoly()
    {
        _colorCheck = (DistanceToRectangleCheck > PointSet.Radius) ? ColorRectangle2Default : ColorRectangle2Collision;
        _color = (DistanceToRectangle > PointSet.Radius) ? ColorRectangleDefault : ColorRectangleCollision;
        Debug.DrawLine(Point1.position,Point2.position,_color);
        Debug.DrawLine(Point2.position,Point3.position,_color);
        Debug.DrawLine(Point3.position,Point4.position,_color);
        Debug.DrawLine(Point4.position,Point1.position,_color);
        //-------------------
        Debug.DrawLine(PointCheck1.position,PointCheck2.position,_colorCheck);
        Debug.DrawLine(PointCheck2.position,PointCheck3.position,_colorCheck);
        Debug.DrawLine(PointCheck3.position,PointCheck4.position,_colorCheck);
        Debug.DrawLine(PointCheck4.position,PointCheck1.position,_colorCheck);
        //-------------------
        Debug.DrawLine(PointSet.transform.position,_pointForce,Color.black);
        //force
        Vector3 vt = _pointForce - PointSet.transform.position;
        vt = Quaternion.Euler(0, AngleThrust, 0) * vt;
        DrawRay(PointSet.transform.position,vt.normalized * 2f,Color.magenta,arrow:true);
    }

    private bool Check(Transform rectangle,float x,float y, Transform point)
    {
        Vector3 absPoint = rectangle.InverseTransformPoint(point.position);
        absPoint = new Vector3(Mathf.Abs(absPoint.x), Mathf.Abs(absPoint.y), Mathf.Abs(absPoint.z));
        float d = Mathf.Sqrt(Mathf.Pow(Mathf.Max(absPoint.x - x,0), 2) + Mathf.Pow(Mathf.Max(absPoint.z - y,0), 2));
        return d > 0;
    }
    
    private bool Check(Transform rectangle,float x,float y, Transform point,float radius)
    {
        Vector3 absPoint = rectangle.InverseTransformPoint(point.position);
        absPoint = new Vector3(Mathf.Abs(absPoint.x), Mathf.Abs(absPoint.y), Mathf.Abs(absPoint.z));
        float d = Mathf.Sqrt(Mathf.Pow(Mathf.Max(absPoint.x - x,0), 2) + Mathf.Pow(Mathf.Max(absPoint.z - y,0), 2));
        return d > radius;
    }

    private float GetDistanceToRectangle(Transform rectangle, float x, float y, Transform point)
    {
        Vector3 absPoint = rectangle.InverseTransformPoint(point.position);
        absPoint = new Vector3(Mathf.Abs(absPoint.x), Mathf.Abs(absPoint.y), Mathf.Abs(absPoint.z));
        return Mathf.Sqrt(Mathf.Pow(Mathf.Max(absPoint.x - x,0), 2) + Mathf.Pow(Mathf.Max(absPoint.z - y,0), 2));
    }
    
    private float GetDistanceToRectangle(Transform rectangle, float x, float y, Transform point,out Vector3 pointForce)
    {
        Vector3 pointInvertToLocalRectangle = rectangle.InverseTransformPoint(point.position);
        pointForce = Pivot.TransformPoint(new Vector3(Mathf.Clamp(pointInvertToLocalRectangle.x, -X, X), 0, Mathf.Clamp(pointInvertToLocalRectangle.z, -Y, Y)));
        //----------------
        Vector3 absPoint = new Vector3(Mathf.Abs(pointInvertToLocalRectangle.x), Mathf.Abs(pointInvertToLocalRectangle.y), Mathf.Abs(pointInvertToLocalRectangle.z));
        return Mathf.Sqrt(Mathf.Pow(Mathf.Max(absPoint.x - x,0), 2) + Mathf.Pow(Mathf.Max(absPoint.z - y,0), 2));
    }
    
    private void DrawRay(Vector3 startPos,Vector3 direct,Color color,float length = 1,bool arrow = false)
    {
        Debug.DrawRay(startPos,direct * length,color);
        if (arrow)
        {
            Vector3 dir1 = (Quaternion.Euler(0,45,0) * -direct).normalized * .5f;
            Vector3 dir2 = (Quaternion.Euler(0,-45,0) * -direct).normalized * .5f;
            Vector3 startPos2 = startPos + direct;
            DrawRay(startPos2,dir1,color);
            DrawRay(startPos2,dir2,color);
        }
    }
}
