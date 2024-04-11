using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AgentFlock : MonoBehaviour
{
    public int Index;
    public NavMeshAgent NavAgent;
    public float ReachDistance;

    private FlockManager _flockManager;
    private NativeArray<float3> Paths;

    private void Awake()
    {
        if (!NavAgent)
        {
            TryGetComponent(out NavAgent);
        }

        Index = -1;

        float a = Random.Range(-1f, 1f);

        transform.position += new Vector3(a, a, a);
    }

    private void OnDisable()
    {
        _flockManager.RemoveAgent(Index);
        try
        {
            Paths.Dispose();
        }
        catch
        {
            //ignore
        }
    }

    private void Start()
    {
        if (FlockManager.Instance)
        {
            _flockManager = FlockManager.Instance;
            _flockManager.AddAgent(this);
            Transform destinationTf = GameObject.FindGameObjectWithTag("Destination").transform;
            if (destinationTf)
            {
                Paths = new NativeArray<float3>(100, Allocator.Persistent);
                DelayToRenderComplete(destinationTf.position);
            }
        }
    }


    private async void DelayToRenderComplete(Vector3 destination)
    {
        NavMeshPath navPath = new NavMeshPath();
        NavAgent.CalculatePath(destination, navPath);
        while (NavAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            await Task.Yield();
        }


        for (int i = 1; i < navPath.corners.Length; i++)
        {
            Paths[i] = GetPathRange(navPath.corners[i], 1.2f);
        }

        //---method
        float3 GetPathRange(float3 origin, float radius)
        {
            float3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection.y = 0;
            randomDirection += origin;
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 10, NavMesh.AllAreas))
            {
                return hit.position;
            }


            return origin;
        }
    }


    public void Move(float3 velocity)
    {
        transform.rotation = Quaternion.LookRotation(velocity);
        NavAgent.Move(velocity);
        // if (Paths.Length > 0 && math.distance(transform.position, Paths[0]) <= ReachDistance)
        // {
        //     Paths.RemoveAt(0);
        // }
    }
}