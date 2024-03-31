
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BeizerScritps : MonoBehaviour
{
    public Transform _objRun;
    public bool RunButton;
    
    #region Method1
    [Space(10)]
    [Header("Method1")]
    public Transform[] PointTfs;
    public int countTime;
    public bool RunCalculator;
    public Transform CenterPoint;
    public Transform SymmetricalPoint;
    private List<Transform> listSymmetricalPoint;
    private List<Vector3> PointCalculator;
    #endregion
    #region Method2
    [Serializable]
    public struct PointInfo
    {
        public Transform Point;
        public bool IsRotaZ;
    }
    [Space(10)]
    [Header("Method2")]
    public float RotaZ;
    public float SpeedRota;
    public float Speed;
    public PointInfo[] PointInfos;
    private Quaternion _targetQuaternion;
    private Vector3 _previousPosition;
    #endregion
    private bool Run;
    private int index;

    private void Update()
    {
        if (RunButton)
        {
            RunButton = false;
            if(!_objRun) return;
            Run = true;
            index = 0;
            _previousPosition = _objRun.position;
        }
        Method1();
        Method2();

    }

    private void Method2()
    {
        if(!Run || PointInfos.Length == 0) return;

        if (Vector3.Distance(_objRun.position, PointInfos[index].Point.position) < 1f)
        {
            index++;
            if (index >= PointInfos.Length)
            {
                index = 0;
            }
        }

        Vector3 rota = Quaternion.LookRotation(PointInfos[index].Point.position - _objRun.position).eulerAngles;
        if (PointInfos[index].IsRotaZ)
        {
            rota.z = RotaZ;
        }
        _targetQuaternion = Quaternion.Euler(rota);
        _objRun.rotation = Quaternion.Lerp(_objRun.rotation,_targetQuaternion,SpeedRota * Time.deltaTime);
        _objRun.Translate(Vector3.forward * Speed * Time.deltaTime);
        Debug.DrawLine(_previousPosition,_objRun.position,Color.blue,Mathf.Infinity);
        _previousPosition = _objRun.position;
    }

    private void Method1()
    {
        return;
        if (RunCalculator)
        {
            PointCalculator = new List<Vector3>();
            RunCalculator = false;
            LerpList();
        }
        DrawLine();
        
        if (Run)
        {
            if (index >= PointCalculator.Count)
            {
                Run = false;
                return;
            }

            _objRun.position = PointCalculator[index];
            
            index++;
        }
    }

    private void DrawLine()
    {
        if(PointCalculator == null) return;
        for (int i = 1; i < PointCalculator.Count; i++)
        {
            Debug.DrawLine(PointCalculator[i - 1], PointCalculator[i],Color.red);
        }
    }

    private void LerpList()
    {
        if(PointTfs.Length < 2) return;

        List<Vector3> Point = new List<Vector3>();

        if (listSymmetricalPoint != null)
        {
            for (int i = listSymmetricalPoint.Count - 1; i >= 0; i--)
            {
                Destroy(listSymmetricalPoint[i].gameObject);
            }
        }
        
        listSymmetricalPoint = new List<Transform>();
        for (int i = 0; i < PointTfs.Length; i++)
        {
            Point.Add(PointTfs[i].position);
            if (i > 0)
            {
                Vector3 vtd = FindSymmetricalPoint(Point[i - 1], Point[i], CenterPoint.position);
                vtd.y = (Point[i - 1].y + Point[i].y)/ 2.0f;
                Transform point = Instantiate(SymmetricalPoint,vtd ,
                    Quaternion.identity);
                listSymmetricalPoint.Add(point);
                listSymmetricalPoint[i -1].gameObject.SetActive(true);
            }
        }

        for (int i = 1; i < Point.Count; i++)
        {
            for (int t = 0; t <= countTime; t++)
            {
                float time = t * 1.0f / countTime;
                PointCalculator.Add(Lerp3(Point[i - 1],listSymmetricalPoint[i -1].position,Point[i],time));
            }
        }
        
        
    }

    private Vector3 Lerp(Vector3 p1, Vector3 p2, float t)
    {
        return Vector3.Lerp(p1, p2, t);
    }

    private Vector3 Lerp3(Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return Lerp(Lerp(p1, p2, t), Lerp(p2, p3, t),t);
    }
    
    private Vector3 FindSymmetricalPoint(Vector3 a,Vector3 b, Vector3 c)
    {
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 vtd = a + (Vector3.Dot(ab, ac) / ab.sqrMagnitude) * ab;
        return (vtd * 2) - c;
    }
}
