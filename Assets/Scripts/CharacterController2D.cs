using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;
using System;

public class CharacterController2D : MonoBehaviour
{
    public float RayCastDistance = 0.2f;
    public LayerMask layerMask;
    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    public GroundType groundType;
    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _capsuleCollider;
    private float downForceAdjustment = 1.2f;

    private Vector2[] raycastPosition = new Vector2[3];
    private RaycastHit2D[] raycastHits = new RaycastHit2D[3];

    public bool below;
    public bool left;
    public bool right;
    public bool up;

    private bool disableGroundCheck;


    public Vector2 _slopeNormal;
    public float _slopeAngle;
    public float slopeAngleLimit;

    public bool hitGroundThisFrame;
    public bool hitWallThisFrame;

    public WallType leftWallType;
    public WallType rightWallType;
    public GroundType ceilingType;

    [HideInInspector]
    public WallEffector leftWallEffector,rightWallEffector;

    [HideInInspector]
    public bool leftIsRunnable,leftIsJumpable,rightIsRunnable,rightIsJumpable;
    [HideInInspector]
    public float leftSlideModifier, rightSlideModifier;



    bool inAirLastFrame;
    bool noWallLastFrame;

    private Transform tempMovingPlatform;
    private Vector2 _movingPlatformVelocity;

    private AirEffector airEffector;

    [HideInInspector]
    public float jumpPadAmount;
    [HideInInspector]
    public float jumpPadUpperLimit;


    public bool inWater, isSubmerged;

    public bool inAirEffector;
    public AirEffectorType airEffectorType;
    public float airEffectorSpeed;
    public Vector2 airEffectorDirection;



    private void Awake()
    {
        
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
    }


    private void Update()
    {
        inAirLastFrame = !below;
        noWallLastFrame = (!right && !left);

        _lastPosition = _rigidbody.position;
        if(_slopeAngle!=0&&below==true)
        {
            if((_moveAmount.x>0&&_slopeAngle>0)||(_moveAmount.x<0&&_slopeAngle<0))
            {
                _moveAmount.y = -Mathf.Abs(Mathf.Tan(_slopeAngle * Mathf.Deg2Rad) * _moveAmount.x);
                _moveAmount.y *= downForceAdjustment;
            }
        }

        if(groundType == GroundType.MovingPlatform)
        {
            _moveAmount.x += MovingPlatformAdjust().x;

            if(MovingPlatformAdjust().y<0)
            {
                _moveAmount.y += MovingPlatformAdjust().y;
                _moveAmount.y *= downForceAdjustment;
            }
        }

        if(groundType==GroundType.CollapsiblePlatform)
        {
            if(MovingPlatformAdjust().y<0)
            {
                _moveAmount.y += MovingPlatformAdjust().y;
                _moveAmount.y *= downForceAdjustment*4;
            }
        }
        //tractor beam adjustment

        if(airEffector && airEffectorType==AirEffectorType.TractorBeam)
        {
            Vector2 airEffectorVector = airEffectorDirection * airEffectorSpeed;
            _moveAmount = Vector2.Lerp(_moveAmount, airEffectorVector,Time.deltaTime);
        }

        if(!inWater)
        {
            _currentPosition = _lastPosition + _moveAmount;
            _rigidbody.MovePosition(_currentPosition);
        }
        else
        {
            if(_rigidbody.velocity.magnitude<10f)
            {
                _rigidbody.AddForce(_moveAmount * 300f);
            }
        }
        


        _moveAmount = Vector2.zero;

        if(!disableGroundCheck)
        {
            CheckGrounded();
            
        }
        CheckOtherCollisions();

        if(below&&inAirLastFrame)
        {
            hitGroundThisFrame = true;
           // Debug.Log("HitGroundThisFrame");
        }
        else
        {
            hitGroundThisFrame = false;
        }

        if((right||left) && noWallLastFrame)
        {
            hitWallThisFrame = true;
        }
        else
        {
            hitWallThisFrame = false;
        }

    }

    public void Move( Vector2 movement)
    {
        _moveAmount += movement;
    }


    private void CheckOtherCollisions()
    {
        RaycastHit2D leftHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.75f, 0f, Vector2.left,
            RayCastDistance * 2, layerMask);

        RaycastHit2D rightHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.75f, 0f, Vector2.right,
            RayCastDistance * 2, layerMask);

        RaycastHit2D aboveHit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, CapsuleDirection2D.Vertical,
            0f, Vector2.up, RayCastDistance, layerMask);

       

        //LeftCheck

        if (leftHit.collider)
        {
            leftWallType = DetermineWallType(leftHit.collider);
            left = true;
            leftWallEffector = leftHit.collider.GetComponent<WallEffector>();

            if(leftWallEffector!=null)
            {
                leftIsRunnable = leftWallEffector.isRunnable;
                leftIsJumpable = leftWallEffector.isJumpable;

                leftSlideModifier = leftWallEffector.wallSlideAmount;
            }
        }
        else
        {
            leftWallType = WallType.Normal;
            left = false;
        }

        //RightCheck

        if(rightHit.collider)
        {
            rightWallType = DetermineWallType(rightHit.collider);
            right = true;
            rightWallEffector = rightHit.collider.GetComponent<WallEffector>();

            if (rightWallEffector != null)
            {
                rightIsRunnable = rightWallEffector.isRunnable;
                rightIsJumpable = rightWallEffector.isJumpable;

                rightSlideModifier = rightWallEffector.wallSlideAmount;
            }
        }
        else
        {
            rightWallType = WallType.Normal;
            right = false;
        }

        //TopCheck

        if(aboveHit.collider)
        {
            ceilingType = DetermineGroundType(aboveHit.collider);
            up = true;
        }
        else
        {
            ceilingType = GroundType.None;
            up = false;
        }


    }
    private void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, CapsuleDirection2D.Vertical,
            0f, Vector2.down, RayCastDistance, layerMask);

        if(hit.collider)
        {
            groundType = DetermineGroundType(hit.collider);
            _slopeNormal = hit.normal;
            _slopeAngle = Vector2.SignedAngle(Vector2.up, _slopeNormal);

            if (_slopeAngle > slopeAngleLimit || _slopeAngle < -slopeAngleLimit)
            {
                below = false;
            }
            else
            {
                below = true;
            }

            if(groundType == GroundType.JumpPad)
            {
                JumpPad jumpPad = hit.collider.GetComponent<JumpPad>();
                jumpPadAmount = jumpPad.jumpPadAmount;
                jumpPadUpperLimit = jumpPad.jumpPadUpperLimit;
            }

        }
        else
        {
            groundType = GroundType.None;
            below = false;
            if(tempMovingPlatform)
            {
                tempMovingPlatform = null;
            }
        }

    

    }

    

    private GroundType DetermineGroundType(Collider2D collider)
    {
        if(collider.GetComponent<GroundEffector>())
        {
            GroundEffector groundEffector = collider.GetComponent<GroundEffector>();
            if(groundType==GroundType.MovingPlatform || groundType == GroundType.CollapsiblePlatform)
            {
                if(!tempMovingPlatform)
                {
                    tempMovingPlatform = collider.transform;

                    if (groundType == GroundType.CollapsiblePlatform)
                    {
                        tempMovingPlatform.GetComponent<CollapsiblePlatform>().CollapsePlatform();
                    }
                }
                

              
            }
            return groundEffector.groundType;
        }
        else
        {
            if(tempMovingPlatform)
            {
                tempMovingPlatform = null;
            }
            return GroundType.LevelGeometry;
        }
    }

    private WallType DetermineWallType(Collider2D collider)
    {
        if(collider.GetComponent<WallEffector>())
        {
            WallEffector wallEffector = collider.GetComponent<WallEffector>();
            return wallEffector.wallType;
        }
        else
        {
            return WallType.Normal;
        }
    }

    private void DebugRays(Vector2 direction , Color color)
    {
        for(int i=0;i<raycastPosition.Length;i++)
        {
            Debug.DrawRay(raycastPosition[i], direction, color);
        }
    }


    public void DisableGroundCheck()
    {
        below = false;
        disableGroundCheck = true;
        StartCoroutine("EnableGroundCheck");
    }

    IEnumerator EnableGroundCheck()
    {
        yield return new WaitForSeconds(0.1f);
        disableGroundCheck = false;
    }

    private Vector2 MovingPlatformAdjust()
    {
        if (tempMovingPlatform && groundType == GroundType.MovingPlatform)
        {
            _movingPlatformVelocity = tempMovingPlatform.GetComponent<MovingPlatform>().difference;
            return _movingPlatformVelocity;
        }
        else if(tempMovingPlatform&& groundType==GroundType.CollapsiblePlatform)
        {
            _movingPlatformVelocity = tempMovingPlatform.GetComponent<CollapsiblePlatform>().difference;
            return _movingPlatformVelocity;
        }
        else return Vector3.zero;
    }

    public void ClearTempPlatform()
    {
        if (tempMovingPlatform)
        {
            tempMovingPlatform = null;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<BuoyancyEffector2D>())
        {
            inWater = true;

        }

        if(collision.GetComponent<AirEffector>())
        {
            inAirEffector = true;
            airEffector = collision.gameObject.GetComponent<AirEffector>();

            airEffectorType = airEffector.airEffectorType;
            airEffectorDirection = airEffector.direction;
            airEffectorSpeed = airEffector.speed;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.bounds.Contains(_capsuleCollider.bounds.min)&&collision.bounds.Contains(_capsuleCollider.bounds.max)
            &&collision.GetComponent<BuoyancyEffector2D>())
        {
            isSubmerged = true;
        }
        else
        {
            isSubmerged = false;
        }


       
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.GetComponent<BuoyancyEffector2D>())
        {
            _rigidbody.velocity = Vector2.zero;
            inWater = false;
        }

        if (collision.GetComponent<AirEffector>())
        {
            inAirEffector = false;
            airEffector.DeactivateEffector();
            airEffector = null;

            airEffectorType = AirEffectorType.None;
            airEffectorDirection = Vector2.zero;
            airEffectorSpeed = 0f;
            
        }
    }


    public void DeactivateAirEffector()
    {
        if(airEffector)
        {
            airEffector.DeactivateEffector();
        }
    }
}
