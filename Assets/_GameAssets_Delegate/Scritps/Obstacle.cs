
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float Radius;

    private void OnValidate()
    {
        transform.localScale = Vector3.one * (Radius * 2);
    }
}
