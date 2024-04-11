using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class MoveAvoidRectangleObstacle : MonoBehaviour
{
    [Serializable]
    public struct RectangleObstacleInfo
    {
        public Obstacle Obstacle;
        public Vector3 PointForce;
        public float Distance;
        public float AngleThrust;
    }

    public int Weight;
    public float Radius;
    public float Speed = 5f;
    public List<Obstacle> ObstacleInfos;
    [Header("Color")] public Vector3 Destination;
    [Space(10)] public bool Run;
    public RectangleObstacleInfo ObsAim;
    public List<RectangleObstacleInfo> ObstacleInfosCollider;
    [Space(10)] public int CheckFrame;
    private bool _isSetAim;
    private Transform _myTransform;
    private int _checkFrame;
    private Vector3 _passVt;
    private Obstacle _passObs;
    [Header("Test Choose")] public float _directChoose;

    private void Awake()
    {
        ObstacleInfosCollider = new List<RectangleObstacleInfo>();
        _myTransform = transform;
    }

    private void OnValidate()
    {
        ObstacleInfos = FindObjectsOfType<Obstacle>().ToList().FindAll(x => x.Type == TypePoly.Rectangle);
        transform.localScale = Vector3.one * (Radius * 2);
    }


    private void Update()
    {
        DrawWeight();
        if (Input.GetMouseButtonDown(0))
        {
            Run = true;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Destination = hit.point;
                Destination.y = 0;
                int NumbersClockwise = 0;
                Vector3 curPos = GetCurrentPosition();
                Vector3 curPosToDestination = Destination - curPos;
                foreach (Obstacle obs in ObstacleInfos)
                {
                    if (Angle180Clockwise(curPosToDestination, obs.Position - curPos) < 0)
                    {
                        NumbersClockwise++;
                    }
                    else
                    {
                        NumbersClockwise--;
                    }
                }

                _directChoose = NumbersClockwise > 0 ? -1 : 1;
            }

            _isSetAim = false;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Run = false;
        }

        if (Run)
        {
            DrawLine(transform.position, Destination, Color.blue, true);
            Vector3 safeDirection = GetSafeDirection3(GetNextPosition());
            DrawRay(transform.position, safeDirection, Color.green, 1, true);
            transform.position += safeDirection * (Speed * Time.deltaTime);
        }
    }

    private void DrawWeight()
    {
        if (Weight <= 0) return;
        Vector3 point1;
        Vector3 point2;
        Vector3 point3;
        Vector3 point4;
        foreach (Obstacle obs in ObstacleInfos)
        {
            switch (obs.Type)
            {
                case TypePoly.Circle:
                    break;
                case TypePoly.Rectangle:
                    float X = obs.Width / 2f;
                    float Y = obs.Height / 2f;
                    Transform obsTf = obs.transform;
                    point1 = obsTf.TransformPoint(new Vector3(-X - Weight, 0, -Y - Weight));
                    point2 = obsTf.TransformPoint(new Vector3(-X - Weight, 0, Y + Weight));
                    point3 = obsTf.TransformPoint(new Vector3(X + Weight, 0, Y + Weight));
                    point4 = obsTf.TransformPoint(new Vector3(X + Weight, 0, -Y - Weight));

                    Debug.DrawLine(point1, point2, Color.green);
                    Debug.DrawLine(point2, point3, Color.green);
                    Debug.DrawLine(point3, point4, Color.green);
                    Debug.DrawLine(point4, point1, Color.green);

                    break;
            }
        }
    }

    private bool isOverrideDir = false;
    private int overrideDir = 1;
    private Vector3 positionPass;

    private Vector3 GetSafeDirection1(Vector3 newPosition)
    {
        int countDequy = 3;

        Vector3 Test(Vector3 safeDirection)
        {
            bool isSetAim = false;
            countDequy--;
            Vector3 nextPos = GetNextPosition(safeDirection);
            float d;
            float distanceObsAim = 0;
            ObstacleInfosCollider.Clear();

            foreach (var obstacle in ObstacleInfos)
            {
                d = GetDistanceToRectangle(obstacle.transform, obstacle.Width, obstacle.Height, nextPos,
                    out Vector3 pointForce);
                if (d < (Radius + Weight))
                {
                    float AngleThrust = Mathf.Clamp(1 - ((d - Radius) / Weight), 0f, 1f) * 90;
                    RectangleObstacleInfo obsInfo = new RectangleObstacleInfo
                        { Obstacle = obstacle, PointForce = pointForce, Distance = d, AngleThrust = AngleThrust };
                    if (ObstacleInfosCollider.Count == 0)
                    {
                        distanceObsAim = Vector3.Distance(pointForce, Destination);
                        ObsAim = obsInfo;
                    }
                    else
                    {
                        d = Vector3.Distance(pointForce, Destination);
                        if (d < distanceObsAim)
                        {
                            distanceObsAim = d;
                            ObsAim = obsInfo;
                        }
                    }

                    AddListCollider(obsInfo);
                    isSetAim = true;
                    _isSetAim = true;
                }
            }

            if (isSetAim)
            {
                RemoveListCollider(ObsAim);
                Vector3 curPosToPoint = ObsAim.PointForce - GetCurrentPosition();
                int dir = Angle180Clockwise(curPosToPoint, safeDirection) > 0 ? -1 : 1;
                Vector3 vtDir = Quaternion.Euler(0, ObsAim.AngleThrust * dir, 0) * curPosToPoint;
                DrawRay(GetCurrentPosition(), vtDir.normalized * 2f, Color.yellow, arrow: true);

                if (Mathf.Abs(Angle180Clockwise(-curPosToPoint, vtDir)) <
                    Mathf.Abs(Angle180Clockwise(-curPosToPoint, safeDirection)))
                {
                    safeDirection = vtDir;
                }

                foreach (RectangleObstacleInfo info in ObstacleInfosCollider)
                {
                    if (info.AngleThrust != 90) continue;
                    curPosToPoint = info.PointForce - GetCurrentPosition();

                    if (Mathf.Abs(Angle180Clockwise(-curPosToPoint, safeDirection)) > 90)
                    {
                        dir = Angle180Clockwise(curPosToPoint, safeDirection) > 0 ? -1 : 1;
                        safeDirection = Quaternion.Euler(0, 90 * dir, 0) * curPosToPoint;
                    }
                }
            }
            else
            {
                _isSetAim = false;
            }

            // bool check = false;
            //
            // if (AngleThrust == 90)
            // {
            //     if (_passVt == Vector3.zero)
            //     {
            //         _checkFrame = CheckFrame;
            //         _passObs = ObsAim;
            //         isOverrideDir = false;
            //         check = true;
            //     }else if (ObsAim == _passObs)
            //     {
            //         check = true;
            //         float angle = Mathf.Abs(Angle180Clockwise(_passVt.normalized, vtDir.normalized));
            //         if(_checkFrame > 0 && (180f - angle) <= 30f)
            //         {
            //             _checkFrame--;
            //             if (_checkFrame == 0)
            //             {
            //                 _passVt = Vector3.zero;
            //                 isOverrideDir = true;
            //                 overrideDir = dir;
            //             }
            //         }
            //     }
            // }
            //
            // if(!check)
            // {
            //     isOverrideDir = false;
            //     _passVt = Vector3.zero;
            // }
            // else
            // {
            //     _passVt = vtDir.normalized;
            // }

            return safeDirection;
        }

        return Test(newPosition - transform.position).normalized;
    }


    [Header("Test Dirrect Override")] public bool IsOverrideDirection;

    public int DirrectionOverride;

    private Vector3 GetSafeDirection2(Vector3 newPosition)
    {
        Vector3 Test(Vector3 safeDirection)
        {
            bool isSetAim = false;
            Vector3 nextPos = GetNextPosition(safeDirection);
            float d;
            float distanceObsAim = 0;
            ObstacleInfosCollider.Clear();
            foreach (var obstacle in ObstacleInfos)
            {
                d = GetDistanceToRectangle(obstacle.transform, obstacle.Width, obstacle.Height, nextPos,
                    out Vector3 pointForce);
                if (d < (Radius + Weight))
                {
                    float AngleThrust = Mathf.Clamp(1 - ((d - Radius) / Weight), 0f, 1f) * 90;
                    RectangleObstacleInfo obsInfo = new RectangleObstacleInfo
                        { Obstacle = obstacle, PointForce = pointForce, Distance = d, AngleThrust = AngleThrust };

                    DrawLine(pointForce, pointForce + Vector3.up * 10f, Color.black);
                    if (ObstacleInfosCollider.Count == 0)
                    {
                        // distanceObsAim = Vector3.Distance(pointForce, Destination);
                        ObsAim = obsInfo;
                    }
                    else
                    {
                        // d = Vector3.Distance(pointForce, Destination);
                        // if (d < distanceObsAim)
                        // {
                        //     distanceObsAim = d;
                        //     ObsAim = obsInfo;
                        // }

                        Vector3 posToObsAim = ObsAim.PointForce - nextPos;
                        Vector3 posToObsSet = obsInfo.PointForce - nextPos;
                        if (Angle180Clockwise(posToObsAim, posToObsSet) * _directChoose < 0)
                        {
                            ObsAim = obsInfo;
                        }
                    }

                    AddListCollider(obsInfo);
                    isSetAim = true;
                    _isSetAim = true;
                }
            }

            if (isSetAim)
            {
                DrawLine(ObsAim.Obstacle.Position, ObsAim.Obstacle.Position + Vector3.up * 10, Color.red);
                DrawRay(GetCurrentPosition(), (ObsAim.Obstacle.Position - GetCurrentPosition()).normalized, Color.red,
                    10f);
                RemoveListCollider(ObsAim);
                Vector3 curPosToPoint = ObsAim.PointForce - GetCurrentPosition();
                Vector3 curPosToRectangle = ObsAim.Obstacle.Position - GetCurrentPosition();
                Vector3 curPosToDestination = Destination - GetCurrentPosition();
                // int dir = Angle180Clockwise(curPosToPoint, safeDirection) > 0 ? -1 : 1;
                int dir = Angle180Clockwise(curPosToRectangle, curPosToDestination) < 0 ? 1 : -1;

                if (dir == DirrectionOverride)
                {
                    IsOverrideDirection = false;
                }

                Vector3 vtDir;

                if (IsOverrideDirection)
                {
                    dir = DirrectionOverride;
                }

                vtDir = Quaternion.Euler(0, ObsAim.AngleThrust * dir, 0) * curPosToPoint;
                if (Mathf.Abs(Angle180Clockwise(-curPosToPoint, vtDir)) <
                    Mathf.Abs(Angle180Clockwise(-curPosToPoint, safeDirection)))
                {
                    safeDirection = vtDir;
                }

                DrawRay(GetCurrentPosition(), vtDir.normalized * 2f, Color.red, arrow: true);

                Vector3 curPosToPoint2 = Vector3.zero;
                int dir1 = 1;
                foreach (RectangleObstacleInfo info in ObstacleInfosCollider)
                {
                    DrawRay(GetCurrentPosition(), (info.Obstacle.Position - GetCurrentPosition()).normalized,
                        Color.black, 10f);
                    if (info.AngleThrust < 90) continue;
                    curPosToPoint2 = info.PointForce - GetCurrentPosition();

                    if (Mathf.Abs(Angle180Clockwise(-curPosToPoint2, safeDirection)) > 90)
                    {
                        curPosToRectangle = info.Obstacle.Position - GetCurrentPosition();
                        dir1 = Angle180Clockwise(curPosToRectangle, curPosToDestination) < 0 ? 1 : -1;
                        if (IsOverrideDirection)
                        {
                            dir1 = DirrectionOverride;
                        }

                        Vector3 vtRecommend = Quaternion.Euler(0, 90 * dir1, 0) * curPosToPoint2;
                        DrawRay(GetCurrentPosition(), vtRecommend.normalized * 2f, Color.black, arrow: true);
                        if (Mathf.Abs(Angle180Clockwise(-curPosToPoint.normalized, vtRecommend.normalized)) > 90)
                        {
                            IsOverrideDirection = true;
                            DirrectionOverride = -dir;
                        }
                        else
                        {
                            safeDirection = vtRecommend;
                        }
                    }
                }
            }
            else
            {
                _isSetAim = false;
            }

            // bool check = false;
            //
            // if (AngleThrust == 90)
            // {
            //     if (_passVt == Vector3.zero)
            //     {
            //         _checkFrame = CheckFrame;
            //         _passObs = ObsAim;
            //         isOverrideDir = false;
            //         check = true;
            //     }else if (ObsAim == _passObs)
            //     {
            //         check = true;
            //         float angle = Mathf.Abs(Angle180Clockwise(_passVt.normalized, vtDir.normalized));
            //         if(_checkFrame > 0 && (180f - angle) <= 30f)
            //         {
            //             _checkFrame--;
            //             if (_checkFrame == 0)
            //             {
            //                 _passVt = Vector3.zero;
            //                 isOverrideDir = true;
            //                 overrideDir = dir;
            //             }
            //         }
            //     }
            // }
            //
            // if(!check)
            // {
            //     isOverrideDir = false;
            //     _passVt = Vector3.zero;
            // }
            // else
            // {
            //     _passVt = vtDir.normalized;
            // }

            return safeDirection;
        }

        return Test(newPosition - transform.position).normalized;
    }

    private Vector3 GetSafeDirection3(Vector3 newPosition)
    {
        Vector3 Test(Vector3 safeDirection)
        {
            bool isSetAim = false;
            Vector3 nextPos = GetNextPosition(safeDirection);
            float d;
            float distanceObsAim = 0;
            ObstacleInfosCollider.Clear();
            foreach (var obstacle in ObstacleInfos)
            {
                d = GetDistanceToRectangle(obstacle.transform, obstacle.Width, obstacle.Height, nextPos,
                    out Vector3 pointForce);
                if (d < (Radius + Weight))
                {
                    float AngleThrust = Mathf.Clamp(1 - ((d - Radius) / Weight), 0f, 1f) * 90;
                    RectangleObstacleInfo obsInfo = new RectangleObstacleInfo
                        { Obstacle = obstacle, PointForce = pointForce, Distance = d, AngleThrust = AngleThrust };

                    DrawLine(pointForce, pointForce + Vector3.up * 10f, Color.black);
                    if (ObstacleInfosCollider.Count == 0)
                    {
                        // distanceObsAim = Vector3.Distance(pointForce, Destination);
                        ObsAim = obsInfo;
                    }
                    else
                    {
                        // d = Vector3.Distance(pointForce, Destination);
                        // if (d < distanceObsAim)
                        // {
                        //     distanceObsAim = d;
                        //     ObsAim = obsInfo;
                        // }

                        Vector3 posToObsAim = ObsAim.PointForce - nextPos;
                        Vector3 posToObsSet = obsInfo.PointForce - nextPos;
                        if (Angle180Clockwise(posToObsAim, posToObsSet) * _directChoose < 0)
                        {
                            ObsAim = obsInfo;
                        }
                    }

                    AddListCollider(obsInfo);
                    isSetAim = true;
                    _isSetAim = true;
                }
            }

            if (isSetAim)
            {
                DrawLine(ObsAim.Obstacle.Position, ObsAim.Obstacle.Position + Vector3.up * 10, Color.red);
                DrawRay(GetCurrentPosition(), (ObsAim.Obstacle.Position - GetCurrentPosition()).normalized, Color.red,
                    10f);
                RemoveListCollider(ObsAim);
                Vector3 curPosToPoint = ObsAim.PointForce - GetCurrentPosition();
                Vector3 curPosToRectangle = ObsAim.Obstacle.Position - GetCurrentPosition();
                Vector3 curPosToDestination = Destination - GetCurrentPosition();
                int dir = Angle180Clockwise(curPosToRectangle, curPosToDestination) < 0 ? 1 : -1;

                if (dir == DirrectionOverride)
                {
                    IsOverrideDirection = false;
                    _checkFrame = 0;
                }

                Vector3 vtDir;

                if (IsOverrideDirection)
                {
                    dir = DirrectionOverride;
                }

                vtDir = Quaternion.Euler(0, ObsAim.AngleThrust * dir, 0) * curPosToPoint;
                if (IsOverrideDirection || Mathf.Abs(Angle180Clockwise(-curPosToPoint, vtDir)) <
                    Mathf.Abs(Angle180Clockwise(-curPosToPoint, safeDirection)))
                {
                    safeDirection = vtDir;
                }

                DrawRay(GetCurrentPosition(), vtDir.normalized * 2f, Color.red, arrow: true);


                foreach (RectangleObstacleInfo info in ObstacleInfosCollider)
                {
                    DrawRay(GetCurrentPosition(), (info.Obstacle.Position - GetCurrentPosition()).normalized,
                        Color.black, 10f);
                    if (_checkFrame > 0)
                    {
                        _checkFrame--;
                        break;
                    }

                    if (Vector3.Distance(info.PointForce, ObsAim.PointForce) < 2f * Radius)
                    {
                        IsOverrideDirection = true;
                        safeDirection = Quaternion.Euler(0, 180, 0) * safeDirection;
                        DirrectionOverride = -dir;
                        dir = DirrectionOverride;
                        _checkFrame = CheckFrame;
                    }
                    else if (info.AngleThrust >= 85f)
                    {
                        curPosToRectangle = info.Obstacle.Position - GetCurrentPosition();
                        int dir1 = Angle180Clockwise(curPosToRectangle, curPosToDestination) < 0 ? 1 : -1;
                        if (IsOverrideDirection)
                        {
                            dir1 = -dir1;
                            if (DirrectionOverride != dir)
                            {
                                vtDir = Quaternion.Euler(0, 180, 0) * vtDir;
                            }
                        }

                        Vector3 curPosToPoint2 = info.PointForce - GetCurrentPosition();
                        Vector3 vtRecommend = Quaternion.Euler(0, info.AngleThrust * dir1, 0) * curPosToPoint2;
                        DrawRay(GetCurrentPosition(), vtRecommend.normalized, Color.black, arrow: true);
                        safeDirection = vtRecommend + vtDir;

                        // curPosToPoint2 = info.PointForce - GetCurrentPosition();
                        //
                        // if (Mathf.Abs(Angle180Clockwise(-curPosToPoint2, safeDirection)) > 90)
                        // {
                        //     dir1 = Angle180Clockwise(curPosToRectangle, curPosToDestination) < 0 ? 1 : -1;
                        //     // if (IsOverrideDirection)
                        //     // {
                        //     //     dir1 = DirrectionOverride;
                        //     // }
                        //     Vector3 vtRecommend = Quaternion.Euler(0, 90 * dir1, 0) * curPosToPoint2;
                        //     if (ObsAim.AngleThrust < 80)
                        //     {
                        //         Vector3 vt1 = Quaternion.Euler(0, 90 * dir, 0) * curPosToPoint;
                        //         safeDirection = vt1 + vtRecommend;
                        //     }
                        //     else
                        //     {
                        //     
                        //     }
                        //     curPosToRectangle = info.Obstacle.Position - GetCurrentPosition();
                        //     DrawRay(GetCurrentPosition(), vtRecommend.normalized * 2f, Color.black, arrow: true);
                        //     if (Mathf.Abs(Angle180Clockwise(-curPosToPoint.normalized, vtRecommend.normalized)) > 90)
                        //     {
                        //     
                        //         IsOverrideDirection = true;
                        //         DirrectionOverride = -dir;
                        //     }
                        //     else
                        //     {
                        //         safeDirection = vtRecommend;
                        //     }
                        // }
                    }
                }
            }
            else
            {
                _isSetAim = false;
            }

            // bool check = false;
            //
            // if (AngleThrust == 90)
            // {
            //     if (_passVt == Vector3.zero)
            //     {
            //         _checkFrame = CheckFrame;
            //         _passObs = ObsAim;
            //         isOverrideDir = false;
            //         check = true;
            //     }else if (ObsAim == _passObs)
            //     {
            //         check = true;
            //         float angle = Mathf.Abs(Angle180Clockwise(_passVt.normalized, vtDir.normalized));
            //         if(_checkFrame > 0 && (180f - angle) <= 30f)
            //         {
            //             _checkFrame--;
            //             if (_checkFrame == 0)
            //             {
            //                 _passVt = Vector3.zero;
            //                 isOverrideDir = true;
            //                 overrideDir = dir;
            //             }
            //         }
            //     }
            // }
            //
            // if(!check)
            // {
            //     isOverrideDir = false;
            //     _passVt = Vector3.zero;
            // }
            // else
            // {
            //     _passVt = vtDir.normalized;
            // }

            return safeDirection;
        }

        return Test(newPosition - transform.position).normalized;
    }

    private Vector3 GetSafeDirection4(Vector3 newPosition)
    {
        Vector3 Test(Vector3 safeDirection)
        {
            bool isSetAim = false;
            Vector3 nextPos = GetNextPosition(safeDirection);
            float d;
            float distanceObsAim = 0;
            ObstacleInfosCollider.Clear();
            foreach (var obstacle in ObstacleInfos)
            {
                d = GetDistanceToRectangle(obstacle.transform, obstacle.Width, obstacle.Height, nextPos,
                    out Vector3 pointForce);
                if (d < (Radius + Weight))
                {
                    float AngleThrust = Mathf.Clamp(1 - ((d - Radius) / Weight), 0f, 1f) * 90;
                    RectangleObstacleInfo obsInfo = new RectangleObstacleInfo
                        { Obstacle = obstacle, PointForce = pointForce, Distance = d, AngleThrust = AngleThrust };

                    DrawLine(pointForce, pointForce + Vector3.up * 10f, Color.black);
                    if (ObstacleInfosCollider.Count == 0)
                    {
                        // distanceObsAim = Vector3.Distance(pointForce, Destination);
                        ObsAim = obsInfo;
                    }
                    else
                    {
                        // d = Vector3.Distance(pointForce, Destination);
                        // if (d < distanceObsAim)
                        // {
                        //     distanceObsAim = d;
                        //     ObsAim = obsInfo;
                        // }

                        Vector3 posToObsAim = ObsAim.PointForce - nextPos;
                        Vector3 posToObsSet = obsInfo.PointForce - nextPos;
                        if (Angle180Clockwise(posToObsAim, posToObsSet) * _directChoose < 0)
                        {
                            ObsAim = obsInfo;
                        }
                    }

                    AddListCollider(obsInfo);
                    isSetAim = true;
                    _isSetAim = true;
                }
            }

            if (isSetAim)
            {
                DrawLine(ObsAim.Obstacle.Position, ObsAim.Obstacle.Position + Vector3.up * 10, Color.red);
                DrawRay(GetCurrentPosition(), (ObsAim.Obstacle.Position - GetCurrentPosition()).normalized, Color.red,
                    10f);
                RemoveListCollider(ObsAim);
                Vector3 curPosToPoint = ObsAim.PointForce - GetCurrentPosition();
                Vector3 curPosToRectangle = ObsAim.Obstacle.Position - GetCurrentPosition();
                Vector3 curPosToDestination = Destination - GetCurrentPosition();
                int dir = Angle180Clockwise(curPosToRectangle, curPosToDestination) < 0 ? 1 : -1;

                if (dir == DirrectionOverride)
                {
                    IsOverrideDirection = false;
                    _checkFrame = 0;
                }

                Vector3 vtDir;

                if (IsOverrideDirection)
                {
                    dir = DirrectionOverride;
                }

                vtDir = Quaternion.Euler(0, ObsAim.AngleThrust * dir, 0) * curPosToPoint;
                if (IsOverrideDirection || Mathf.Abs(Angle180Clockwise(-curPosToPoint, vtDir)) <
                    Mathf.Abs(Angle180Clockwise(-curPosToPoint, safeDirection)))
                {
                    safeDirection = vtDir;
                }

                DrawRay(GetCurrentPosition(), vtDir.normalized * 2f, Color.red, arrow: true);


                foreach (RectangleObstacleInfo info in ObstacleInfosCollider)
                {
                    DrawRay(GetCurrentPosition(), (info.Obstacle.Position - GetCurrentPosition()).normalized,
                        Color.black, 10f);
                    if (_checkFrame > 0)
                    {
                        _checkFrame--;
                        break;
                    }

                    if (Vector3.Distance(info.PointForce, ObsAim.PointForce) < 2f * Radius)
                    {
                        IsOverrideDirection = true;
                        safeDirection = Quaternion.Euler(0, 180, 0) * safeDirection;
                        DirrectionOverride = -dir;
                        dir = DirrectionOverride;
                        _checkFrame = CheckFrame;
                    }
                    else if (info.AngleThrust >= 85f)
                    {
                        curPosToRectangle = info.Obstacle.Position - GetCurrentPosition();
                        int dir1 = Angle180Clockwise(curPosToRectangle, curPosToDestination) < 0 ? 1 : -1;
                        if (IsOverrideDirection)
                        {
                            dir1 = -dir1;
                            if (DirrectionOverride != dir)
                            {
                                vtDir = Quaternion.Euler(0, 180, 0) * vtDir;
                            }
                        }

                        Vector3 curPosToPoint2 = info.PointForce - GetCurrentPosition();
                        Vector3 vtRecommend = Quaternion.Euler(0, info.AngleThrust * dir1, 0) * curPosToPoint2;
                        DrawRay(GetCurrentPosition(), vtRecommend.normalized, Color.black, arrow: true);
                        safeDirection = vtRecommend + vtDir;

                        // curPosToPoint2 = info.PointForce - GetCurrentPosition();
                        //
                        // if (Mathf.Abs(Angle180Clockwise(-curPosToPoint2, safeDirection)) > 90)
                        // {
                        //     dir1 = Angle180Clockwise(curPosToRectangle, curPosToDestination) < 0 ? 1 : -1;
                        //     // if (IsOverrideDirection)
                        //     // {
                        //     //     dir1 = DirrectionOverride;
                        //     // }
                        //     Vector3 vtRecommend = Quaternion.Euler(0, 90 * dir1, 0) * curPosToPoint2;
                        //     if (ObsAim.AngleThrust < 80)
                        //     {
                        //         Vector3 vt1 = Quaternion.Euler(0, 90 * dir, 0) * curPosToPoint;
                        //         safeDirection = vt1 + vtRecommend;
                        //     }
                        //     else
                        //     {
                        //     
                        //     }
                        //     curPosToRectangle = info.Obstacle.Position - GetCurrentPosition();
                        //     DrawRay(GetCurrentPosition(), vtRecommend.normalized * 2f, Color.black, arrow: true);
                        //     if (Mathf.Abs(Angle180Clockwise(-curPosToPoint.normalized, vtRecommend.normalized)) > 90)
                        //     {
                        //     
                        //         IsOverrideDirection = true;
                        //         DirrectionOverride = -dir;
                        //     }
                        //     else
                        //     {
                        //         safeDirection = vtRecommend;
                        //     }
                        // }
                    }
                }
            }
            else
            {
                _isSetAim = false;
            }

            // bool check = false;
            //
            // if (AngleThrust == 90)
            // {
            //     if (_passVt == Vector3.zero)
            //     {
            //         _checkFrame = CheckFrame;
            //         _passObs = ObsAim;
            //         isOverrideDir = false;
            //         check = true;
            //     }else if (ObsAim == _passObs)
            //     {
            //         check = true;
            //         float angle = Mathf.Abs(Angle180Clockwise(_passVt.normalized, vtDir.normalized));
            //         if(_checkFrame > 0 && (180f - angle) <= 30f)
            //         {
            //             _checkFrame--;
            //             if (_checkFrame == 0)
            //             {
            //                 _passVt = Vector3.zero;
            //                 isOverrideDir = true;
            //                 overrideDir = dir;
            //             }
            //         }
            //     }
            // }
            //
            // if(!check)
            // {
            //     isOverrideDir = false;
            //     _passVt = Vector3.zero;
            // }
            // else
            // {
            //     _passVt = vtDir.normalized;
            // }

            return safeDirection;
        }

        return Test(newPosition - transform.position).normalized;
    }

    #region SUPPORT

    private Vector3 GetPoint(Obstacle Circle1, Obstacle Circle2, bool GetHigher)
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

    private void AddListCollider(RectangleObstacleInfo obs)
    {
        if (String.IsNullOrEmpty(obs.Obstacle.Name)) return;
        int index = ObstacleInfosCollider.IndexOf(obs);
        if (index < 0)
        {
            ObstacleInfosCollider.Add(obs);
        }
    }

    private void RemoveListCollider(RectangleObstacleInfo obs)
    {
        int index = ObstacleInfosCollider.IndexOf(obs);
        if (index >= 0)
        {
            ObstacleInfosCollider.RemoveAt(index);
        }
    }

    private bool CalculateCircleIntersection(Vector3 center1, float radius1, Vector3 center2, float radius2,
        out Vector3 intersection1, out Vector3 intersection2)
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
        intersection1 = new Vector3(p2.x + h * (center2.z - center1.z) / distance, 0,
            p2.z - h * (center2.x - center1.x) / distance);
        intersection2 = new Vector3(p2.x - h * (center2.z - center1.z) / distance, 0,
            p2.z + h * (center2.x - center1.x) / distance);
        return true;
    }


    private float GetDistanceToRectangle(Transform rectangle, float width, float height, Vector3 point,
        out Vector3 pointForce)
    {
        float x = width / 2.0f;
        float y = height / 2.0f;
        Vector3 pointInvertToLocalRectangle = rectangle.InverseTransformPoint(point);
        pointForce = rectangle.TransformPoint(new Vector3(Mathf.Clamp(pointInvertToLocalRectangle.x, -x, x), 0,
            Mathf.Clamp(pointInvertToLocalRectangle.z, -y, y)));
        //----------------
        Vector3 absPoint = new Vector3(Mathf.Abs(pointInvertToLocalRectangle.x),
            Mathf.Abs(pointInvertToLocalRectangle.y), Mathf.Abs(pointInvertToLocalRectangle.z));
        return Mathf.Sqrt(Mathf.Pow(Mathf.Max(absPoint.x - x, 0), 2) + Mathf.Pow(Mathf.Max(absPoint.z - y, 0), 2));
    }

    private float GetDistanceToRectangle(Transform rectangle, float width, float height, Vector3 point)
    {
        float x = width / 2.0f;
        float y = height / 2.0f;
        Vector3 pointInvertToLocalRectangle = rectangle.InverseTransformPoint(point);
        Vector3 absPoint = new Vector3(Mathf.Abs(pointInvertToLocalRectangle.x),
            Mathf.Abs(pointInvertToLocalRectangle.y), Mathf.Abs(pointInvertToLocalRectangle.z));
        return Mathf.Sqrt(Mathf.Pow(Mathf.Max(absPoint.x - x, 0), 2) + Mathf.Pow(Mathf.Max(absPoint.z - y, 0), 2));
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

    private float Angle360Clockwise(Vector3 start, Vector3 dir)
    {
        float angle = Angle180Clockwise(start, dir);
        if (angle < 0)
        {
            angle += 360;
        }

        return angle;
    }

    private float Angle180Clockwise(Vector3 start, Vector3 dir)
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

    private void DrawRay(Vector3 startPos, Vector3 direct, Color color, float length = 1, bool arrow = false)
    {
        Debug.DrawRay(startPos, direct * length, color);
        if (arrow)
        {
            Vector3 dir1 = (Quaternion.Euler(0, 45, 0) * -direct).normalized * .5f;
            Vector3 dir2 = (Quaternion.Euler(0, -45, 0) * -direct).normalized * .5f;
            Vector3 startPos2 = startPos + direct;
            DrawRay(startPos2, dir1, color);
            DrawRay(startPos2, dir2, color);
        }
    }

    private void DrawLine(Vector3 startPos, Vector3 endPos, Color color, bool arrow = false, float time = 0f)
    {
        if (time > 0)
        {
            Debug.DrawLine(startPos, endPos, color, time);
        }
        else
        {
            Debug.DrawLine(startPos, endPos, color);
        }

        if (arrow)
        {
            Vector3 direct = endPos - startPos;
            Vector3 dir1 = (Quaternion.Euler(0, 45, 0) * -direct).normalized * .5f;
            Vector3 dir2 = (Quaternion.Euler(0, -45, 0) * -direct).normalized * .5f;
            Vector3 startPos2 = startPos + direct;
            DrawRay(startPos2, dir1, color);
            DrawRay(startPos2, dir2, color);
        }
    }

    private void Quaterniona()
    {
        // var a = quaternion.LookRotation();
        // var b = Quaternion.LookRotation()
        //math.lerp()
        // transform.TransformPoint();
        // transform.InverseTransformPoint()

        // Vector3.Normalize();
        // float3 a = new float3(1, 2, 3);
        // math.normalize(a);
    }

    #endregion
}