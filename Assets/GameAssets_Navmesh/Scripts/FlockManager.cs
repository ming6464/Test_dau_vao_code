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
    private int _batchSize = 64;
    
    //"LIST"
    private AgentFlock[] _agentFlocks;
    private NativeArray<float3x3> _agentPositionData;
    private JobHandle _jobHandle;
    private NativeArray<FlockWriteData> _flockWriteDatas;
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
        _flockWriteDatas = new NativeArray<FlockWriteData>(_population, Allocator.Persistent);
        _indexAdd = new NativeList<int>(Allocator.Persistent);
    }

    private void Update()
    {
        Debug.DrawLine(_destination, _destination + Vector3.up, Color.red);

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
            _flockWriteDatas.Dispose();
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
            // if (_agentPositionData[i].c2.z - 1 == 0 || _agentPriority[i] - 1 == 0) continue;
            _agentFlocks[i].OnAvoidNeighbors(_flockWriteDatas[i].velocity);
        }

        for (int i = 0; i <= _indexMax; i++)
        {
            if (!_agentFlocks[i] || _indexAdd.Contains(i)) continue;
            _agentFlocks[i].HandleStop(_flockWriteDatas[i].isBlocked == 1);
            Debug.Log($"index : {i} * name : {_agentFlocks[i].name} stop :  {_flockWriteDatas[i].isBlocked == 1}");
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
            float3x3 agentPosData = _agentPositionData[i];

            if (Math.Abs(_agentFlocks[i].RemainingDistance - 999) < 0.01f)
            {
                team = 999;
            }

            if (_agentPositionData[i].c2.z - 1 == 0 || _flockWriteDatas[i].isBlocked == 1)
            {
                team = -1;
                number--;
            }

            _agentFlocks[i].index = i;
            agentPosData.c0 = _agentFlocks[i].myTrans.position;
            agentPosData.c2.x = team;
            agentPosData.c2.y = _agentFlocks[i].Radius;
            agentPosData.c1 = _agentFlocks[i].forward;
            _agentPositionData[i] = agentPosData;
        }

        for (int i = 0; i <= _indexMax; i ++)
        {
            if(!_agentFlocks[i]) continue;
            _agentFlocks[i].SetName(i.ToString());
        }
    }

    private void ScheduleAgent()
    {
        FlockJob job = new FlockJob
        {
            AvoidDistance = _avoidDistance,
            Speed = _speed,
            AgentPositionData = _agentPositionData,
            FlockWriteDatas = _flockWriteDatas,
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
        [WriteOnly] public NativeArray<FlockWriteData> FlockWriteDatas;


        //properties

        public void Execute(int index)
        {
            if (IndexMax < index) return;
            if (AgentPositionData[index].Equals(default) || AgentPositionData[index].c2.z - 1 == 0) return;
            Random random = new Random((uint)(index + 1));
            FlockWriteDatas[index] = CalculateVT(index, random);
        }

        private FlockWriteData CalculateVT(int index, Random random)
        {
            float3 agentCurPos = AgentPositionData[index].c0;
            float3 directAvoid = float3.zero;
            float curTeamNumber = AgentPositionData[index].c2.x;
            float curRadius = AgentPositionData[index].c2.y;
            float angleCheck = FieldOfView / 2f;
            bool isCheck = math.distance(agentCurPos, Destination) <= DistanceStartCheck;
            float3 forward = AgentPositionData[index].c1;
            FlockWriteData flockWriteData;
            flockWriteData.velocity = float3.zero;
            flockWriteData.isBlocked = 0;
            int agentInView = 0;
            for (int i = 0; i <= IndexMax; i++)
            {
                if (index == i || AgentPositionData[i].c2.z - 1 == 0) continue;

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
                    if ( !vtFore.Equals(float3.zero) &&  (math.length(vtFore) <= RadiusCheckFieldOfView &&
                                                        math.abs(MathJob.Angle(forward, -vtFore)) <= angleCheck))
                    {
                        
                        JobLogger.Log($"index : {index} - : {i} of view | angle : { math.abs(MathJob.Angle(forward, -vtFore))} | angle check : {angleCheck}");
                        
                        agentInView++;
                        if (DensityCheck <= agentInView)
                        {
                            flockWriteData.isBlocked = 1;
                            break;
                        }
                    }
                }
                
                if (curTeamNumber < AgentPositionData[i].c2.x) continue;

                float distance = math.length(vtFore);

                float distanceSet = AvoidDistance + curRadius + AgentPositionData[i].c2.y;


                if (distance > distanceSet) continue;

                vtFore = math.normalize(vtFore);

                directAvoid += vtFore * (distanceSet - distance);
            }
            
            JobLogger.Log($"index : {index} + | is stop job : {flockWriteData.isBlocked} | ischeck : {isCheck} | agent in view : {agentInView}");

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
            
            flockWriteData.velocity = directAvoid * Speed * TimeDelta;
            
            return flockWriteData;
        }
    }

    private void OnDrawGizmos()
    {
        try
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(_destination, new Vector3(_distanceStartCheck * 2, .2f, _distanceStartCheck * 2));
            Gizmos.color = Color.yellow;
            Vector3 vt1 = transform.forward * _radiusCheckFieldOfView;
            Vector3 vt2 = (Quaternion.Euler(0, _fieldOfView / 2f, 0) * vt1);
            Vector3 vt3 = (Quaternion.Euler(0, -_fieldOfView / 2f, 0) * vt1);
            Vector3 pos1 = vt1 + transform.position;
            Vector3 pos2 = vt2 + transform.position;
            Vector3 pos3 = vt3 + transform.position;
            float dotLength = math.abs(Vector3.Dot(vt2, vt1.normalized));

            Gizmos.DrawLine(transform.position, pos2);
            Gizmos.DrawLine(transform.position, pos3);
            float add = (_radiusCheckFieldOfView - dotLength) / _densityDraw;
            Vector3 pos4 = pos2, pos5 = pos3;
            for (float i = dotLength + add; i < _radiusCheckFieldOfView; i += add)
            {
                // Tính cos(góc giữa c và a)
                float cosTheta = i / _radiusCheckFieldOfView;
                float angle = Mathf.Acos(cosTheta) * Mathf.Rad2Deg;
                Vector3 pos_4 = (Quaternion.Euler(0, angle, 0) * vt1) + transform.position;
                Vector3 pos_5 = (Quaternion.Euler(0, -angle, 0) * vt1) + transform.position;
                Gizmos.DrawLine(pos4, pos_4);
                Gizmos.DrawLine(pos5, pos_5);
                pos4 = pos_4;
                pos5 = pos_5;
            }

            Gizmos.DrawLine(pos1, pos4);
            Gizmos.DrawLine(pos1, pos5);
        }
        catch
        {
            //ignored
        }
    }
    
    private struct FlockWriteData
    {
        public float3 velocity;
        public int isBlocked;
    }
}