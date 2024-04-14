using System;
using System.Collections.Generic;
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

    public int indexCheckVar;
    [SerializeField] private int _countMax;
    [SerializeField] private int _batchSize;
    [SerializeField] private float _avoidDistance;
    [SerializeField] private float _reachDistance;
    [SerializeField] private float _speed;

    [Range(1, 5)] [SerializeField] private int _teamCapacity;

    //"LIST"
    private AgentFlock[] _agentFlocks;
    public NativeArray<float3x2> _agentPositionData;
    private JobHandle _jobHandle;
    public NativeArray<float3> _velocityData;
    private NativeList<int> _indexAdd;
    public int IndexMax;

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
        _agentPositionData = new NativeArray<float3x2>(_countMax, Allocator.Persistent);
        _velocityData = new NativeArray<float3>(_countMax, Allocator.Persistent);
        _indexAdd = new NativeList<int>(Allocator.Persistent);
    }

    private void Update()
    {
        _jobHandle.Complete();
        ApplyVelocity();
        UpdateDataList();
        ScheduleAgent();
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
        for (int i = 0; i < IndexMax; i++)
        {
            if (_agentFlocks[i] && !_indexAdd.Contains(i) && !_velocityData[i].Equals(default))
            {
                _agentFlocks[i].Move(_velocityData[i]);
            }
        }

        _indexAdd.Clear();
    }


    private void UpdateDataList()
    {
        int[] listSort = new int[IndexMax];

        for (int i = 0; i < IndexMax; i++)
        {
            listSort[i] = i;
        }

        for (int i = 0; i < IndexMax; i++)
        {
            int indexI = listSort[i];
            if (!_agentFlocks[indexI])
            {
                continue;
            }

            for (int j = i + 1; j < IndexMax; j++)
            {
                int indexJ = listSort[j];
                if (!_agentFlocks[indexJ]) continue;

                if (_agentFlocks[indexI].RemainingDistance > _agentFlocks[indexJ].RemainingDistance)
                {
                    (listSort[i], listSort[j]) = (listSort[j], listSort[i]);
                }
            }
        }

        int teamNumber = 0;
        int number = 0;
        foreach (int i in listSort)
        {
            if (!_agentFlocks[i])
            {
                _agentPositionData[i] = default;
                continue;
            }

            number++;
            if (number > _teamCapacity)
            {
                number = 0;
                teamNumber++;
            }

            int team = teamNumber;

            if (_agentFlocks[i].RemainingDistance == 999)
            {
                team = 999;
            }

            _agentFlocks[i].Index = i;
            _agentFlocks[i].SetTeam(team);
            _agentPositionData[i] = new float3x2(_agentFlocks[i].transform.position, new float3(team, 0, 0));
            Debug.Log(
                $"Index : {i} | position : {_agentFlocks[i].transform.position} | name {_agentFlocks[i].transform.name}");
        }
    }


    private void ScheduleAgent()
    {
        FlockJob job = new FlockJob();
        job.AgentPositionData = _agentPositionData;
        job.AvoidDistance = _avoidDistance;
        job.VelocityData = _velocityData;
        job.Speed = _speed;
        job.IndexMax = IndexMax;
        job.TimeDelta = Time.deltaTime;
        job.indexCheckVar = indexCheckVar;
        _jobHandle = job.Schedule(_countMax, _batchSize);
    }

    #endregion

    #region PUBLIC METHOD

    public void AddAgent(AgentFlock agent, int passIndex, int ID)
    {
        RemoveAgent(passIndex, ID);
        for (int i = 0; i < _agentFlocks.Length; i++)
        {
            if (_agentFlocks[i]) continue;
            if (i > IndexMax)
            {
                IndexMax = i;
            }

            _indexAdd.Add(i);
            agent.Index = i;
            agent.ReachDistance = _reachDistance;
            _agentFlocks[i] = agent;
            return;
        }
    }

    public void RemoveAgent(int index, int ID)
    {
        if (index < 0) return;
        if (_agentFlocks[index] && _agentFlocks[index].transform.GetInstanceID() == ID)
        {
            if (index >= IndexMax)
            {
                IndexMax = index - 1;
            }

            _agentFlocks[index] = null;
            int indexOf = _indexAdd.IndexOf(index);
            if (indexOf >= 0)
            {
                _indexAdd.RemoveAtSwapBack(index);
            }
        }
    }

    #endregion


    [BurstCompatible]
    public struct FlockJob : IJobParallelFor
    {
        [ReadOnly] public float TimeDelta;
        [ReadOnly] public float AvoidDistance;
        [ReadOnly] public float Speed;
        [ReadOnly] public int IndexMax;
        [ReadOnly] public NativeArray<float3x2> AgentPositionData;
        [WriteOnly] public NativeArray<float3> VelocityData;
        [ReadOnly] public int indexCheckVar;

        public void Execute(int index)
        {
            if (IndexMax < index) return;
            if (AgentPositionData[index].Equals(default)) return;
            var random = new Random((uint)(index + 1));
            VelocityData[index] = CalculateVT(index, random) * Speed * TimeDelta;
        }

        private float3 CalculateVT(int index, Random random)
        {
            float3 agentCurPos = AgentPositionData[index].c0;
            float3 directAvoid = float3.zero;
            float curTeamNumber = AgentPositionData[index].c1.x;
            int collisionCount = 0;
            for (int i = 0; i < IndexMax; i++)
            {
                if (index == i || AgentPositionData[i].Equals(default) ||
                    curTeamNumber < AgentPositionData[i].c1.x) continue;
                float3 vtFore = agentCurPos - AgentPositionData[i].c0;

                if (vtFore.Equals(float3.zero))
                {
                    vtFore = new float3(random.NextFloat(-.3f, .3f), 0, random.NextFloat(-.3f, .3f))
                    {
                        y = agentCurPos.y
                    };

                    int ik = 0;

                    while (vtFore is { z: 0, x: 0 })
                    {
                        if (ik == 0)
                        {
                            vtFore.z = random.NextFloat(-.3f, .3f);
                        }
                        else if (ik == 1)
                        {
                            vtFore.x = random.NextFloat(-.3f, .3f);
                        }
                        else
                        {
                            vtFore.x = random.NextFloat(-.3f, .3f);
                            vtFore.z = random.NextFloat(-.3f, .3f);
                        }

                        ik++;
                        if (ik > 3)
                        {
                            ik = 0;
                        }
                    }
                }

                float distance = math.length(vtFore);

                if (distance > AvoidDistance) continue;

                collisionCount++;

                vtFore = math.normalize(vtFore);

                directAvoid += vtFore * (AvoidDistance - distance);
            }

            if (!directAvoid.Equals(float3.zero))
            {
                directAvoid = math.normalize(directAvoid);
            }

            if (math.any(math.isnan(directAvoid)))
            {
                if (math.isnan(directAvoid.x))
                {
                    directAvoid.x = random.NextFloat(-.3f, .3f);
                }

                if (math.isnan(directAvoid.y))
                {
                    directAvoid.x = agentCurPos.y;
                }

                if (math.isnan(directAvoid.z))
                {
                    directAvoid.z = random.NextFloat(-.3f, .3f);
                }
            }

            return directAvoid;
        }
    }
}