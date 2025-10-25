using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet.Connection;
using FishNet.Object;

public class PlayerMovement : MonoBehaviour
{
    [Header("Animation")]
    public Animator playerAnimController;

    [Header("Movement")]
    public float moveSpeed;

    public GameObject charModel;

    public Transform lookAtTarget;

    public Camera cinCam;

    public float groundDrag;

    public bool afterSwing = false;

    public float zVelo = 0f;

    public Vector3 xVelo = Vector3.zero;

    private Vector3 originalEuler;

    private Vector3 originalEulerBeforeLookAt;

    public TextMeshProUGUI speedText;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool readyToJump;

    public float dashSpeedChangeFactor;

    public float maxYSpeed;

    public float slideSpeed;

    public float wallrunSpeed;

    public weaponPickup wp;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("FOV")]
    public PlayerCam cam;
    public float grappleFov = 95f;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Grappling gp;
    public jumpCDhandler jcdh;
    public WallRunning wr;
    public SwingScript ss;
    public Grappling gg;

    public bool isJumping = false;
    public bool isWalking = false;
    public bool wallrunning = false;

    public MovementState state;

    public enum MovementState
    {
        freeze,
        sprinting,
        walking,
        crouching,
        wallrunning,
        dashing,
        grappling,
        swinging,
        sliding,
        air
    }

    public bool sliding;

    public float velocityFactor;

    public bool dashing;

    public bool freeze;

    public bool activeGrapple;

    public bool swinging;

    public Transform orientation;

    public float lastGlide;

    public PlayerCam pc;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    Quaternion OriginalRot;

    // public override void OnStartClient()
    // {
        // base.OnStartClient();
        // if (base.IsOwner)
       // {

        //}
  //  }


    private void Start()
    {
        pc = cinCam.GetComponent<PlayerCam>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        gp = GetComponent<Grappling>();
        ss = GetComponent<SwingScript>();
        wr = GetComponent<WallRunning>();
        wp = GetComponent<weaponPickup>();
        jcdh = GetComponent<jumpCDhandler>();
        readyToJump = true;
       // playerAnimController = GetComponent<Animator>();

        startYScale = transform.localScale.y;

        originalEuler = new Vector3(charModel.transform.eulerAngles.x, charModel.transform.eulerAngles.y, charModel.transform.eulerAngles.z);
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        OriginalRot = charModel.transform.rotation;

        StateHandler();
        MyInput();
        SpeedControl();
        GetSlopeMoveDirection();

        speedText.text = "Speed: " + moveSpeed;

        if (grounded)
        {
            playerAnimController.SetBool("isJumping", false);
        }
        else if (readyToJump == false && !dashing && !wallrunning && !ss.isSwinging && !activeGrapple)
        {
            playerAnimController.SetBool("isIdle", false);
            playerAnimController.SetBool("isWalking", false);
            playerAnimController.SetBool("isSprinting", false);
            playerAnimController.SetBool("isCrouching", false);
            playerAnimController.SetBool("isCrouchMoving", false);
            playerAnimController.SetBool("isSliding", false);
            playerAnimController.SetBool("isJumping", true);
            playerAnimController.SetBool("isDashing", false);
            playerAnimController.SetBool("isSwinging", false);
        }

        if (!wallrunning || !ss.isSwinging || !activeGrapple);
        {
            float zAngle = Mathf.SmoothDampAngle(charModel.transform.eulerAngles.z, originalEuler.z, ref zVelo, 0.1f);
            charModel.transform.eulerAngles = new Vector3(charModel.transform.eulerAngles.x, charModel.transform.eulerAngles.y, zAngle);
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), 0.2f);

        }
        
        if(Input.GetKeyDown(crouchKey) && !activeGrapple && !ss.isSwinging && grounded)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        }
    }

    public void CounterMovement()
    {
        Vector3 vel = rb.velocity;
        vel.y = 0f;

        float coefficientOfFriction = 12f;

        rb.AddForce(-vel * coefficientOfFriction, ForceMode.Acceleration);
    }

    public void MovePlayer()
    {
        if (swinging) return;
        if (state == MovementState.dashing) return;
        if (state == MovementState.freeze) return;

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope && moveSpeed > 12)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 10f, ForceMode.Force);
            CounterMovement();

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 120f, ForceMode.Force);
        }

        else if (OnSlope() && !exitingSlope && moveSpeed <= 12)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 10f, ForceMode.Force);
            CounterMovement();

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        // on ground
        else if (grounded && !OnSlope())
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
            CounterMovement();
        }

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        if(!wallrunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (activeGrapple) return;
        if (gg.grappling) return;
        if (state == MovementState.sprinting) return;
        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

        if (maxYSpeed != 0 && rb.velocity.y > maxYSpeed)
        {
            rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
        }
    }

    private bool enableMovementOnNextTouch;

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetFov), 2f);
    }

    private Vector3 velocityToSet;

    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet * velocityFactor;

        cam.DoFov(grappleFov);
    }

    public void ResetFov()
    {
        cam.DoFov(80f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;

            GetComponent<Grappling>().StopGrapple();

            activeGrapple = false;

            cam.DoFov(80f);
        }
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    private void Jump()
    {
        rb.drag = 0f;
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        exitingSlope = false;

        jcdh.doCD();
    }

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;

    private void StateHandler()
    {

        if (freeze)
        {
            rb.drag = 0f;
            state = MovementState.freeze;
            desiredMoveSpeed = 0;
            rb.velocity = Vector3.zero;
        }

        else if (activeGrapple)
        {
            pc.targetFOV = 100f;
            pc.isTransitioning = true;

            rb.drag = 0f;
            state = MovementState.grappling;
            desiredMoveSpeed = 0f;

            playerAnimController.SetBool("isIdle", false);
            playerAnimController.SetBool("isWalking", false);
            playerAnimController.SetBool("isSprinting", false);
            playerAnimController.SetBool("isCrouching", false);
            playerAnimController.SetBool("isCrouchMoving", false);
            playerAnimController.SetBool("isSliding", false);
            playerAnimController.SetBool("isJumping", false);
            playerAnimController.SetBool("isDashing", false);
            playerAnimController.SetBool("isSwinging", true);
            if (!grounded)
            {
                // charModel.transform.LookAt(ss.swingPoint, Vector3.forward);
                // Quaternion NewRot = charModel.transform.rotation;
                // charModel.transform.rotation = Quaternion.Lerp(OriginalRot, NewRot, 0.01f);
                // afterSwing = true;
            }

            if (grounded)
            {
                charModel.transform.rotation = Quaternion.Lerp(charModel.transform.rotation, cam.transform.rotation, 0.023f);
            }
        }

        else if (ss.isSwinging)
        {
            pc.targetFOV = 150f;
            pc.isTransitioning = true;


            rb.drag = 0f;
            state = MovementState.swinging;
            desiredMoveSpeed = 25f;

            playerAnimController.SetBool("isIdle", false);
            playerAnimController.SetBool("isWalking", false);
            playerAnimController.SetBool("isSprinting", false);
            playerAnimController.SetBool("isCrouching", false);
            playerAnimController.SetBool("isCrouchMoving", false);
            playerAnimController.SetBool("isSliding", false);
            playerAnimController.SetBool("isJumping", false);
            playerAnimController.SetBool("isDashing", false);
            playerAnimController.SetBool("isSwinging", true);
            if (!grounded)
            {
                charModel.transform.LookAt(ss.swingPoint, Vector3.forward);
                Quaternion NewRot = charModel.transform.rotation;
                charModel.transform.rotation = Quaternion.Lerp(OriginalRot, NewRot, 0.01f);
                afterSwing = true;
            }

            if (grounded)
            {
                charModel.transform.rotation = Quaternion.Lerp(charModel.transform.rotation, cam.transform.rotation, 0.023f);
            }
        }

        else if (wallrunning)
        {
            pc.targetFOV = 150f;
            pc.isTransitioning = true;

            charModel.transform.eulerAngles = new Vector3(originalEuler.x, cam.transform.eulerAngles.y, charModel.transform.eulerAngles.z);
            afterSwing = false;
            state = MovementState.wallrunning;
            playerAnimController.SetBool("isIdle", false);
            playerAnimController.SetBool("isWalking", false);
            playerAnimController.SetBool("isSprinting", true);
            playerAnimController.SetBool("isCrouching", false);
            playerAnimController.SetBool("isCrouchMoving", false);
            playerAnimController.SetBool("isSliding", false);
            playerAnimController.SetBool("isJumping", false);
            playerAnimController.SetBool("isDashing", false);
            playerAnimController.SetBool("isSwinging", false);

            if (wr.wallRight)
            {
                // charModel.transform.eulerAngles = Vector3.SmoothDamp( (new Vector3(charModel.transform.eulerAngles.x, charModel.transform.eulerAngles.y, charModel.transform.eulerAngles.z)), (new Vector3(charModel.transform.eulerAngles.x, charModel.transform.eulerAngles.y, originalEuler.z) + new Vector3(0, 0, 25)), ref velocityForWallrunZlerp, 0.1f);
                float zAngle = Mathf.SmoothDampAngle(charModel.transform.eulerAngles.z, originalEuler.z + 25, ref zVelo, 0.1f);
                charModel.transform.eulerAngles = new Vector3(charModel.transform.eulerAngles.x, charModel.transform.eulerAngles.y, zAngle);
            }
            else if (wr.wallLeft)
            {
                // charModel.transform.eulerAngles = Vector3.SmoothDamp( (new Vector3(charModel.transform.eulerAngles.x, charModel.transform.eulerAngles.y, charModel.transform.eulerAngles.z)), (new Vector3(charModel.transform.eulerAngles.x, charModel.transform.eulerAngles.y, originalEuler.z) + new Vector3(0, 0, -25)), ref velocityForWallrunZlerp, 0.1f);
                float zAngle = Mathf.SmoothDampAngle(charModel.transform.eulerAngles.z, originalEuler.z + -25, ref zVelo, 0.1f);
                charModel.transform.eulerAngles = new Vector3(charModel.transform.eulerAngles.x, charModel.transform.eulerAngles.y, zAngle);
            }

            desiredMoveSpeed = wallrunSpeed;
        }

        else if (sliding && grounded)
        {
            pc.targetFOV = 150f;
            pc.isTransitioning = true;

            charModel.transform.eulerAngles = new Vector3(originalEuler.x, cam.transform.eulerAngles.y, 0);

            afterSwing = false;
            state = MovementState.sliding;

            playerAnimController.SetBool("isIdle", false);
            playerAnimController.SetBool("isWalking", false);
            playerAnimController.SetBool("isSprinting", false);
            playerAnimController.SetBool("isCrouching", false);
            playerAnimController.SetBool("isCrouchMoving", false);
            playerAnimController.SetBool("isSliding", true);
            playerAnimController.SetBool("isJumping", false);
            playerAnimController.SetBool("isDashing", false);
            playerAnimController.SetBool("isSwinging", false);
            playerAnimController.SetBool("isDropping", false);

            if (OnSlope() && rb.velocity.y < 0.1f)
            {
                desiredMoveSpeed = 39f;
                cam.DoFov(95f);
            }
            else
            {
                desiredMoveSpeed = 30f;
                cam.DoFov(95f);
            }
        }

        else if (Input.GetKey(crouchKey) && grounded)
        {
            pc.targetFOV = 50f;
            pc.isTransitioning = true;

            charModel.transform.eulerAngles = new Vector3(originalEuler.x, cam.transform.eulerAngles.y, charModel.transform.eulerAngles.z);

            afterSwing = false;
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
            if (rb.velocity.magnitude > 1)
            {
                playerAnimController.SetBool("isIdle", false);
                playerAnimController.SetBool("isWalking", false);
                playerAnimController.SetBool("isSprinting", false);
                playerAnimController.SetBool("isCrouching", false);
                playerAnimController.SetBool("isCrouchMoving", true);
                playerAnimController.SetBool("isSliding", false);
                playerAnimController.SetBool("isJumping", false);
                playerAnimController.SetBool("isDashing", false);
                playerAnimController.SetBool("isSwinging", false);
                playerAnimController.SetBool("isDropping", false);
            }
            else
            {
                playerAnimController.SetBool("isIdle", false);
                playerAnimController.SetBool("isWalking", false);
                playerAnimController.SetBool("isSprinting", false);
                playerAnimController.SetBool("isCrouching", true);
                playerAnimController.SetBool("isCrouchMoving", false);
                playerAnimController.SetBool("isSliding", false);
                playerAnimController.SetBool("isJumping", false);
                playerAnimController.SetBool("isDashing", false);
                playerAnimController.SetBool("isSwinging", false);
                playerAnimController.SetBool("isDropping", false);
            }
            cam.DoFov(70f);
        }

        else if (dashing)
        {
            pc.targetFOV = 400f;
            pc.isTransitioning = true;

            charModel.transform.eulerAngles = new Vector3(originalEuler.x, cam.transform.eulerAngles.y, charModel.transform.eulerAngles.z);

            rb.drag = 0f;
            state = MovementState.dashing;

            playerAnimController.SetBool("isIdle", false);
            playerAnimController.SetBool("isWalking", false);
            playerAnimController.SetBool("isSprinting", false);
            playerAnimController.SetBool("isCrouching", false);
            playerAnimController.SetBool("isCrouchMoving", false);
            playerAnimController.SetBool("isSliding", false);
            playerAnimController.SetBool("isJumping", false);
            playerAnimController.SetBool("isDashing", true);
            playerAnimController.SetBool("isSwinging", false);
            playerAnimController.SetBool("isDropping", false);

            desiredMoveSpeed = 69f;
            speedChangeFactor = dashSpeedChangeFactor;

            if (afterSwing)
            {
                charModel.transform.eulerAngles = Vector3.SmoothDamp(charModel.transform.eulerAngles, new Vector3(originalEuler.x, cam.transform.eulerAngles.y, 0), ref xVelo, 0.023f);
                afterSwing = false;
            }

        }

        else if (grounded && Input.GetKey(sprintKey) && rb.velocity.magnitude > 1)
        {
            charModel.transform.eulerAngles = new Vector3(originalEuler.x, cam.transform.eulerAngles.y, charModel.transform.eulerAngles.z);

            pc.targetFOV = 120f;
            pc.isTransitioning = true;

            afterSwing = false;
            rb.drag = 0f;
            state = MovementState.sprinting;
            desiredMoveSpeed = 24f;
            playerAnimController.SetBool("isIdle", false);
            playerAnimController.SetBool("isWalking", false);
            playerAnimController.SetBool("isSprinting", true);
            playerAnimController.SetBool("isCrouching", false);
            playerAnimController.SetBool("isCrouchMoving", false);
            playerAnimController.SetBool("isSliding", false);
            playerAnimController.SetBool("isJumping", false);
            playerAnimController.SetBool("isDashing", false);
            playerAnimController.SetBool("isSwinging", false);
            playerAnimController.SetBool("isDropping", false);
            cam.DoFov(95f);

        }

        else if (grounded)
        {
            pc.targetFOV = 60f;
            pc.isTransitioning = true;

            charModel.transform.eulerAngles = new Vector3(originalEuler.x, cam.transform.eulerAngles.y, 0);


            if (afterSwing)
            {
                charModel.transform.eulerAngles = Vector3.SmoothDamp(charModel.transform.eulerAngles, new Vector3(cam.transform.eulerAngles.x, cam.transform.eulerAngles.y, 0), ref xVelo, 0.023f);
                afterSwing = false;
            }

            rb.drag = 0f;
            if (rb.velocity.magnitude > 1)
            {

                playerAnimController.SetBool("isIdle", false);
                playerAnimController.SetBool("isWalking", true);
                playerAnimController.SetBool("isSprinting", false);
                playerAnimController.SetBool("isCrouching", false);
                playerAnimController.SetBool("isCrouchMoving", false);
                playerAnimController.SetBool("isSliding", false);
                playerAnimController.SetBool("isJumping", false);
                playerAnimController.SetBool("isDashing", false);
                playerAnimController.SetBool("isSwinging", false);
                playerAnimController.SetBool("isDropping", false);


            }
            else
            {
                playerAnimController.SetBool("isIdle", true);
                playerAnimController.SetBool("isWalking", false);
                playerAnimController.SetBool("isSprinting", false);
                playerAnimController.SetBool("isCrouching", false);
                playerAnimController.SetBool("isCrouchMoving", false);
                playerAnimController.SetBool("isSliding", false);
                playerAnimController.SetBool("isJumping", false);
                playerAnimController.SetBool("isDashing", false);
                playerAnimController.SetBool("isSwinging", false);
                playerAnimController.SetBool("isDropping", false);
            }

            if (Input.GetKeyDown(KeyCode.F) && wp.CurrentGameObject)
            {
                charModel.transform.eulerAngles = new Vector3(originalEuler.x, cam.transform.eulerAngles.y, 0);
                afterSwing = false;

                if (wp.CurrentGameObject && !wp.CurrentObject)
                {
                    rb.velocity = new Vector3(0,0,0);
                    playerAnimController.SetBool("isIdle", false);
                    playerAnimController.SetBool("isWalking", false);
                    playerAnimController.SetBool("isSprinting", false);
                    playerAnimController.SetBool("isCrouching", false);
                    playerAnimController.SetBool("isCrouchMoving", false);
                    playerAnimController.SetBool("isSliding", false);
                    playerAnimController.SetBool("isJumping", false);
                    playerAnimController.SetBool("isDashing", false);
                    playerAnimController.SetBool("isSwinging", false);
                    playerAnimController.SetBool("isDropping", true);
                }
            }
            state = MovementState.walking;
            desiredMoveSpeed = 12f;
            cam.DoFov(80f);
        }

        else
        {
            pc.targetFOV = 75f;
            pc.isTransitioning = true;

            rb.drag = 0f;
            state = MovementState.air;
            desiredMoveSpeed = 17f;

            playerAnimController.SetBool("isIdle", false);
            playerAnimController.SetBool("isWalking", false);
            playerAnimController.SetBool("isSprinting", false);
            playerAnimController.SetBool("isCrouching", false);
            playerAnimController.SetBool("isCrouchMoving", false);
            playerAnimController.SetBool("isSliding", false);
            playerAnimController.SetBool("isJumping", true);
            playerAnimController.SetBool("isDashing", false);
            playerAnimController.SetBool("isSwinging", false);
            playerAnimController.SetBool("isDropping", false);

            if (afterSwing)
            {
                charModel.transform.rotation = Quaternion.Lerp(charModel.transform.rotation, cam.transform.rotation, 0.023f);

                if (charModel.transform.eulerAngles.x < 0f)
                {
                    charModel.transform.eulerAngles = new Vector3(0, charModel.transform.eulerAngles.y, charModel.transform.eulerAngles.z);
                }
                
            }

            if (desiredMoveSpeed < 17f)
                desiredMoveSpeed = 12f;
            else
                desiredMoveSpeed = 17f;

        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) keepMomentum = true;

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 6f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpSlideSpeed());
        }

        else if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        else
        {
            StopAllCoroutines();
            moveSpeed = desiredMoveSpeed;
        }

        

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private float speedChangeFactor;

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    private IEnumerator SmoothlyLerpSlideSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            time += Time.deltaTime;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    IEnumerator waitForJumpReset()
    {


        yield return new WaitForSeconds(1);

        readyToJump = true;
    }

    public bool OnSlope()
    {

        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

}