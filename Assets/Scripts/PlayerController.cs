using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GlobalTypes;


public class PlayerController : MonoBehaviour
{
    #region Public Variables
    [Header("Player Properties")]
    public float walkSpeed = 15f;
    public float creepSpeed = 10f;
    public float _gravity = 20f;
    public float JumpSpeed = 15f;
    public float doubleJumpSpeed = 15f;
    public float tripleJumpSpeed = 20f;
    public float xWallJumpSpeed = 15f;
    public float yWallJumpSpeed = 15f;
    public float wallRunAmount = 8f;
    public float wallSlideAmount = 0.1f;

    public float glideTime = 3f;
    public float glideDescentAmount = 2f;

    public float powerJumpSpeed = 40f;
    public float groundSlamSpeed = 60f;

    public float powerJumpWaitTime = 1f;

    public float dashSpeed = 50f;
    public float dashTime = 0.2f;
    public float dashCoolDownTime = 1f;
    public float deadZoneValue = 0.15f;
    public float swimSpeed = 150f;


   
 


    [Header("State")]
    public bool isJumping=false;
    public bool isDoubleJumping;
    public bool isTripleJumping;
    public bool isWallJumping;
    public bool isWallRunning;
    public bool isDucking, isCreeping,isGliding,isPowerJumping,isDashing,isGroundSlamming;
    public bool isSwimming;
    

    [Header("Abilities")]
    public bool canDoubleJump;
    public bool canTripleJump;
    public bool canWallJump;
    public bool canJumpAfterWallJump;
    public bool canWallRun;
    public bool canMultipleWallRun;
    public bool canWallSlide,canGlide,canGlideAfterWallContact,canPowerJump;
    public bool canGroundDash, canAirDash,canGroundSlam;
    public bool canSwim;


    #endregion


    #region Private Variables

    private Vector2 _input;
    private Vector2 _moveDirection;
    private bool _startJump;
    private bool _releaseJump;
    private float _currentGlideTime;
    private bool _startGlide=true;
    private float _powerJumpTime;
    private float _dashTimer;
   
    private bool facingRight;

    private CharacterController2D _characterController2D;
    private CapsuleCollider2D _capsuleCollider;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _originalColliderSize;

    private bool ableToWallRun = true;
    bool holdJump;


    private float _jumpPadAmount = 15f;
    private float _jumpPadAdjustment;
    private Vector2 _tempVelocity;

    #endregion

    private void Start()
    {
        _characterController2D = gameObject.GetComponent<CharacterController2D>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _originalColliderSize = _capsuleCollider.size;
        
    }

    private void Update()
    {
        if(_dashTimer>=-10f)
        _dashTimer -= Time.deltaTime;

        //ApplyDeadZone();
        ProcessHorizontalMovement();

        if (_characterController2D.below)
        {
            OnGround();
        }
        else if(_characterController2D.inAirEffector)
        {
            InAirEffector();
        }
        else if(_characterController2D.inWater)
        {
            InWater();
        }
        else 
        {

            InAir();
        }

        _characterController2D.Move(_moveDirection * Time.deltaTime);
    }

    private void InAirEffector()
    {
        if(_startJump)
        {
            _characterController2D.DeactivateAirEffector();
            Jump();
        }

       if(_characterController2D.airEffectorType==AirEffectorType.Ladder)
        {
            if(_input.y>0f)
            {
                _moveDirection.y = _characterController2D.airEffectorSpeed;
            }
            else if(_input.y<0f)
            {
                _moveDirection.y = -_characterController2D.airEffectorSpeed;
            }
            else
            {
                _moveDirection.y = 0f;
            }
        }

       if(_characterController2D.airEffectorType == AirEffectorType.TractorBeam)
        {
            if(_moveDirection.y!=0f)
            {
                _moveDirection.y = Mathf.Lerp(_moveDirection.y, 0f, Time.deltaTime * 4f);
            }
        }

       if(_characterController2D.airEffectorType == AirEffectorType.Updraft)
        {
            if(_input.y<=0f)
            {
                isGliding = false;
            }
            if(isGliding)
            {
                _moveDirection.y = _characterController2D.airEffectorSpeed;
            }
            else
            {
                InAir();
            }
        }
    }

    void GravityCalculations()
    {
       
        if(_moveDirection.y>0 && _characterController2D.up)
        {
            if(_characterController2D.ceilingType == GlobalTypes.GroundType.OneWayPlatform)
            {
                StartCoroutine(DisableOneWayPlatform(false));
            }
            else
            {
                _moveDirection.y = 0f;
            }
            
        }


        if (canWallSlide && (_characterController2D.left || _characterController2D.right))
        {
            if (_characterController2D.hitWallThisFrame)
            {
                _moveDirection.y = 0;
            }

            if (_moveDirection.y <= 0)
            {
                if (_characterController2D.left && _characterController2D.leftWallEffector)
                {
                    _moveDirection.y -= _gravity * _characterController2D.leftSlideModifier * Time.deltaTime;
                }
                else if (_characterController2D.right && _characterController2D.rightWallEffector)
                {
                    _moveDirection.y -= _gravity * _characterController2D.rightSlideModifier * Time.deltaTime;
                }
                else
                {
                    _moveDirection.y -= _gravity * wallSlideAmount * Time.deltaTime;
                }

            }
            else
            {
                _moveDirection.y -= _gravity * Time.deltaTime;
            }
        }
        else if (canGlide && _input.y > 0f && _moveDirection.y < 0.2f)
        {
            if (_currentGlideTime > 0f)
            {

                isGliding = true;

                if (_startGlide)
                {
                    _moveDirection.y = 0f;
                    _startGlide = false;
                }

                _moveDirection.y -= glideDescentAmount * Time.deltaTime;
                _currentGlideTime -= Time.deltaTime;

            }
            else
            {
                Debug.Log("GlideTimeoVer");
                isGliding = false;
                _moveDirection.y -= _gravity * Time.deltaTime;
            }
        }
       



        else if (isGroundSlamming && !isPowerJumping && _moveDirection.y < 0f)
        {
            _moveDirection.y = -groundSlamSpeed;
        }
        else if (!isDashing)
        {
            _moveDirection.y -= _gravity * Time.deltaTime;
            
        }
        
        
        
    }

    void OnGround()
    {
       
        if(_characterController2D.inAirEffector)
        {
            InAirEffector();
            return;
        }

        if(_characterController2D.hitGroundThisFrame)
        {
            _tempVelocity = _moveDirection;
        }
        _moveDirection.y = 0;
        ClearAllFlags();


        Jump();

        DuckingAndCreeping();

        JumpPad();
       
    }

    void JumpPad()
    {
        if(_characterController2D.groundType==GlobalTypes.GroundType.JumpPad)
        {
            _jumpPadAmount = _characterController2D.jumpPadAmount;
           

            if(-_tempVelocity.y > _jumpPadAmount)
            {
                _moveDirection.y = -_tempVelocity.y * 0.92f;
            }
            else
            {
                _moveDirection.y = _jumpPadAmount;
            }


            if(holdJump)
            {
                _jumpPadAdjustment += _moveDirection.y * 0.1f;
                _moveDirection.y += _jumpPadAdjustment;
            }
            else
            {
                _jumpPadAdjustment = 0f;
            }

            if(_moveDirection.y>_characterController2D.jumpPadUpperLimit)
            {
                _moveDirection.y = _characterController2D.jumpPadUpperLimit;
            }
        }
    }
    
    void InAir()
    {

        ClearGroundAbilityFlags();
        AirJump();

        WallRunning();
        

        GravityCalculations();

        if(isGliding && _input.y<=0f)
        {
            isGliding = false;
        }

       
    }
    void ClearAllFlags()
    {
        isJumping = false;
        isDoubleJumping = false;
        isTripleJumping = false;
        isWallJumping = false;
        isGliding = false;
        _currentGlideTime = glideTime;
        isGroundSlamming = false;
        _startGlide = true;
    }

    void Jump()
    {
        if (_startJump)
        {
            _moveDirection.y = 0f;
            _startJump = false;

            if (canPowerJump && isDucking && _characterController2D.groundType != GlobalTypes.GroundType.OneWayPlatform && _powerJumpTime > powerJumpWaitTime)
            {
                _moveDirection.y = powerJumpSpeed;
                StartCoroutine("PowerJumpWaiter");
            }
            else if(isDucking && _characterController2D.groundType == GlobalTypes.GroundType.OneWayPlatform)
            {
                StartCoroutine(DisableOneWayPlatform(true));
            }
            else
            {
                _moveDirection.y = JumpSpeed;
            }

            isJumping = true;
            _characterController2D.DisableGroundCheck();
            _characterController2D.ClearTempPlatform();
            ableToWallRun = true;
        }
    }

    void ClearGroundAbilityFlags()
    {
        if ((isCreeping || isDucking) && _moveDirection.y > 0f)
        {
            StartCoroutine("ClearCrouchState");
        }
        _powerJumpTime = 0f;
       
    }

    void AirJump()
    {
        if (_releaseJump)
        {
            _releaseJump = false;
            if (_moveDirection.y > 0f)
            {
                _moveDirection.y *= 0.5f;
            }
        }

        if (_startJump)
        {
            if (canTripleJump && (!_characterController2D.left && !_characterController2D.right))
            {
                if (isDoubleJumping && !isTripleJumping)
                {
                    _moveDirection.y = tripleJumpSpeed;
                    isTripleJumping = true;
                }
            }


            if (canDoubleJump && !_characterController2D.left && !_characterController2D.right)
            {
                if (!isDoubleJumping)
                {
                    _moveDirection.y = doubleJumpSpeed;
                    isDoubleJumping = true;
                }

            }

            if(_characterController2D.inWater)
            {
                isDoubleJumping = false;
                isTripleJumping = false;
                _moveDirection.y = JumpSpeed;
            }

            if (canWallJump && (_characterController2D.left || _characterController2D.right))
            {
                if (_characterController2D.left && _characterController2D.leftWallEffector && !_characterController2D.leftIsJumpable)
                    return;
                else if (_characterController2D.right && _characterController2D.rightWallEffector && !_characterController2D.rightIsJumpable)
                    return;


                if (_moveDirection.x <= 0f && _characterController2D.left)
                {
                    _moveDirection.x = xWallJumpSpeed;
                    _moveDirection.y = yWallJumpSpeed;

                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    facingRight = true;
                }
                else if (_moveDirection.x >= 0f && _characterController2D.right)
                {
                    _moveDirection.x = -xWallJumpSpeed;
                    _moveDirection.y = yWallJumpSpeed;

                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    facingRight = false;
                }



                StartCoroutine("WallJumpWaiter");
                if (canJumpAfterWallJump)
                {
                    isDoubleJumping = false;
                    isTripleJumping = false;
                }


            }

            _startJump = false;
        }
    }

    void WallRunning()
    {
        if (canWallRun && (_characterController2D.left || _characterController2D.right))
        {
            if (_characterController2D.left && _characterController2D.leftWallEffector && !_characterController2D.leftIsRunnable)
                return;
            else if (_characterController2D.right && _characterController2D.rightWallEffector && !_characterController2D.rightIsRunnable)
                return;

            if (_input.y > 0 && ableToWallRun)
            {

                _moveDirection.y = wallRunAmount;

                if (_characterController2D.left)
                {
                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                else if (_characterController2D.right)
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }

                StartCoroutine("WallRunWaiter");
            }
        }
        else
        {
            if (canMultipleWallRun)
            {
                StopCoroutine("WallRunWaiter");
                ableToWallRun = true;
                isWallRunning = false;
            }
        }

        //GlideAfterWallContact
        if ((_characterController2D.left || _characterController2D.right) && canWallRun)
        {
            if (canGlideAfterWallContact)
            {
                _currentGlideTime = glideTime;
            }
            else
            {
                _currentGlideTime = 0;
            }
        }
    }
    void DuckingAndCreeping()
    {
        if (_input.y < 0f)
        {
            if ((!isCreeping && !isDucking))
            {
                _capsuleCollider.size = new Vector2(_capsuleCollider.size.x, _capsuleCollider.size.y * 0.5f);
                transform.position = new Vector2(transform.position.x, transform.position.y - (_originalColliderSize.y * 0.25f));
                isDucking = true;
                //isCreeping = true;
                _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp_crouching");
            }

            _powerJumpTime += Time.deltaTime;
        }
        else
        {
            if (isDucking || isCreeping)
            {
                RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale, CapsuleDirection2D.Vertical, 0f, Vector2.up,
                    _originalColliderSize.y / 2, _characterController2D.layerMask);


                if (!hitCeiling.collider)
                {
                    _capsuleCollider.size = _originalColliderSize;
                    transform.position = new Vector2(transform.position.x, transform.position.y + (_originalColliderSize.y * 0.25f));
                    _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
                    isDucking = false;
                    isCreeping = false;
                }

            }
            _powerJumpTime = 0f;
        }

        if (isDucking && _moveDirection.x != 0f)
        {
            isCreeping = true;
            _powerJumpTime = 0f;
        }
        else
        {
            isCreeping = false;
        }
    }
    void ProcessHorizontalMovement()
    {
        if (!isWallJumping)
        {
            
            _moveDirection.x = _input.x;


            if (_moveDirection.x < 0f)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
                facingRight = false;
            }
            else if (_moveDirection.x > 0f)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
                facingRight = true;
            }

            if (isDashing)
            {
                if (facingRight) _moveDirection.x = dashSpeed;
                else _moveDirection.x = -dashSpeed;

                _moveDirection.y = 0f;
            }
            else if(isCreeping)
            {
                _moveDirection.x *= creepSpeed;
            }
            else
            {
                
                _moveDirection.x *= walkSpeed;
            }



        }
    }
    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            _startJump = true;
            _releaseJump = false;
            holdJump = true;
        }
        else if(context.canceled)
        {
            _releaseJump = true;
            _startJump = false;
            holdJump = false;
        }
    }



    void InWater()
    {
        ClearGroundAbilityFlags();
        if (isGroundSlamming)
            isGroundSlamming = false;
        AirJump();
        if(_input.y!=0f && canSwim && !holdJump)
        {
            if(_input.y>0f && !_characterController2D.isSubmerged)
            {
                _moveDirection.y = 0f;
            }
            else
            {
                _moveDirection.y = (_input.y * swimSpeed) * Time.deltaTime;
            }
            
        }
        else if(_moveDirection.y<0f&&_input.y==0f)
        {
            _moveDirection.y += 2f;
        }

        if(_characterController2D.isSubmerged && canSwim)
        {
            isSwimming = true;
        }
        else
        {
            isSwimming = false;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if(context.started && _dashTimer<=0f)
        {
            if((canAirDash&&!_characterController2D.below)||(canGroundDash&&_characterController2D.below))
            {
                StartCoroutine("Dash");
                
            }
        }
    }


    public void OnAttack(InputAction.CallbackContext context)
    {
        if(context.performed &&_input.y<0f)
        {
            if(canGroundSlam && !_characterController2D.below)
            {
                isGroundSlamming = true;
            }
            
        }
    }
    IEnumerator Dash()
    {
        isDashing = true;
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        _dashTimer = dashCoolDownTime;
    }


    private void ApplyDeadZone()
    {
        if (_input.x > -deadZoneValue || _input.x < deadZoneValue)
            _input.x = 0f;

        if (_input.y > -deadZoneValue || _input.y < deadZoneValue)
            _input.y = 0f;
    }
    private IEnumerator WallJumpWaiter()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(0.4f);
        isWallJumping = false;
    }

    private IEnumerator DisableOneWayPlatform(bool checkBelow)
    {
        bool originalCanGroundSlam = canGroundSlam;
        GameObject tempOneWayPlatform = null;

        if(checkBelow)
        {
            Vector2 rayCastPosition = transform.position - new Vector3(0, _capsuleCollider.size.y * 0.5f, 0);
            RaycastHit2D hit = Physics2D.Raycast(rayCastPosition, Vector2.down, _characterController2D.RayCastDistance, _characterController2D.layerMask);

            if(hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;
            }
        }
        else
        {
            Vector2 rayCastPosition = transform.position + new Vector3(0, _capsuleCollider.size.y * 0.5f, 0);
            RaycastHit2D hit = Physics2D.Raycast(rayCastPosition, Vector2.up, _characterController2D.RayCastDistance, _characterController2D.layerMask);

            if (hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;
            }
        }

        if(tempOneWayPlatform)
        {
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = false;
            canGroundSlam = false;
        }

        yield return new WaitForSeconds(0.25f);

        if(tempOneWayPlatform)
        {
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = true;
            canGroundSlam = originalCanGroundSlam;
        }

    }
    IEnumerator WallRunWaiter()
    {
        isWallRunning = true;
        yield return new WaitForSeconds(0.5f);
        isWallRunning = false;

        if(!isWallJumping)
        {
            ableToWallRun = false;
        }
    }

    IEnumerator ClearCrouchState()
    {
        yield return new WaitForSeconds(0.05f);
        RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale, CapsuleDirection2D.Vertical, 0f, Vector2.up,
                        _originalColliderSize.y / 2, _characterController2D.layerMask);


        if (!hitCeiling.collider)
        {
            _capsuleCollider.size = _originalColliderSize;
            //transform.position = new Vector2(transform.position.x, transform.position.y + (_originalColliderSize.y * 0.25f));
            _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
            isDucking = false;
            isCreeping = false;
        }

    }

    IEnumerator PowerJumpWaiter()
    {
        isPowerJumping = true;
        yield return new WaitForSeconds(0.8f);
        isPowerJumping = false;
    }
}

