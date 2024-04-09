using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class MoveAvoidObstacle : MonoBehaviour
{
    [Serializable]
    public struct ObstacleInfo
    {
        public Vector3 Position;
        public float Radius;
        public string Name;
    }

    public int Weight;
    public float Radius;
    public float Speed = 5f;
    public List<ObstacleInfo> ObstacleInfos;
    
    
    public bool UseNavMeshAgent;
    public NavMeshAgent NavMeshAgent;
    
    [Header("Color")]
    public Color MyColorRay;
    public Color MyColorArrow;
    public Vector3 Destination;
    public bool Run;
    public ObstacleInfo ObsAim;

    public List<ObstacleInfo> ObstacleInfosCollider;
    private bool _isSetAim;
    private Transform _myTransform;

    private void Awake()
    {
        ObstacleInfosCollider = new List<ObstacleInfo>();
        _myTransform = transform;
    }

    private void OnValidate()
    {
        ObstacleInfos = new List<ObstacleInfo>();
        transform.localScale = Vector3.one * (Radius * 2);
        foreach (Obstacle obs in FindObjectsOfType<Obstacle>())
        {
            ObstacleInfos.Add(new ObstacleInfo{Position = obs.transform.position,Radius = obs.Radius,Name = obs.Name});
        }

        NavMeshAgent = GetComponent<NavMeshAgent>();
    }
    

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Run = true;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Destination = hit.point;
                Destination.y = 0;
            }
            _isSetAim = false;
        }else if (Input.GetMouseButtonDown(1))
        {
            Run = false;
        }

        if (Run)
        {
            DrawLine(transform.position,Destination,MyColorRay,true);
            Vector3 safeDirection = GetSafeDirection(GetNextPosition());
            DrawRay(transform.position,safeDirection,Color.green,1,true);
            if (UseNavMeshAgent)
            {
                NavMeshAgent.Move(safeDirection * (Speed * Time.deltaTime));
            }
            else
            {
                transform.position += safeDirection * (Speed * Time.deltaTime);
            }
        }
    }

    
    private Vector3 GetSafeDirection(Vector3 newPosition)
    {
        int countDequy = 3;
        Vector3 Test(Vector3 safeDirection)
        {
            countDequy--;
            bool isSetAim = false;
            Vector3 nextPos = GetNextPosition(safeDirection);
            foreach (var obstacle in ObstacleInfos)
            {
                if (Vector3.Distance(obstacle.Position,nextPos) < Radius + obstacle.Radius)
                {
                    if (!_isSetAim)
                    {
                        ObsAim = obstacle;
                    }
                    else if(ObsAim.Position != obstacle.Position)
                    {
                        Vector3 nextPosToObsAim = ObsAim.Position - GetCurrentPosition();
                        Vector3 nextPosToNewObs = obstacle.Position - GetCurrentPosition();
                        float angle = Angle180Clockwise(nextPosToObsAim, nextPosToNewObs);
                        if (angle < 0)
                        {
                            AddListCollider(ObsAim);
                            ObsAim = obstacle;
                        }
                    }
                    
                    isSetAim = true;
                    _isSetAim = true;
                }
            }

            RemoveListCollider(ObsAim);
        
            if (isSetAim)
            {
                Vector3 pointCollider = GetPoint(new ObstacleInfo { Position = nextPos, Radius = Radius }, ObsAim,true);
                pointCollider.y = ObsAim.Position.y;
                Vector3 obsToPoint = (pointCollider - ObsAim.Position);

                float add = ObsAim.Radius * Weight * 1.0f / Radius;
                if (add < Weight) add = Weight;
                
                nextPos = ObsAim.Position + obsToPoint.normalized * (Radius + ObsAim.Radius + add);

                safeDirection = nextPos - GetCurrentPosition();
                if (countDequy < 0)
                {
                    return safeDirection;
                }
                return Test(safeDirection);
            }

            if (_isSetAim)
            {
                bool isCutCircle = false;
                foreach(ObstacleInfo obs in ObstacleInfosCollider)
                {
                    if (IsLineIntersectCircle(obs.Position, obs.Radius + Radius, nextPos,
                            Destination))
                    {
                        isCutCircle = true;
                        break;
                    }
                }

                if (!isCutCircle)
                {
                    if (Mathf.Abs(Angle180Clockwise(ObsAim.Position - nextPos, Destination - nextPos)) < 90)
                    {
                        isCutCircle = true;
                    }
                }
                
                if (isCutCircle)
                {
                    nextPos = (GetCurrentPosition() - ObsAim.Position).normalized * ObsAim.Radius + ObsAim.Position;
                    Vector3 pointCollider = GetPoint(new ObstacleInfo { Position = nextPos, Radius = Radius }, ObsAim,true);
                    pointCollider.y = ObsAim.Position.y;
                    Vector3 obsToPoint = (pointCollider - ObsAim.Position);
                    
                    float add = ObsAim.Radius * Weight * 1.0f / Radius;
                    if (add < Weight) add = Weight;
                    
                    nextPos = ObsAim.Position + obsToPoint.normalized * (Radius + ObsAim.Radius + add);
                    
                    safeDirection = nextPos - GetCurrentPosition();
                    return safeDirection;
                }
                _isSetAim = false;
                ObstacleInfosCollider.Clear();
            }
            
            return safeDirection;
        }
        
        Vector3 GetPoint(ObstacleInfo Circle1,ObstacleInfo Circle2,bool GetHigher)
        {
            Vector3 centerA = Circle1.Position;
            Vector3 centerB = Circle2.Position;
            float radiusA = Circle1.Radius;
            float radiusB = Circle2.Radius;
            if (Vector3.Distance(centerA, centerB) < (radiusB + radiusA))
            {
                Vector3 pointC, pointD;
                CalculateCircleIntersection(centerA, radiusA, centerB, radiusB, out pointC, out pointD);
                
                Vector3 AToB = centerB - centerA;
                Vector3 AToC = pointC - centerA;
                bool C_Higher = Angle180Clockwise(AToB, AToC) < 0;
                if (C_Higher == GetHigher)
                {
                    return pointC;
                }

                return pointD;
            }

            return Vector3.zero;
        }
        return Test(newPosition - transform.position).normalized;
    }
    
    #region SUPPORT
    
    private bool IsLineIntersectCircle(Vector3 O, float R, Vector3 A, Vector3 B)
    {
        Vector2 point1 = new Vector2(A.x, A.z);
        Vector2 point2 = new Vector2(B.x, B.z);
        Vector2 point3 = new Vector2(O.x, O.z);

        float d = Mathf.Abs(
            ((point2.x - point1.x) * (point1.y - point3.y) - (point1.x - point3.x) * (point2.y - point1.y)) /
            (Vector2.Distance(point1, point2)));

        if (d < R)
        {
            return true;
        }

        return false;
    }

    private void AddListCollider(ObstacleInfo obs)
    {
        if(String.IsNullOrEmpty(obs.Name)) return;
        int index = ObstacleInfosCollider.FindIndex(x => x.Name == obs.Name);
        if (index < 0)
        {
            ObstacleInfosCollider.Add(obs);
        }
    }

    private void RemoveListCollider(ObstacleInfo obs)
    {
        if(String.IsNullOrEmpty(obs.Name)) return;
        int index = ObstacleInfosCollider.FindIndex(x => x.Name == obs.Name);
        if (index >= 0)
        {
            ObstacleInfosCollider.RemoveAt(index);
        }
    }
    
    private bool CalculateCircleIntersection(Vector3 center1, float radius1, Vector3 center2, float radius2, out Vector3 intersection1, out Vector3 intersection2)
    {
        float distance = Vector3.Distance(center1, center2);
        if (distance > radius1 + radius2 || distance < Mathf.Abs(radius1 - radius2))
        {
            intersection1 = Vector3.zero;
            intersection2 = Vector3.zero;
            return false;
        }
        
        float a = (radius1 * radius1 - radius2 * radius2 + distance * distance) / (2 * distance);
        float h = Mathf.Sqrt(radius1 * radius1 - a * a);
        Vector3 p2 = center1 + (center2 - center1) * (a / distance);
        intersection1 = new Vector3(p2.x + h * (center2.z - center1.z) / distance,0, p2.z - h * (center2.x - center1.x) / distance);
        intersection2 = new Vector3(p2.x - h * (center2.z - center1.z) / distance,0, p2.z + h * (center2.x - center1.x) / distance);
        return true;
    }
    
    
    private float GetDistanceToRectangle(Transform rectangle, float width, float height, Transform point,out Vector3 pointForce)
    {
        float x = width / 2.0f;
        float y = height / 2.0f;
        Vector3 pointInvertToLocalRectangle = rectangle.InverseTransformPoint(point.position);
        pointForce = rectangle.TransformPoint(new Vector3(Mathf.Clamp(pointInvertToLocalRectangle.x, -x, x), 0, Mathf.Clamp(pointInvertToLocalRectangle.z, -y, y)));
        //----------------
        Vector3 absPoint = new Vector3(Mathf.Abs(pointInvertToLocalRectangle.x), Mathf.Abs(pointInvertToLocalRectangle.y), Mathf.Abs(pointInvertToLocalRectangle.z));
        return Mathf.Sqrt(Mathf.Pow(Mathf.Max(absPoint.x - x,0), 2) + Mathf.Pow(Mathf.Max(absPoint.z - y,0), 2));
    }
    
    private float Angle360AxisXClockwise(Vector3 dir)
    {
        float angle = Vector2.SignedAngle(Vector2.right, new Vector2(dir.x, dir.z));
        if (angle < 0)
        {
            angle += 360;
        }
        return angle;
    }
    
    private float Angle360Clockwise(Vector3 start,Vector3 dir)
    {
        float angle = Angle180Clockwise(start, dir);
        if (angle < 0)
        {
            angle += 360;
        }
        return angle;
    }
    
    private float Angle180Clockwise(Vector3 start,Vector3 dir)
    {
        return Vector2.SignedAngle(new Vector2(start.x, start.z), new Vector2(dir.x, dir.z));
    }
    

    private Vector3 GetDirectToDestination()
    {
        return Destination - _myTransform.position;
    }

    private Vector3 GetNextPosition()
    {
        return GetDirectToDestination().normalized * (Speed * Time.deltaTime) + GetCurrentPosition();
    }

    private Vector3 GetNextPosition(Vector3 direct)
    {
          return direct.normalized * (Speed * Time.deltaTime) + GetCurrentPosition();
    }

    private Vector3 GetCurrentPosition()
    {
        return _myTransform.position;
    }

    private Vector3 GetDirectToNextPosition()
    {
        return GetNextPosition() - GetCurrentPosition();
    }

    private void DrawRay(Vector3 startPos,Vector3 direct,Color color,float length = 1,bool arrow = false)
    {
        Debug.DrawRay(startPos,direct * length,color);
        if (arrow)
        {
            Vector3 dir1 = (Quaternion.Euler(0,45,0) * -direct).normalized * .5f;
            Vector3 dir2 = (Quaternion.Euler(0,-45,0) * -direct).normalized * .5f;
            Vector3 startPos2 = startPos + direct;
            DrawRay(startPos2,dir1,MyColorArrow);
            DrawRay(startPos2,dir2,MyColorArrow);
        }
    }

    private void DrawLine(Vector3 startPos, Vector3 endPos, Color color,bool arrow = false,float time = 0f)
    {
        
        if (time > 0)
        {
            Debug.DrawLine(startPos,endPos,color,time);
        }
        else
        {
            Debug.DrawLine(startPos,endPos,color);
        }
        if (arrow)
        {
            Vector3 direct = endPos - startPos;
            Vector3 dir1 = (Quaternion.Euler(0,45,0) * -direct).normalized * .5f;
            Vector3 dir2 = (Quaternion.Euler(0,-45,0) * -direct).normalized * .5f;
            Vector3 startPos2 = startPos + direct;
            DrawRay(startPos2,dir1,MyColorArrow);
            DrawRay(startPos2,dir2,MyColorArrow);
        }
    }

    
    #endregion
}