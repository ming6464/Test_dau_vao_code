using System;
using UnityEngine;

public class TestScript_Job : MonoBehaviour
{
    [Range(1, 100)] public float Speed;
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    private void Update()
    {
        _transform.position += Vector3.one * Speed;
    }
}