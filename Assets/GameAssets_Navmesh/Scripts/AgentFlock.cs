using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class AgentFlock : MonoBehaviour
{
    public int Index
    {
        get => _index;
        set => _index = value;
    }

    public NavMeshAgent NavAgent;
    public float ReachDistance;
    public int FrameCountDownSet;
    public float RemainingDistance;

    private FlockManager _flockManager;
    private List<float3> Paths;
    private bool CalculatePath;
    private int _frameCountDownDelta;
    [SerializeField] private int _index;

    private void Awake()
    {
        if (!NavAgent)
        {
            TryGetComponent(out NavAgent);
        }

        Paths = new List<float3>();
        _frameCountDownDelta = -1;
        CalculatePath = false;
        Index = -1;
    }

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.RightClick, OnRightClick);
        EventDispatcher.Instance.RegisterListener(EventID.LeftClick, OnLeftClick);
    }

    private void OnLeftClick(object obj)
    {
        if (obj == null) return;
        Debug.Log($"Right Click {Index}");
        OnRightClick(null);
        DelayToRenderComplete((Vector3)obj);
    }

    private void OnRightClick(object obj)
    {
        NavAgent.ResetPath();
    }

    private void OnDisable()
    {
        _flockManager.RemoveAgent(Index, GetInstanceID());
    }

    private void Start()
    {
        if (NavAgent.isOnNavMesh)
        {
            NavAgent.isStopped = true;
        }

        if (FlockManager.Instance)
        {
            _flockManager = FlockManager.Instance;
            _flockManager.AddAgent(this, Index, GetInstanceID());
        }
    }

    private void Update()
    {
        // if (Paths.Count > 0)
        // {
        //     for (int i = 0; i < Paths.Count - 1; i++)
        //     {
        //         if (i == 0)
        //         {
        //             Debug.DrawLine(transform.position, Paths[i], Color.blue);
        //         }
        //         else
        //         {
        //             Debug.DrawLine(Paths[i + 1], Paths[i], Color.blue);
        //         }
        //     }
        // }

        if (!NavAgent.isStopped && NavAgent.path.corners.Length > 1)
        {
            for (int i = 0; i < NavAgent.path.corners.Length - 1; i++)
            {
                Debug.DrawLine(NavAgent.path.corners[i + 1], NavAgent.path.corners[i], Color.green);
            }

            RemainingDistance = NavAgent.remainingDistance;
        }

        if (NavAgent.isStopped)
        {
            RemainingDistance = 999;
        }
    }

    public void SetTeam(int team)
    {
        NavAgent.avoidancePriority = 0 + team;
    }

    private async void DelayToRenderComplete(Vector3 destination)
    {
        if (NavAgent.isOnNavMesh)
        {
            NavAgent.isStopped = true;
        }

        NavAgent.SetDestination(destination);

        while (NavAgent.pathPending)
        {
            await Task.Yield();
        }

        _frameCountDownDelta = FrameCountDownSet;
        if (NavAgent.isOnNavMesh)
        {
            NavAgent.isStopped = false;
        }
    }


    public void Move(float3 velocity)
    {
        if (!velocity.Equals(float3.zero))
        {
            Debug.Log($"Index : {Index} | velocity : {velocity}");
            Debug.DrawRay(transform.position, math.normalize(velocity) * 2f, Color.red);
        }

        NavAgent.Move(velocity);

        // if (Paths.Count > 0)
        // {
        //     Vector3 vt = Paths[0];
        //     transform.rotation = Quaternion.LookRotation(vt - transform.position);
        //     if (math.length((float3)transform.position - Paths[0]) <= ReachDistance)
        //     {
        //         Paths.RemoveAt(0);
        //     }
        //
        //     if (Paths.Count > 0 && _frameCountDownDelta > 0)
        //     {
        //         _frameCountDownDelta--;
        //         if (_frameCountDownDelta == 0)
        //         {
        //             // NavAgent.SetDestination(Paths[0]);
        //             _frameCountDownDelta = FrameCountDownSet;
        //         }
        //     }
        // }
    }
}