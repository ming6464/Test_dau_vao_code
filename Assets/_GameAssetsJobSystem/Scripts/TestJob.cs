using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public class TestJob : MonoBehaviour
{
    public bool Run;
    public Transform Point;
    public Int32 Count;
    public Int32 BatchSize;
    public bool Finish;
    private JobHandle _jobHandle;
    private MyJobLog _job;
    private NativeArray<float3> pos;

    private void Start()
    {
        pos = new NativeArray<float3>(Count, Allocator.Persistent);
    }

    private void Update()
    {
        pos[0] = Point.position;
        Finish = _jobHandle.IsCompleted;
        _jobHandle.Complete();
        if (Run)
        {
            _job = new MyJobLog();
            _job.Pos = pos;
            Run = false;
            _jobHandle = _job.Schedule(Count, BatchSize);
        }
    }

    public struct MyJobLog : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> Pos;

        public void Execute(int index)
        {
            Debug.Log($"Hello - {Pos[0]}");
        }
    }
}