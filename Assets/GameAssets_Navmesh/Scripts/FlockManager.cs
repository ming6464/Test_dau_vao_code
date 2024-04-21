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
    [SerializeField] private Vector3 _destination;
    [SerializeField] private float _distanceStartCheck;
    [Range(1, 360)] [SerializeField] private float _fieldOfView;
    [SerializeField] private float _radiusCheckFieldOfView;
    [Min(0)] [SerializeField] private int _densityCheck;
    [Min(1)] [SerializeField] private float _densityDraw = 5;

    [SerializeField, Range(0, 50), Tooltip("Sô lần được phép tác dụng lực tránh né khi bị dừng lại")]
    private byte _countApplyVelocity;

    private int _batchSize = 64;

    //"LIST"
    private AgentFlock[] _agentFlocks;
    private NativeArray<float3x3> _agentPositionData;
    private JobHandle _jobHandle;
    private NativeArray<AgentData> _agentReadDatas;
    private NativeArray<AgentData> _agentWriteDatas;
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

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.LeftClick, OnLeftClick);
    }

    private void OnValidate()
    {
        try
        {
            foreach (var agent in _agentFlocks)
            {
                if (!agent) continue;
                agent.radiusCheckFieldOfView = _radiusCheckFieldOfView;
                agent.densityDraw = _densityDraw;
                agent.fieldOfView = _fieldOfView;
            }
        }
        catch
        {
            //ignored
        }
    }

    private void OnLeftClick(object obj)
    {
        return;
        if (obj == null) return;
        _destination = (Vector3)obj;
    }


    private void Init()
    {
        _agentFlocks = new AgentFlock[_population];
        _agentPositionData = new NativeArray<float3x3>(_population, Allocator.Persistent);
        _agentWriteDatas = new NativeArray<AgentData>(_population, Allocator.Persistent);
        _agentReadDatas = new NativeArray<AgentData>(_population, Allocator.Persistent);
        _indexAdd = new NativeList<int>(Allocator.Persistent);
    }

    private void Update()
    {
        Debug.DrawLine(_destination, _destination + Vector3.up, Color.red);

        if (_indexMax <= 0) return;
        _indexMax = math.min(_indexMax, _population - 1);
        _jobHandle.Complete();
        _agentReadDatas.CopyFrom(_agentWriteDatas);
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
            _agentWriteDatas.Dispose();
        }
        catch
        {
            //ignore
        }

        try
        {
            _agentWriteDatas.Dispose();
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
        for (int i = 0; i <= _indexMax; i++)
        {
            if (!_agentFlocks[i] || _indexAdd.Contains(i)) continue;
            if (_agentPositionData[i].c2.z - 1 == 0) continue;
            _agentFlocks[i].HandleStop(_agentReadDatas[i].isBlocked == 1);
            if (_agentReadDatas[i].isBlocked == 1)
            {
                if (_agentReadDatas[i].countApplyVeloctiy < _countApplyVelocity)
                {
                    AgentData agentWriteData = _agentReadDatas[i];
                    agentWriteData.countApplyVeloctiy++;
                    _agentReadDatas[i] = agentWriteData;
                }
                else
                {
                    continue;
                }
            }

            _agentFlocks[i].OnAvoidNeighbors(_agentReadDatas[i].velocity);
            Debug.Log($" {i} is stop :  {_agentReadDatas[i].isBlocked == 1}");
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

            if (_agentPositionData[i].c2.z - 1 == 0)
            {
                continue;
            }

            number++;
            if (number > _teamCapacity)
            {
                number = 0;
                teamNumber++;
            }

            int team = teamNumber;
            float3x3 agentPosData = _agentPositionData[i];

            if (Math.Abs(_agentFlocks[i].RemainingDistance - 999) < 0.01f)
            {
                team = 999;
            }

            _agentFlocks[i].index = i;
            agentPosData.c0 = _agentFlocks[i].myTrans.position;
            agentPosData.c2.x = team;
            agentPosData.c2.y = _agentFlocks[i].Radius;
            agentPosData.c1 = _agentFlocks[i].forward;
            _agentPositionData[i] = agentPosData;
        }
    }

    private void ScheduleAgent()
    {
        FlockJob job = new FlockJob
        {
            AvoidDistance = _avoidDistance,
            Speed = _speed,
            AgentPositionData = _agentPositionData,
            AgentWriteData = _agentWriteDatas,
            AgentReadData = _agentReadDatas,
            RadiusCheckFieldOfView = _radiusCheckFieldOfView,
            DistanceStartCheck = _distanceStartCheck,
            Destination = _destination,
            IndexMax = _indexMax,
            DensityCheck = _densityCheck,
            FieldOfView = _fieldOfView,
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
                agent.radiusCheckFieldOfView = _radiusCheckFieldOfView;
                agent.densityDraw = _densityDraw;
                agent.fieldOfView = _fieldOfView;
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
                _agentPositionData[index] = float3x3.zero;
            }

            float3x3 agentPosData = _agentPositionData[index];
            agentPosData.c2.z = isDefault ? 0 : 1;
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
        [ReadOnly] public float FieldOfView;
        [ReadOnly] public float RadiusCheckFieldOfView;
        [ReadOnly] public float DistanceStartCheck;
        [ReadOnly] public int DensityCheck;
        [ReadOnly] public float3 Destination;
        [ReadOnly] public NativeArray<float3x3> AgentPositionData;
        [ReadOnly] public NativeArray<AgentData> AgentReadData;
        [WriteOnly] public NativeArray<AgentData> AgentWriteData;


        //properties

        public void Execute(int index)
        {
            if (IndexMax < index) return;
            if (AgentPositionData[index].Equals(default) || AgentPositionData[index].c2.z - 1 == 0) return;
            Random random = new Random((uint)(index + 1));
            AgentWriteData[index] = CalculateVT(index, random);
        }

        private AgentData CalculateVT(int index, Random random)
        {
            AgentData agentWriteData = AgentReadData[index];
            float3 agentCurPos = AgentPositionData[index].c0;
            float3 directAvoid = float3.zero;
            float curTeamNumber = AgentPositionData[index].c2.x;
            float curRadius = AgentPositionData[index].c2.y;
            float angleCheck = FieldOfView / 2f;
            bool isCheck = math.distance(agentCurPos, Destination) <= DistanceStartCheck;
            int agentInView = 0;
            agentWriteData.isBlocked = 0;
            for (int i = 0; i <= IndexMax; i++)
            {
                if (index == i) continue;

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

                if (isCheck)
                {
                    if (math.length(vtFore) <= RadiusCheckFieldOfView &&
                        math.abs(MathJob.Angle(AgentPositionData[index].c1, -vtFore)) <= angleCheck)
                    {
                        agentInView++;
                    }
                }

                if (curTeamNumber < AgentPositionData[i].c2.x) continue;

                float distance = math.length(vtFore);

                float distanceSet = AvoidDistance + curRadius + AgentPositionData[i].c2.y;


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

            directAvoid = directAvoid * Speed * TimeDelta;
            agentWriteData.velocity = directAvoid;
            if (DensityCheck <= agentInView)
            {
                agentWriteData.isBlocked = 1;
            }
            else
            {
                agentWriteData.countApplyVeloctiy = 0;
            }

            return agentWriteData;
        }
    }

    private struct AgentData
    {
        public float3 velocity;
        public byte isBlocked;
        public byte countApplyVeloctiy;
    }

    private void OnDrawGizmos()
    {
        try
        {
            DrawCircle(transform.position, _distanceStartCheck, 360, transform.forward, Color.green, _densityDraw);
            DrawCircle(transform.position, _radiusCheckFieldOfView, _fieldOfView, transform.forward, Color.yellow,
                _densityDraw);
        }
        catch
        {
            //ignored
        }


        void DrawCircle(float3 center, float radius, float angle, float3 forward, Color color, float detail = 5)
        {
            try
            {
                if (forward.Equals(float3.zero)) return;
                if (angle > 360)
                {
                    angle %= 360;
                }

                Gizmos.color = color;
                angle = math.abs(angle);
                detail = math.max(detail, 5);
                float angleReal = angle / 2f;
                float3 vt0 = math.normalize(forward) * radius;
                float3 point1 = MathJob.Rotate(vt0, new float3(0, angleReal, 0)) + center;
                float3 point2 = MathJob.Rotate(vt0, new float3(0, -angleReal, 0)) + center;
                float3 point3 = default;
                float3 point4 = default;

                if (angle < 360)
                {
                    Gizmos.DrawLine(center, point1);
                    Gizmos.DrawLine(center, point2);
                }

                float add = angleReal / detail;
                bool check = false;
                for (float i = 1; i <= detail; i++)
                {
                    float angleReal2 = angleReal - i * add;
                    point3 = MathJob.Rotate(vt0, new float3(0, angleReal2, 0)) + center;
                    point4 = MathJob.Rotate(vt0, new float3(0, -angleReal2, 0)) + center;
                    Gizmos.DrawLine(point1, point3);
                    Gizmos.DrawLine(point2, point4);
                    point1 = point3;
                    point2 = point4;
                }
            }
            catch
            {
                //ignored
            }
        }
    }
}