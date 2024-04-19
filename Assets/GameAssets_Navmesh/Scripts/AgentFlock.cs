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
    public int index;

    public NavMeshAgent NavAgent;
    public float ReachDistance;
    public float RemainingDistance;
    public int ID;
    private FlockManager _flockManager;
    private bool CalculatePath;
    public Transform myTrans;
    public float Radius;
    
    private void Awake()
    {
        if (!NavAgent)
        {
            TryGetComponent(out NavAgent);
        }

        CalculatePath = false;
        index = -1;
        myTrans = transform;
        ID = myTrans.GetInstanceID();
    }

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.RightClick, OnRightClick);
        EventDispatcher.Instance.RegisterListener(EventID.LeftClick, OnLeftClick);
    }

    private void OnLeftClick(object obj)
    {
        if (obj == null) return;
        OnRightClick(null);
        DelayToRenderComplete((Vector3)obj);
    }

    private void OnRightClick(object obj)
    {
        NavAgent.ResetPath();
    }

    private void OnDisable()
    {
        _flockManager.RemoveAgent(index, GetInstanceID());
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
            _flockManager.AddAgent(this, index, GetInstanceID());
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

        if (NavAgent.isOnNavMesh)
        {
            NavAgent.isStopped = false;
        }
    }


    public void OnAvoidNeighbors(float3 velocity)
    {
        if (!velocity.Equals(float3.zero))
        {
            Debug.DrawRay(transform.position, math.normalize(velocity) * 2f, Color.red);
        }

        NavAgent.Move(velocity);
    }
}