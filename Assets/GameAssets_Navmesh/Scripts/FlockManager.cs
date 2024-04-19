using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class FlockManager : MonoBehaviour
{
    #region PROPERTIES

    public static FlockManager Instance => _ins;
    private static FlockManager _ins;

    [Tooltip("Số lượng tối đa các bot mà flock có thể quản lí"), Range(10, 5000), SerializeField]
    private int _population = 250;

    [Tooltip("Tốc độ né"), Range(1, 5f), SerializeField]
    private float _speed = 3;

    [Tooltip("Khoảng giữa 2 bot"), Range(0, 5f), SerializeField]
    private float _avoidDistance = .5f;

    [Range(1, 10), SerializeField] private int _teamCapacity = 5;
    
    [SerializeField] private float _distanceStartCheck;
    [SerializeField] private float _fieldOfView;
    [SerializeField] private float _radiusCheckFieldOfView;
    
    private int _batchSize = 64;

    //"LIST"
    private AgentFlock[] _agentFlocks;
    private NativeArray<float3x2> _agentPositionData;
    private JobHandle _jobHandle;
    private NativeArray<float3> _velocityData;
    private NativeList<int> _indexAdd;
    public int _indexMax;

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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 pos1 = ((Quaternion.Euler(0, _fieldOfView / 2, 0) * transform.forward) * _fieldOfView) + transform.position;
        Vector3 pos2 = ((Quaternion.Euler(0, -_fieldOfView / 2, 0) * transform.forward) * _fieldOfView) + transform.position;
        Gizmos.DrawLine(transform.position,pos1);
        Gizmos.DrawLine(transform.position,pos2);
    }

    private void Init()
    {
        _agentFlocks = new AgentFlock[_population];
        _agentPositionData = new NativeArray<float3x2>(_population, Allocator.Persistent);
        _velocityData = new NativeArray<float3>(_population, Allocator.Persistent);
        _indexAdd = new NativeList<int>(Allocator.Persistent);
    }


    private void Update()
    {
        if (_indexMax <= 0) return;
        _indexMax = math.min(_indexMax, _population - 1);
        _jobHandle.Complete();
        ApplyVelocity();
        UpdateDataList();
        ScheduleAgent();
    }


    private void OnDisable()
    {
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

        try
        {
            _indexAdd.Dispose();
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
        bool check;
        for (int i = 0; i <= _indexMax; i++)
        {
            if (!_agentFlocks[i]) continue;
            check = !_indexAdd.Contains(i) && _agentPositionData[i].c1.z == 0;
            if (!check) continue;
            _agentFlocks[i].OnAvoidNeighbors(_velocityData[i]);
        }

        _indexAdd.Clear();
    }

    private void UpdateDataList()
    {
        int[] listSort = new int[_indexMax + 1];

        for (int i = 0; i <= _indexMax; i++)
        {
            listSort[i] = i;
        }

        for (int i = 0; i <= _indexMax; i++)
        {
            int indexI = listSort[i];
            if (!_agentFlocks[indexI])
            {
                continue;
            }

            for (int j = i + 1; j <= _indexMax; j++)
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

            if (_agentPositionData[i].c1.z - 1 == 0)
            {
                team = 0;
                teamNumber--;
            }
            else if (Math.Abs(_agentFlocks[i].RemainingDistance - 999) < 0.01f)
            {
                team = 999;
            }

            _agentFlocks[i].index = i;
            _agentPositionData[i] = new float3x2(_agentFlocks[i].transform.position,
                new float3(team, _agentFlocks[i].Radius, 0));
        }
    }

    private void ScheduleAgent()
    {
        FlockJob job = new FlockJob
        {
            AvoidDistance = _avoidDistance,
            Speed = _speed,
            AgentPositionData = _agentPositionData,
            VelocityData = _velocityData,
            IndexMax = _indexMax,
            TimeDelta = Time.deltaTime
        };

        _jobHandle = job.Schedule(_population, _batchSize);
    }

    #endregion

    #region PUBLIC METHOD

    public void AddAgent(AgentFlock agent, int passIndex, int ID)
    {
        try
        {
            RemoveAgent(passIndex, ID);
            for (int i = 0; i < _agentFlocks.Length; i++)
            {
                if (_agentFlocks[i]) continue;
                if (i > _indexMax)
                {
                    _indexMax = i;
                }

                _indexAdd.Add(i);
                agent.index = i;
                _agentFlocks[i] = agent;
                return;
            }
        }
        catch
        {
            //ignore
        }
    }

    public void RemoveAgent(int index, int ID)
    {
        if (index < 0) return;

        try
        {
            if (_agentFlocks[index] && _agentFlocks[index].transform.GetInstanceID() == ID)
            {
                if (index >= _indexMax)
                {
                    _indexMax = index - 1;
                    _indexMax = math.max(_indexMax, 0);
                }

                _agentFlocks[index] = null;
                for (int i = 0; i < _indexAdd.Length; i++)
                {
                    if (_indexAdd[i] == index)
                    {
                        _indexAdd[i] = -1;
                        return;
                    }
                }
            }
        }
        catch
        {
            //ignore
        }
    }

    public void EditPriority(bool isDefault, int index, int ID)
    {
        try
        {
            if (_agentFlocks[index].ID != ID) return;

            if (_agentPositionData[index].Equals(default))
            {
                _agentPositionData[index] = new float3x2(float3.zero, float3.zero);
            }

            float3x2 agentPosData = _agentPositionData[index];
            agentPosData.c1.z = isDefault ? 0 : 1;
            _agentPositionData[index] = agentPosData;
        }
        catch
        {
            //ignored
        }
    }

    #endregion


    [BurstCompatible]
    private struct FlockJob : IJobParallelFor
    {
        [ReadOnly] public float TimeDelta;
        [ReadOnly] public float AvoidDistance;
        [ReadOnly] public float Speed;
        [ReadOnly] public int IndexMax;
        [ReadOnly] public NativeArray<float3x2> AgentPositionData;
        [WriteOnly] public NativeArray<float3> VelocityData;
        //properties

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
            float curRadius = AgentPositionData[index].c1.y;
            for (int i = 0; i <= IndexMax; i++)
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

                float distanceSet = AvoidDistance + curRadius + AgentPositionData[i].c1.y;

                if (distance > distanceSet) continue;

                vtFore = math.normalize(vtFore);

                directAvoid += vtFore * (distanceSet - distance);
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