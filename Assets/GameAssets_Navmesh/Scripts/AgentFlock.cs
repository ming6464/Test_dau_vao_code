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
    public Vector3 forward;
    public float radiusCheckFieldOfView;
    public float fieldOfView;
    public float densityDraw;

    public Color colorStop;
    public Color colorRun;
    private Color myColor;
    private Vector3 Destination;

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
        Destination = (Vector3)obj;
        DelayToRenderComplete(Destination);
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

        try
        {
            forward = NavAgent.steeringTarget - myTrans.position;
        }
        catch
        {
            //ignored
        }

        if (NavAgent.hasPath && Vector3.Distance(Destination, transform.position) <= ReachDistance)
        {
            _flockManager.EditPriority(false, index, ID);
            NavAgent.ResetPath();
        }
        else
        {
            _flockManager.EditPriority(true, index, ID);
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
        DrawCircle(transform.position, radiusCheckFieldOfView, fieldOfView, forward, myColor, densityDraw);

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