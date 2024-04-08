
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float Radius;
    public string Name;
    private void OnValidate()
    {
        transform.localScale = Vector3.one * (Radius * 2);
        Name = transform.GetSiblingIndex().ToString();
        transform.name = Name;
    }
}
