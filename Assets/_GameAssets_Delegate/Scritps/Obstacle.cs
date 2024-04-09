
using System;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public TypePoly Type;
    public string Name;
    
    [Header("Rectangle")] 
    public float X;
    public float Y;
    
    [Header("Circle")]
    public float Radius;
    
    private void OnValidate()
    {
        transform.name = transform.GetSiblingIndex().ToString();
        switch (Type)
        {
            case TypePoly.Circle:
                transform.localScale = Vector3.one * (Radius * 2);
                break;
            case TypePoly.Rectangle:
                transform.localScale = new Vector3(X * 2, transform.localScale.y, Y * 2);
                break;
        }
        
    }

    private void Start()
    {
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
