using System;
using System.Collections.Generic;
using System.Linq;
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

    public float Speed;
    public NavMeshAgent NavAgent;
    public float ReachDistance;
    public int FrameCountDownSet;
    public float RemainingDistance;
    public Vector3 Forward;
    public bool InTimeAvoid;

    private FlockManager _flockManager;
    private List<Vector3> Paths;
    private bool CalculatePath;
    private int _frameCountDownDelta;
    [SerializeField] private int _index;

    private void Awake()
    {
        if (!NavAgent)
        {
            TryGetComponent(out NavAgent);
        }

        Paths = new List<Vector3>();
        _frameCountDownDelta = -1;
        CalculatePath = false;
        Index = -1;
    }

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.RightClick, OnRightClick);
        EventDispatcher.Instance.RegisterListener(EventID.LeftClick, OnLeftClick);
        InTimeAvoid = true;
        Forward = transform.forward;
    }

    private void OnLeftClick(object obj)
    {
        if (obj == null) return;
        InTimeAvoid = true;
        Debug.Log($"Right Click {Index}");
        OnRightClick(null);
        DelayToRenderComplete((Vector3)obj);
        if (NavAgent.isOnNavMesh)
        {
            NavAgent.speed = Speed;
        }

        AwaitResetState();
    }

    private async void AwaitResetState()
    {
        await Task.Delay(300);
        if (NavAgent.isOnNavMesh)
        {
            NavAgent.speed = 0;
        }

        InTimeAvoid = false;
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

        try
        {
            Forward = NavAgent.path.corners[1] - transform.position;
            Forward.Normalize();
        }
        catch
        {
            Forward = transform.forward;
        }
    }


    private async void DelayToRenderComplete(Vector3 destination)
    {
        if (NavAgent.isOnNavMesh)
        {
            NavAgent.isStopped = true;
        }

        NavMeshPath path = new NavMeshPath();
        NavAgent.CalculatePath(destination, path);

        while (NavAgent.pathPending)
        {
            await Task.Yield();
        }

        Paths = path.corners.ToList();
        if (Paths.Count > 0)
        {
            Paths.RemoveAt(0);
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

        if (!velocity.Equals(default) && !velocity.Equals(float3.zero))
        {
            Vector3 vt = velocity;

            NavAgent.Move(velocity);
            transform.rotation = Quaternion.LookRotation(velocity);
        }
    }


    public void Mov2(float3 velocity)
    {
        if (!velocity.Equals(default) && !velocity.Equals(float3.zero))
        {
            Debug.Log($"Index : {Index} | velocity : {velocity}");
            Debug.DrawRay(transform.position, math.normalize(velocity) * 2f, Color.red);
            Vector3 vt = velocity;
            vt = vt.normalized * Speed * Time.deltaTime;
            NavAgent.Move(vt);
            transform.rotation = Quaternion.LookRotation(vt);
        }
    }
}