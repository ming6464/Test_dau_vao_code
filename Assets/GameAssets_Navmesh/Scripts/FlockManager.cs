using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Random = Unity.Mathematics.Random;

public class FlockManager : MonoBehaviour
{
    #region PROPERTIES

    public static FlockManager Instance => _ins;
    private static FlockManager _ins;

    [SerializeField] private float _agentRadius;
    [SerializeField] private int _countMax;
    [SerializeField] private int _batchSize;
    [SerializeField] private float _avoidDistance;
    [SerializeField] private float _reachDistance;

    [SerializeField] private float _speed;

    //"LIST"
    private AgentFlock[] _agentFlocks;

    // private NativeHashMap<int, NativeArray<float3>> _agentPaths;
    private NativeArray<float3> _agentPositionData;
    private JobHandle _jobHandle;
    private NativeArray<float3> _velocityData;

    #endregion

    #region UNITY CORE

    private void Awake()
    {
        if (!_ins)
        {
            _ins = this;
        }

        Init();
    }

    private void Init()
    {
        _agentFlocks = new AgentFlock[_countMax];
        // _agentPaths = new NativeHashMap<int, NativeArray<float3>>(_countMax, Allocator.Persistent);
        _agentPositionData = new NativeArray<float3>(_countMax, Allocator.Persistent);
        _velocityData = new NativeArray<float3>(_countMax, Allocator.Persistent);
    }

    private void Update()
    {
        UpdateDataList();
        ScheduleAgent();
    }

    private void LateUpdate()
    {
        ApplyVelocity();
    }

    private void OnDisable()
    {
        try
        {
            // _agentPaths.Dispose();
        }
        catch
        {
            //ignore
        }

        try
        {
            _agentPositionData.Dispose();
        }
        catch
        {
            //ignore
        }

        try
        {
            _velocityData.Dispose();
        }
        catch
        {
            //ignore
        }
    }

    #endregion

    #region PRIVATE METHOD

    private void ApplyVelocity()
    {
        _jobHandle.Complete();
        for (int i = 0; i < _velocityData.Length; i++)
        {
            if (_agentFlocks[i])
            {
                _agentFlocks[i].Move(_velocityData[i]);
            }
        }
    }

    private void UpdateDataList()
    {
        for (int i = 0; i < _agentFlocks.Length; i++)
        {
            if (_agentFlocks[i])
            {
                _agentPositionData[i] = _agentFlocks[i].transform.position;
            }
            else
            {
                _agentPositionData[i] = default;
            }
        }
    }

    private void ScheduleAgent()
    {
        _jobHandle.Complete();
        FlockJob job = new FlockJob();
        job.AgentPositionData = _agentPositionData;
        job.AvoidDistance = _avoidDistance;
        job.VelocityData = _velocityData;
        job.Radius = _agentRadius;
        job.Speed = _speed;
        job.TimeDelta = Time.deltaTime;
        // job.AgentPaths = _agentPaths;
        _jobHandle = job.Schedule(_countMax, _batchSize);
    }

    #endregion

    #region PUBLIC METHOD

    public void AddAgent(AgentFlock agent)
    {
        for (int i = 0; i < _agentFlocks.Length; i++)
        {
            if (_agentFlocks[i]) continue;
            agent.Index = i;
            agent.ReachDistance = _reachDistance;
            _agentFlocks[i] = agent;
        }
    }

    public void RemoveAgent(int index)
    {
        _agentFlocks[index] = null;
        // if (_agentPaths.ContainsKey(index))
        // {
        //     _agentPaths.Remove(index);
        // }
    }

    public void AddPath(int index, NativeList<float3> path)
    {
        // if (_agentPaths.ContainsKey(index)) return;
        // _agentPaths.Add(index, path);
    }

    #endregion


    [BurstCompatible]
    public struct FlockJob : IJobParallelFor
    {
        [ReadOnly] public float Radius;
        [ReadOnly] public float TimeDelta;
        [ReadOnly] public float AvoidDistance;

        [ReadOnly] public float Speed;

        // [ReadOnly] public NativeHashMap<int, NativeArray<float3>> AgentPaths;
        [ReadOnly] public NativeArray<float3> AgentPositionData;
        [WriteOnly] public NativeArray<float3> VelocityData;
        //properties

        public void Execute(int index)
        {
            if (AgentPositionData[index].Equals(default)) return;
            VelocityData[index] = math.normalize(CalculateVT(index)) * Speed * TimeDelta;
        }

        private float3 CalculateVT(int index)
        {
            float3 currentPosition = AgentPositionData[index];
            float3 directAvoid = new Vector3(0, 0, 0);
            float3 directToNextPosition = new float3(0, 0, 0);
            float weightDirectAvoidPercent = 1;

            float radiusCheck = Radius + AvoidDistance / 2f;
            for (int i = 0; i < AgentPositionData.Length; i++)
            {
                if (index == i) continue;
                float3 agentPos = AgentPositionData[i];
                float distance = math.distance(agentPos, currentPosition);
                if (distance > (2 * radiusCheck)) continue;
                agentPos += new Random().NextFloat3(new float3(1f, 0f, 1f));
                float3 counterForce = math.normalize(currentPosition - agentPos) * ((2 * radiusCheck) - distance);
                directAvoid += counterForce;
            }

            // if (AgentPaths.ContainsKey(index))
            // {
            //     if (AgentPaths[index].Length > 0)
            //     {
            //         weightDirectAvoidPercent = .3f;
            //     }
            // }


            return (directAvoid * weightDirectAvoidPercent) + (directToNextPosition * (1 - weightDirectAvoidPercent));
        }
    }
}