using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Movement : MonoBehaviour
{
    private Collision coll;
    [HideInInspector]
    public Rigidbody2D rb;
    private AnimationScript anim;
    private CapsuleCollider2D hitbox;
    private SpriteRenderer sprite_renderer;
    private Transform location;
    private SpriteRenderer halo;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;
    // Joe's variables
    // private bool justToggled = false;
    private bool newFeatureEnabled = false;
    private bool distinctMovement = false;

    [Space]

    private bool groundTouch;
    private bool hasDashed;
    // BRANDON VARS:
    private bool coyoteTime;
    private bool coyoteTimeReset;
    // END BRANDON VARS

    public int side = 1;
    public Vector2 orgSize;
    GameObject ChildGameObject0;


    [Space]
    [Header("Polish")]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallJumpParticle;
    public ParticleSystem slideParticle;

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();
        hitbox = GetComponent<CapsuleCollider2D>();
        sprite_renderer = GetComponentInChildren<SpriteRenderer>();
        location = GetComponent<Transform>();
        ChildGameObject0 = this.transform.GetChild(3).gameObject;
        halo = ChildGameObject0.GetComponent<SpriteRenderer>();
        halo.enabled = false;
        orgSize = hitbox.size;

    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        //DEATH CODE
        if(location.transform.position.y < -8 && newFeatureEnabled){
            halo.enabled = true;
            StartCoroutine(respawnHalo());
            location.transform.position = new Vector2(-8.1f, -2.5f); //set these values for respawn loc
            rb.velocity = new Vector2(0,0);
        }

        Walk(dir);
        anim.SetHorizontalMovement(x, y, rb.velocity.y);

        //Added code
        //------------------------------
        if(Input.GetKeyDown(KeyCode.Alpha1)) {
            Debug.Log("Switching to Base Movement");
            newFeatureEnabled = false;
            speed = 7;
            jumpForce = 12;
            dashSpeed = 40;
        }
        if(Input.GetKeyDown(KeyCode.Alpha2)) {
            Debug.Log("Switching to Polished Movement");
            newFeatureEnabled = true;
            speed = 7;
            jumpForce = 12;
            dashSpeed = 40;
        }
        if(Input.GetKeyDown(KeyCode.Alpha3)){
            Debug.Log("Switching to Distinct Movement");
            newFeatureEnabled = true;
            distinctMovement = !distinctMovement;
            speed = 10;
            jumpForce = 15;
            dashSpeed = 30;
        }
        // if(Input.GetButtonDown("Fire3") && !justToggled)
        // {
        //     newFeatureEnabled = !newFeatureEnabled;
        //     justToggled = true;
        // }
        // if(Input.GetButtonUp("Fire3"))
        // {
        //     justToggled = false;
        // }
        //------------------------------

        if (coll.onWall && Input.GetButton("Fire2") && canMove)
        {
            if(side != coll.wallSide)
                anim.Flip(side*-1);
            wallGrab = true;
            wallSlide = false;
        }

        if (Input.GetButtonUp("Fire2") || !coll.onWall || !canMove)
        {
            wallGrab = false;
            //Line below is unnecessary?
            wallSlide = false;
        }

        if (coll.onGround && !isDashing)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
        }
        //Code Changed here
        //------------------------
        if(newFeatureEnabled == true){
            /*
            if(!coll.onGround && !coll.onWall)
            {   
                hitbox.size = new Vector2(orgSize.x - 0.2f, orgSize.y - 0.2f);
            }else{
                print("IM STUCK");
                hitbox.size = new Vector2(orgSize.x, orgSize.y);
            }
            */

            if(hasDashed)
            {
                sprite_renderer.color = new Color(0.6226f, 0.6226f, 0.6226f, 1.0f);
            }
            else
            {
                sprite_renderer.color = new Color(1, 1, 1, 1);
            }
        }
        //------------------------
        if (wallGrab && !isDashing)
        {
            rb.gravityScale = 0;
            if(x > .2f || x < -.2f)
            rb.velocity = new Vector2(rb.velocity.x, 0);

            float speedModifier = y > 0 ? .5f : 1;

            rb.velocity = new Vector2(rb.velocity.x, y * (speed * speedModifier));
        }
        else
        {
            rb.gravityScale = 3;
        }

        if(coll.onWall && !coll.onGround)
        {
            // Ivan's implementation: Checks to see if the player is dashing and touches a wall
            if (x != 0 && !wallGrab && !isDashing && newFeatureEnabled)
            {
                wallSlide = true;
                WallSlide();
            // If the implementation is not being used, keep the normal condition as is
            } else if(x != 0 && !wallGrab && !newFeatureEnabled) {
                wallSlide = true;
                WallSlide();
            }
        }

        // Ivan's implementation: checks to see if we are not dashing while touching the ground and not touching any walls
        if ((!coll.onWall || coll.onGround) && !isDashing && newFeatureEnabled)
            wallSlide = false;
        // If the implementation is not being used, keep the normal condition as is
        else if(!coll.onWall || coll.onGround)
            wallSlide = false;

        if (Input.GetButtonDown("Jump"))
        {
            anim.SetTrigger("jump");
            if (!newFeatureEnabled){
                if (coll.onGround)
                    Jump(Vector2.up, false);
                if (coll.onWall && !coll.onGround)
                    WallJump();
            }
            else{ // BRANDON EDIT
                if (coll.onWall && !coll.onGround)
                    WallJump();
                else if (coll.onGround || coyoteTime)
                    Jump(Vector2.up, false);
            } // END BRANDON EDIT
            
        }

        if (Input.GetButtonDown("Fire1") && !hasDashed)
        {
            if(xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);
        }

        if (!newFeatureEnabled) {
            if (coll.onGround && !groundTouch)
            {
                GroundTouch();
                groundTouch = true;
            }

            if(!coll.onGround && groundTouch)
            {
                groundTouch = false;
            }
        }
        else{   // BRANDON EDIT
            if (coll.onGround && !groundTouch)
            {
                GroundTouch();
                groundTouch = true;
                coyoteTimeReset = true;
            }

            if(!coll.onGround && groundTouch)
            {
                groundTouch = false;
                if(coyoteTimeReset){ StartCoroutine(CoyoteTime(0.1f)); }
            }
        }   // END SEGMENT
        WallParticle(y);

        if (wallGrab || wallSlide || !canMove)
            return;

        if(x > 0)
        {
            side = 1;
            anim.Flip(side);
        }
        if (x < 0)
        {
            side = -1;
            anim.Flip(side);
        }
    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;

        side = anim.sr.flipX ? -1 : 1;

        jumpParticle.Play();
    }

    // Ivan's implementation: Dash Collision Redirection
    // changes the velocity of the player depending if they are colliding against the ground or walls
    void DashAgainstWall(float y) {
        if(!distinctMovement) rb.velocity = Vector2.zero;   // when in base/polished, we set velocity of starting dash position to 0, else we keep the momentum of player
        Vector2 dir = new Vector2(0, y);
        rb.velocity += dir.normalized * dashSpeed;
    }
    void DashAgainstGround(float x) {
        if(!distinctMovement) rb.velocity = Vector2.zero;   // when in base/polished, we set velocity of starting dash position to 0, else we keep the momentum of player
        Vector2 dir = new Vector2(x, 0);
        rb.velocity += dir.normalized * dashSpeed;
    }

    private void Dash(float x, float y)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));

        hasDashed = true;

        anim.SetTrigger("dash");

        // Ivan's implementation: checks to see if player is colliding against any walls at the start of their dash
        if(coll.onWall && newFeatureEnabled) {
            DashAgainstWall(y);
        } else if(coll.onGround && newFeatureEnabled) {
            DashAgainstGround(x);
        } else {    // if the player is not colliding with surfaces or the player does not have implemented movement
            // when in base/polished, we set velocity of starting dash position to 0, else we keep the momentum of player
            if(!distinctMovement) rb.velocity = Vector2.zero;
            Vector2 dir = new Vector2(x, y);

            rb.velocity += dir.normalized * dashSpeed;
        }
        //Added Line below
        //hitbox.size = new Vector2(orgSize.x - 0.3f, orgSize.y - 0.3f);
        StartCoroutine(DashWait());
    }

    IEnumerator respawnHalo()
    {
        yield return new WaitForSeconds(0.75f);
        halo.enabled = false;
    }
    IEnumerator DashWait()
    {
        FindObjectOfType<GhostTrail>().ShowGhost();
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        dashParticle.Play();
        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;
        yield return new WaitForSeconds(.3f);

        dashParticle.Stop();
        rb.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (coll.onGround)
            hasDashed = false;
    }

    private void WallJump()
    {
        if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        {
            side *= -1;
            anim.Flip(side);
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

        wallJumped = true;
    }

    private void WallSlide()
    {
        bool goingUp = false;
        if(coll.wallSide != side)
         anim.Flip(side * -1);

        if (!canMove)
            return;

        bool pushingWall = false;
        
        if((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;
        //changed here allow
        //---------------
        if(newFeatureEnabled){
            if(rb.velocity.y > 0)
            {
                goingUp = true;
            }
            if(goingUp){
                rb.velocity = new Vector2(push, rb.velocity.y);
            }
            else{
                rb.velocity = new Vector2(push, -slideSpeed);
            }
        }else{
            rb.velocity = new Vector2(push, -slideSpeed);
        }
        //----------------
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        if (wallGrab)
            return;

        if (!wallJumped)
        {
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

        if(newFeatureEnabled){ // BRANDON EDIT
            StopCoroutine(CoyoteTime(0));
            coyoteTime = false;
            coyoteTimeReset = false;
        }

        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * jumpForce;

        particle.Play();
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    // Coroutine not called unless newFeatureEnabled. Part of Brandon's edits.
    IEnumerator CoyoteTime(float time) {
        coyoteTime = true;
        yield return new WaitForSeconds(time);
        groundTouch = false;
        coyoteTime = false;
    }

    void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    void WallParticle(float vertical)
    {
        var main = slideParticle.main;

        if (wallSlide || (wallGrab && vertical < 0))
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    int ParticleSide()
    {
        int particleSide = coll.onRightWall ? 1 : -1;
        return particleSide;
    }
}
