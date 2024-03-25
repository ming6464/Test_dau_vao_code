using UnityEngine;

public class CanonController : MonoBehaviour
{

    #region PROPERTIES
    
    [SerializeField] private GameObject _bulletPrefab;
    
    private float _angleYRotationNext;

    private float _angleRotationVelocity;

    private bool _isRotateToTarget;

    private TankController _tankController;

    private bool _isReturnOriginAngle;

    private float _originLocalAngleY;
    
    #endregion
    
    #region UNITY CORE

    private void Start()
    {
        _originLocalAngleY = transform.localRotation.eulerAngles.y;
    }
    
    public void Init(TankController tankMovement)
    {
        _tankController = tankMovement;
    }
    
    private void Update()
    {
        if (_isRotateToTarget)
        {
            float angleY = Mathf.SmoothDampAngle(_isReturnOriginAngle ? transform.localRotation.eulerAngles.y : transform.rotation.eulerAngles.y, _angleYRotationNext,
                ref _angleRotationVelocity, _isReturnOriginAngle ? .05f : .2f);

            if (Mathf.Abs(Mathf.DeltaAngle(angleY, _angleYRotationNext)) <= .1f)
            {
                angleY = _angleYRotationNext;
                _isRotateToTarget = false;
            }

            if (_isReturnOriginAngle)
            {
                transform.localRotation = Quaternion.Euler(0,angleY,0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0,angleY,0);
            }
            
            
            if (!_isRotateToTarget)
            {
                if (_isReturnOriginAngle)
                {
                    _isRotateToTarget = false;
                    if (_tankController)
                    {
                        _tankController.FinishSetUpFireCanon();
                    }
                }
                else
                {
                    OnFire();
                    _angleYRotationNext = _originLocalAngleY;
                    _isReturnOriginAngle = true;
                    _isRotateToTarget = true;
                }
                
                
            }
        }
    }


    #endregion

    #region MAIN

    #region Fire

    public void SetUpFire(Transform target)
    {
        if(!target) return;

        _isRotateToTarget = true;
        _isReturnOriginAngle = false;
        _angleYRotationNext =
            Quaternion.LookRotation(target.position - transform.position).eulerAngles.y;
    }

    

    private void OnFire()
    {
        this.PostEvent(EventID.PlayerFire);
        if (_bulletPrefab)
        {
            Debug.DrawRay(transform.position,transform.forward * 10,Color.green,10f);
            return;
        }
        
    }

    #endregion
    
    #endregion
    
    
    
}
