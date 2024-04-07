using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MoveAvoidObstacle : MonoBehaviour
{
    [Serializable]
    public struct ObstacleInfo
    {
        public Vector3 Position;
        public float Radius;
    }
    public float Radius;
    public float Speed = 5f;
    public float MaxWeight;
    public List<ObstacleInfo> ObstacleInfos;
    
    
    public bool UseNavMeshAgent;
    public float AvoidDistance;
    public NavMeshAgent NavMeshAgent;
    public float RayLength;
    public bool CanMove;
    public bool CanMoveOnDetectCollider;
    
    [Header("Color")]
    public Color[] Colors;

    public Color MyColorRay;
    public Color MyColorArrow;
    public int NumberCollider;
    public Vector3 Destination;
    public bool Run;
    public float Dir;
    public ObstacleInfo ObsAim;

    private Vector3 _positionSetAim;
    private bool _isSetAim;
    private float _avoidDistance;
    private bool _canMove;
    private Transform _myTransform;

    private void Awake()
    {
        _myTransform = transform;
    }

    private void OnValidate()
    {
        _avoidDistance = Radius + AvoidDistance;
        ObstacleInfos = new List<ObstacleInfo>();
        transform.localScale = Vector3.one * (Radius * 2);
        foreach (Obstacle obs in FindObjectsOfType<Obstacle>())
        {
            ObstacleInfos.Add(new ObstacleInfo{Position = obs.transform.position,Radius = obs.Radius});
        }

        NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _avoidDistance = Radius + AvoidDistance;
    }


    private void Update()
    {
        NumberCollider = 0;

        if (Input.GetMouseButtonDown(0))
        {
            Run = true;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Destination = hit.point;
                Dir = Angle360AxisXClockwise(Destination) > 180 ? 1 : -1;
            }

            _isSetAim = false;
            _positionSetAim = GetNextPosition();
        }else if (Input.GetMouseButtonDown(1))
        {
            Run = false;
        }

        if (Run)
        {
            _canMove = CanMove;
            DrawLine(transform.position,Destination,MyColorRay);
            Vector3 safeDirection = GetSafeDirection2();
        
            if (_canMove)
            {
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
    }
    
    
    
    private Vector3 GetSafeDirection2()
    {
        Vector3 nextPosition = GetNextPosition();
        Vector3 safeDirection = GetDirectToNextPosition();
        Vector3 positionSetAim = Vector3.zero;
        bool IsSetAim = false;

        foreach (ObstacleInfo obstacle in ObstacleInfos)
        {
            Vector3 toObstacle = obstacle.Position - nextPosition;
            
            if (toObstacle.magnitude < (_avoidDistance + obstacle.Radius))
            {
                Vector3 dir = -toObstacle.normalized * ((_avoidDistance + obstacle.Radius - toObstacle.magnitude) * MaxWeight);
                safeDirection += dir;
                _canMove = CanMoveOnDetectCollider;
                Vector3 positionSetAim_ = obstacle.Position + (-toObstacle.normalized * obstacle.Radius);
                NumberCollider++;
                
                if (!IsSetAim)
                {
                    IsSetAim = true;
                    ObsAim = obstacle;
                    positionSetAim = positionSetAim_;
                }
                else
                {
                    bool isLeft = positionSetAim_.x < positionSetAim.x;
                    if ((Dir < 0 && isLeft) || (Dir > 0 && !isLeft))
                    {
                        positionSetAim = positionSetAim_;
                        ObsAim = obstacle;
                    }
                }
            }
        }

        bool IsCollider = false;

        nextPosition = GetNextPosition(safeDirection);
        
        foreach (ObstacleInfo obstacle in ObstacleInfos)
        {
            Vector3 toObstacle = obstacle.Position - nextPosition;
            
            if (toObstacle.magnitude < (_avoidDistance + obstacle.Radius))
            {
                IsCollider = true;
                break;
            }
        }
        
        if (IsCollider)
        {
            
        }
        
        return safeDirection.normalized;
    }

    private Vector3 GetSafeDirection3(Vector3 nextPosition, Vector3 direct)
    {
        Vector3 safeDirection = direct;

        if (_isSetAim)
        {
            
        }
        else
        {
            foreach (ObstacleInfo obstacle in ObstacleInfos)
            {
                Vector3 toObstacle = obstacle.Position - nextPosition;
            
                if (toObstacle.magnitude < (_avoidDistance + obstacle.Radius))
                {
                    safeDirection -= toObstacle.normalized * ((_avoidDistance + obstacle.Radius - toObstacle.magnitude) * MaxWeight);
                    Vector3 positionSetAim_ = obstacle.Position + (-toObstacle.normalized * obstacle.Radius);

                    if (!_isSetAim)
                    {
                        _isSetAim = true;
                        _positionSetAim = positionSetAim_;
                        ObsAim = obstacle;
                    }
                    else
                    {
                        bool isLeft;
                        float Angle1 = Angle360AxisXClockwise(nextPosition, _positionSetAim);
                        float Angle2 = Angle360AxisXClockwise(nextPosition, positionSetAim_);
                        if (Angle1 > 180)
                        {
                            isLeft = false;
                        }
                        else
                        {
                            if (Angle2 > 180) Angle2 = 180 - Angle2;
                            isLeft = Angle2 > Angle1;
                        }
                
                        if ((Dir < 0 && isLeft) || (Dir > 0 && !isLeft))
                        {
                            _positionSetAim = positionSetAim_;
                            ObsAim = obstacle;
                        }
                    }
                }
            }
        }

        

        bool IsCollider = false;

        nextPosition = GetNextPosition(safeDirection);
        
        foreach (ObstacleInfo obstacle in ObstacleInfos)
        {
            Vector3 toObstacle = obstacle.Position - nextPosition;
            
            if (toObstacle.magnitude < (_avoidDistance + obstacle.Radius))
            {
                IsCollider = true;
                break;
            }
        }
        
        if (IsCollider)
        {
            GetSafeDirection3(nextPosition, nextPosition - GetCurrentPosition());
        }
        
        return safeDirection.normalized;
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
    
    private float Angle360AxisXClockwise(Vector3 start,Vector3 dir)
    {
        float angle = Vector2.SignedAngle(new Vector2(start.x, start.z), new Vector2(dir.x, dir.z));
        if (angle < 0)
        {
            angle += 360;
        }
        return angle;
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
            Vector3 dir1 = (Quaternion.Euler(0,45,0) * -direct).normalized * .03f;
            Vector3 dir2 = (Quaternion.Euler(0,-45,0) * -direct).normalized * .03f;
            Vector3 startPos2 = startPos + direct;
            DrawRay(startPos2,dir1,MyColorArrow);
            DrawRay(startPos2,dir2,MyColorArrow);
        }
    }

    private void DrawLine(Vector3 startPos, Vector3 endPos, Color color)
    {
        Debug.DrawLine(startPos,endPos,color);
    }
    
    

    private Vector3 GetSafeDirection(Vector3 newPosition)
    {
        Color color;
        int index = 0;
        Vector3 safeDirection = (newPosition - transform.position).normalized;
        Vector3 pos = newPosition;
        foreach (var obstacle in ObstacleInfos)
        {
            Vector3 toObstacle = obstacle.Position - newPosition;

            // Kiểm tra nếu khoảng cách lớn hơn tổng bán kính của player và obstacle
            if (toObstacle.magnitude < _avoidDistance + obstacle.Radius)
            {
                if (index >= Colors.Length)
                {
                    index = 0;
                }

                
                color = Colors[index];
                Vector3 dir = -toObstacle.normalized *
                              (Mathf.Clamp01((_avoidDistance + obstacle.Radius - toObstacle.magnitude) /
                                             _avoidDistance) * MaxWeight);
                // Nếu không an toàn, điều chỉnh hướng di chuyển
                safeDirection += dir;
                DrawLine(transform.position,pos ,color);
                DrawRay(pos,dir,color,arrow:true);
                pos = transform.position + safeDirection;
                DrawLine(transform.position,pos ,color);
                _canMove = CanMoveOnDetectCollider;
                index++;
            }
        }
        return safeDirection.normalized;
    }
    
    private Vector3 GetSafeDirection1(Vector3 newPosition)
    {
        Vector3 nextPosition = newPosition;
        Vector3 safeDirection = nextPosition - transform.position;
        Color color;
        foreach (ObstacleInfo obstacle in ObstacleInfos)
        {
            Vector3 toObstacle = obstacle.Position - nextPosition;
            
            if (toObstacle.magnitude < (_avoidDistance + obstacle.Radius))
            {
                color = Colors[NumberCollider % Colors.Length];
                
                Vector3 dir = -toObstacle.normalized * ((_avoidDistance + obstacle.Radius - toObstacle.magnitude) * MaxWeight);
                safeDirection += dir;
                DrawLine(transform.position,nextPosition ,color);
                DrawRay(nextPosition,dir,color,arrow:true);
                nextPosition += dir;
                DrawLine(transform.position, nextPosition,color);
                _canMove = CanMoveOnDetectCollider;
                NumberCollider++;
            }
        }
        return safeDirection.normalized;
    }
    
}