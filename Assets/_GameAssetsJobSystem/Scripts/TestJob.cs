using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class TestJob : MonoBehaviour
{
    public bool Run;
    public Int32 Count;
    public Int32 BatchSize;
    public bool Finish;
    private JobHandle _jobHandle;
    private MyJobLog _job;
    private void Start()
    {
        _job = new MyJobLog();
    }

    private void Update()
    {
        Finish = _jobHandle.IsCompleted;
        _jobHandle.Complete();
        if (Run)
        {
            Run = false;
            _jobHandle = _job.Schedule(Count, BatchSize);
        }
    }

    public struct MyJobLog : IJobParallelFor
    {
        [WriteOnly]
        private float a;
        
        public void Execute(int index)
        {
            a++;
            Debug.Log($"Hello - {a} -//// {index}");
        }
    }
}
