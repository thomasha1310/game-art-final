using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private float horizontal;
    private bool isFacingRight = true;
    private Rigidbody2D rb = null;
    private Animator animator = null;
    private bool isControllable = true;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.1f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;

    private int numCoinsCollected = 0;

    private AudioFxPlayer aFxPlayer;

    [SerializeField, Tooltip("The maximum speed of the player")]
    private float speed = 8f;
    [SerializeField, Tooltip("Acceleration of the player.")]
    private float acceleration = 0.2f;
    [SerializeField, Tooltip("Deceleration of the player")]
    private float groundDeceleration = 0.5f;
    [SerializeField, Tooltip("Horizontal deceleration of the player, while in the air.")]
    private float airDeceleration = 0.5f;
    [SerializeField, Tooltip("Magnitude of the power applied to make the player jump.")]
    private float jumpingPower = 16f;
    [SerializeField, Tooltip("Horizontal power applied for a wall jump.")]
    private float wallJumpXPower = 8f;
    [SerializeField, Tooltip("Vertical power applied for a wall jump.")]
    private float wallJumpYPower = 16f;
    [SerializeField, Tooltip("Should the player do a smaller jump if the jump button is short tapped?")]
    private bool shouldHop = true;
    [SerializeField, Tooltip("Ratio of hop vs full jump.")]
    private float hopRatio = 0.5f;
    [SerializeField, Tooltip("How long before the player is grounded can the player jump again, in seconds.")]
    private float jumpBufferTime = 0.1f;
    [SerializeField, Tooltip("How long after the player runs off a platform should the jump action still count, in seconds.")]
    private float coyoteTime = 0.1f;

    [SerializeField, Tooltip("An empty game object attachedd to the player detecting whether the player is on the ground.")]
    private Transform groundSensor;
    [SerializeField, Tooltip("The layer used by tilemaps or sprites that the player can stand on. ")]
    private LayerMask groundLayer;
    [SerializeField, Tooltip("An empty game object attachedd to the player detecting whether the player is touching a wall.")]
    private Transform wallSensor;
    [SerializeField, Tooltip("The layer used by the tilemap with the wall tiles, for wall sliding and jumping.")]
    private LayerMask wallLayer;

    [SerializeField, Tooltip("Idle animation.")]
    private AnimationClip animIdle;
    [SerializeField, Tooltip("Walk animation.")]
    private AnimationClip animWalk;
    [SerializeField, Tooltip("Animation to be played while the player is in the air and moving upwards.")]
    private AnimationClip animJumpUp;
    [SerializeField, Tooltip("Animation to be played while the player is in the air and moving downwards.")]
    private AnimationClip animJumpDown;
    [SerializeField, Tooltip("Wall slide animation.")]
    private AnimationClip animWallSlide;
    [SerializeField, Tooltip("Death animation.")]
    private AnimationClip animDeath;
    [SerializeField, Tooltip("Success animation (finished level).")]
    private AnimationClip animSuccess;

    [SerializeField, Tooltip("Indicates whether the player is alive. Displayed only for debugging purposes.")]
    private bool isAlive = true;
    [SerializeField, Tooltip("The player dies if it falls below this Y level.")]
    private float deadBelowY = -5f;
    [SerializeField, Tooltip("How long to wait after the player dies to load the next scene.")]
    private float delayNextScene = 2f;
    [SerializeField, Tooltip("The name of the scene to be loaded if the player fails. Leave empty to reload the current scene.")]
    private String nextSceneFailure;
    [SerializeField, Tooltip("The name of the scene to be loaded if the player finishes the level.")]
    private String nextSceneSuccess;

    [SerializeField] private LayerMask damageLayer;
    [SerializeField] private LayerMask coinLayer;
    [SerializeField] private LayerMask successLayer;

    [SerializeField] private GameObject musicSource;
    [SerializeField] private GameObject audioFxSource;

    private enum AnimationState
    {
        kDefault,
        kIdle,
        kWalk,
        kJumpUp, kJumpDown,
        kWallSlide,
        kDead,
        kSuccess
    }

    private AnimationState animState;

    private void PlayAnimation(AnimationClip clip)
    {
        if (animator == null || clip == null) return;
        animator.Play(clip.name);
    }

    private void SetAnimationState(AnimationState state)
    {
        if (animState == state) return;

        animState = state;

        switch (state)
        {
            case AnimationState.kIdle:

                PlayAnimation(animIdle);
                break;
            case AnimationState.kWalk:
                PlayAnimation(animWalk);
                break;
            case AnimationState.kJumpUp:
                PlayAnimation(animJumpUp);
                PlaySoundFx("Jump");
                break;
            case AnimationState.kJumpDown:
                PlayAnimation(animJumpDown);
                break;
            case AnimationState.kWallSlide:
                PlayAnimation(animWallSlide);
                PlaySoundFx("WallSlide");
                break;
            case AnimationState.kDead:
                StopMusic();
                DeleteTrail();
                PlaySoundFx("Death");
                PlaySoundFx("GameOver");
                PlayAnimation(animDeath);
                break;
            case AnimationState.kSuccess:
                StopMusic();
                PlaySoundFx("Victory");
                PlayAnimation(animSuccess);
                break;
            default:
                break;

        }
    }

    private void PlaySoundFx(String fxName)
    {
        if (!aFxPlayer) return;
        aFxPlayer.PlayFxByName(fxName);
    }

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        numCoinsCollected = 0;
        try
        {
            aFxPlayer = audioFxSource.GetComponent(typeof(AudioFxPlayer)) as AudioFxPlayer;
        }
        catch
        {

        }
    }

    void Start()
    {

    }

    void Update()
    {
        if (isAlive)
        {
            UpdateAlive();
            UpdateAnimationState();
        }
        else
        {
            UpdateDead();
        }
    }

    private void UpdateAlive()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (coyoteTimeCounter > 0 && jumpBufferCounter > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            jumpBufferCounter = 0;
        }

        if (shouldHop && Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * hopRatio);
            coyoteTimeCounter = 0;
        }

        if (wallSensor)
        {
            WallSlide();
            WallJump();
        }

        if (!isWallJumping)
        {
            Flip();
        }

    }

    private void UpdateDead()
    {

    }

    private void LoadNextScene()
    {
        String nextScene = null;
        if (animState == AnimationState.kDead)
        {
            nextScene = nextSceneFailure;
        }
        else if (animState == AnimationState.kSuccess)
        {
            nextScene = nextSceneSuccess;
        }

        if (!String.IsNullOrEmpty(nextScene))
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void UpdateAnimationState()
    {
        if (rb.bodyType == RigidbodyType2D.Static)
        {
            isAlive = false;
            Invoke(nameof(LoadNextScene), delayNextScene);
            SetAnimationState(AnimationState.kDead);
            return;
        }

        if (IsGrounded())
        {
            if (animState==AnimationState.kJumpDown)
            {
                PlaySoundFx("Land");
            }
            if (IsZeroish(rb.velocity.x))
            {
                SetAnimationState(AnimationState.kIdle);
                return;
            }
            else
            {
                SetAnimationState(AnimationState.kWalk);
                return;
            }
        }
        else
        {
            if (isWallSliding)
            {
                SetAnimationState(AnimationState.kWallSlide);
                return;
            }
            if (rb.velocity.y > 0.1f)
            {
                SetAnimationState(AnimationState.kJumpUp);
                return;
            }
            else
            {
                SetAnimationState(AnimationState.kJumpDown);
                return;
            }
        }
    }

    private bool IsZeroish(float v)
    {
        return Mathf.Abs(v) < 0.001f;
    }

    private void FixedUpdate()
    {
        if (rb.bodyType == RigidbodyType2D.Static) return;

        if (!isControllable)
        {
            rb.velocity *= 0.9f;
            return;
        }

        if (isWallJumping)
        {
            return;
        }

        float vx = rb.velocity.x;

        // turning around?
        if (!IsZeroish(horizontal) && (Mathf.Sign(vx) != Mathf.Sign(horizontal)))
        {
            vx = 0;
        }

        float deceleration = IsGrounded() ? groundDeceleration : airDeceleration;

        float r = Mathf.Abs(horizontal) > 0 ? acceleration : deceleration;

        vx = Mathf.Lerp(vx, horizontal * speed, r);
        if (IsZeroish(horizontal) && Mathf.Abs(vx) < 0.5f)
        {
            vx = 0.0f;
        }
        rb.velocity = new Vector2(vx, rb.velocity.y);

        if (rb.position.y < deadBelowY)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundSensor.position, 0.2f, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallSensor.position, 0.2f, wallLayer);
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpXPower, wallJumpYPower);
            wallJumpingCounter = 0f;

            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }


    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void CollectCoin(GameObject coin)
    {
        numCoinsCollected += 1;
        Debug.Log("collected coins: " + numCoinsCollected);
        PlaySoundFx("Coin");
        Destroy(coin, 0f);
    }

    private void StopMusic()
    {
        if (!musicSource) return;
        AudioSource audioSource = musicSource.GetComponent<AudioSource>();
        audioSource.Stop();
    }

    private void DeleteTrail()
    {
        foreach (Transform child in this.transform)
        {
            //Debug.Log("child:" + child.name);
            if (child.name=="Trail")
            {
                Destroy(child.gameObject);
                return;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject o = collision.gameObject;

        if ((successLayer & (1 << o.layer)) != 0)
        {
            isAlive = false;
            isControllable = false;
            SetAnimationState(AnimationState.kSuccess);
            Invoke(nameof(LoadNextScene), delayNextScene);
            return;
        }
        else if ((damageLayer & (1 << o.layer)) != 0)
        {
            isAlive = false;
            isControllable = false;
            SetAnimationState(AnimationState.kDead);
            Invoke(nameof(LoadNextScene), delayNextScene);
            return;
        }
        else if ((coinLayer & (1 << o.layer)) != 0)
        {
            CollectCoin(o);
            return;
        }


    }
}