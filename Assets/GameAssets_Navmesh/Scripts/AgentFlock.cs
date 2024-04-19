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

    public float radiusCheckFieldOfView;
    public float fieldOfView;
    public float densityDraw;

    public Color colorStop;
    public Color colorRun;
    private Color myColor;

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
        myColor = colorRun;
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
            // for (int i = 0; i < NavAgent.path.corners.Length - 1; i++)
            // {
            //     Debug.DrawLine(NavAgent.path.corners[i + 1], NavAgent.path.corners[i], Color.green);
            // }

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

    public void HandleStop(bool isStop)
    {
        if (NavAgent.isOnNavMesh)
        {
            NavAgent.isStopped = isStop;
        }

        myColor = isStop ? colorStop : colorRun;
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

    private void OnDrawGizmos()
    {
        try
        {
            Gizmos.color = myColor;
            Vector3 vt1 = transform.forward * radiusCheckFieldOfView;
            Vector3 vt2 = (Quaternion.Euler(0, fieldOfView / 2f, 0) * vt1);
            Vector3 vt3 = (Quaternion.Euler(0, -fieldOfView / 2f, 0) * vt1);
            Vector3 pos1 = vt1 + transform.position;
            Vector3 pos2 = vt2 + transform.position;
            Vector3 pos3 = vt3 + transform.position;
            float dotLength = math.abs(Vector3.Dot(vt2, vt1.normalized));

            Gizmos.DrawLine(transform.position, pos2);
            Gizmos.DrawLine(transform.position, pos3);
            float add = (radiusCheckFieldOfView - dotLength) / densityDraw;
            Vector3 pos4 = pos2, pos5 = pos3;
            for (float i = dotLength + add; i < radiusCheckFieldOfView; i += add)
            {
                // Tính cos(góc giữa c và a)
                float cosTheta = i / radiusCheckFieldOfView;
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
}