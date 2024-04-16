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
    public NativeArray<float3x3> _agentPositionData;
    private JobHandle _jobHandle;
    public NativeArray<float3> _velocityData;
    private NativeList<int> _indexAdd;
    public int CountMax;

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
        _agentPositionData = new NativeArray<float3x3>(_countMax, Allocator.Persistent);
        _velocityData = new NativeArray<float3>(_countMax, Allocator.Persistent);
        _indexAdd = new NativeList<int>(Allocator.Persistent);
    }

    private void Update()
    {
        if (CountMax <= 0) return;
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
        for (int i = 0; i < CountMax; i++)
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
        int[] listSort = new int[CountMax];

        for (int i = 0; i < CountMax; i++)
        {
            listSort[i] = i;
        }

        for (int i = 0; i < CountMax; i++)
        {
            int indexI = listSort[i];
            if (!_agentFlocks[indexI])
            {
                continue;
            }

            for (int j = i + 1; j < CountMax; j++)
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
            _agentPositionData[i] = new float3x3(_agentFlocks[i].transform.position, _agentFlocks[i].Forward,
                new float3(team, _agentFlocks[i].InTimeAvoid ? 1 : 0, 0));
        }
    }


    private void ScheduleAgent()
    {
        FlockJob job = new FlockJob();
        job.AgentPositionData = _agentPositionData;
        job.AvoidDistance = _avoidDistance;
        job.VelocityData = _velocityData;
        job.Speed = _speed;
        job.CountMax = CountMax;
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
            if (i >= CountMax)
            {
                CountMax = i + 1;
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
            if (index >= CountMax)
            {
                CountMax = index - 1;
            }

            _agentFlocks[index] = null;
            int indexOf = _indexAdd.IndexOf(index);
            if (indexOf >= 0)
            {
                _indexAdd[indexOf] = -1;
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
        [ReadOnly] public int CountMax;
        [ReadOnly] public NativeArray<float3x3> AgentPositionData;
        [WriteOnly] public NativeArray<float3> VelocityData;
        [ReadOnly] public int indexCheckVar;

        public void Execute(int index)
        {
            if (CountMax <= index) return;
            if (AgentPositionData[index].Equals(default)) return;
            var random = new Random((uint)(index + 1));
            if (AgentPositionData[index].c2.y == 0)
            {
                VelocityData[index] = CalculateVT1(index, random) * Speed * TimeDelta;
            }
            else
            {
                VelocityData[index] = CalculateVT(index, random);
            }
        }

        private float3 CalculateVT(int index, Random random)
        {
            float3 agentCurPos = AgentPositionData[index].c0;
            float3 directAvoid = float3.zero;
            float curTeamNumber = AgentPositionData[index].c2.x;
            int collisionCount = 0;
            for (int i = 0; i < CountMax; i++)
            {
                if (index == i || AgentPositionData[i].Equals(default) ||
                    curTeamNumber < AgentPositionData[i].c2.x) continue;
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

        private float3 CalculateVT1(int index, Random random)
        {
            float3 agentCurPos = AgentPositionData[index].c0;
            float3 directAvoid = AgentPositionData[index].c1;
            float curTeamNumber = AgentPositionData[index].c2.x;
            NativeList<float3> positionSet = new NativeList<float3>(Allocator.Temp);
            int collisionCount = 0;
            for (int i = 0; i < CountMax; i++)
            {
                if (index == i || AgentPositionData[i].Equals(default) ||
                    curTeamNumber < AgentPositionData[i].c2.x) continue;
            }


            return directAvoid;
        }

        private bool IsLineIntersectCircle(float3 O, float R, float3 dir, float3 A)
        {
            if (dir.Equals(float3.zero)) return false;
            float3 B = math.normalize(dir) * 10 + A;
            float2 point1 = new float2(A.x, A.z);
            float2 point2 = new float2(B.x, B.z);
            float2 point3 = new float2(O.x, O.z);

            float d = math.abs(
                ((point2.x - point1.x) * (point1.y - point3.y) - (point1.x - point3.x) * (point2.y - point1.y)) /
                (math.distance(point1, point2)));

            if (d < R)
            {
                return true;
            }

            return false;
        }
    }
}