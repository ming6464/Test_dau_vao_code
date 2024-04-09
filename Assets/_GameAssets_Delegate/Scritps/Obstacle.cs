
using System;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Transform ObjectTf;
    public TypePoly Type;
    public string Name;
    public Vector3 Position;
    
    [Header("Rectangle")] 
    public float Width;
    public float Height;
    
    [Header("Circle")]
    public float Radius;
    
    private void OnValidate()
    {
        transform.name = transform.GetSiblingIndex().ToString();
        if (ObjectTf)
        {
            switch (Type)
            {
                case TypePoly.Circle:
                    ObjectTf.localScale = Vector3.one * (Radius * 2);
                    break;
                case TypePoly.Rectangle:
                    ObjectTf.localScale = new Vector3(Width, transform.localScale.y, Height);
                    break;
            }
        }
       
        
    }

    private void Awake()
    {
        Position = transform.position;
        Name = transform.GetSiblingIndex().ToString();
        transform.name = Name;
    }
}

[Serializable]
public enum TypePoly
{
    Circle,
    Rectangle
}
