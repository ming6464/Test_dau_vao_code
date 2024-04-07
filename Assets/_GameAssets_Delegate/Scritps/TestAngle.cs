using System;
using UnityEngine;

public class TestAngle : MonoBehaviour
{
    public Transform Target;
    public bool Run;
    public float Result;
    private Vector3 _angleDraw;


    private void Update()
    {
        if (Run)
        {
            Run = false;
            
            Result = Vector2.SignedAngle(Vector2.right, new Vector2(Target.position.x,Target.position.z));
            Debug.Log(Result);
        }

        _angleDraw = Target.position;
        _angleDraw.y = 0;
        Debug.DrawLine(Vector3.zero,Vector3.forward * 100,Color.blue);
        Debug.DrawLine(Vector3.zero,Vector3.forward * -100,Color.blue);
        Debug.DrawLine(Vector3.zero,Vector3.right * 100,Color.blue);
        Debug.DrawLine(Vector3.zero,Vector3.right * -100,Color.blue);
        Debug.DrawLine(Vector3.zero,_angleDraw,Color.blue);
    }
    
}