using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public class Player : Entity {

    public class AnimType {

        public static AnimType B_CROUCH = new AnimType("crouch");
        public static AnimType B_WALK = new AnimType("walk");
        public static AnimType B_FALL = new AnimType("falling");
        public static AnimType T_DASH = new AnimType("dash");
        public static AnimType T_JUMP = new AnimType("jump");
        public static AnimType T_FELLHARD = new AnimType("fellhard");

        public static explicit operator int(AnimType type) {
            return type.hash;
        }

        public static implicit operator string(AnimType type) {
            return type.id;
        }

        public string id { get; private set; }
        public int hash { get; private set; }

        private AnimType(string name) {
            this.id = name;
            this.hash = Animator.StringToHash(name);
        }
    }

    public Player() : base() {
        UserInputManager.OnKeyInput.AddListener(OnKeyInput);
    }

    ~Player() {
        UserInputManager.OnKeyInput.RemoveListener(OnKeyInput);
    }

    [Header("Player")]
    public int MaxJumps;
    public float DashSpeed;
    public int DashTicks;

    [Space(14.0f)]
    public Transform Poof;

    [Space(14.0f)]
    public Transform FustratedEmote;
    public Transform ExclimationEmote;
    public Transform WorkingEmote;
    public Transform SuccessEmote;
    public Transform FailureEmote;


    bool climb = false;
    bool drop = false;

    bool walkRight = false;
    bool walkLeft = false;

    int getWalkDir() {
        if(walkRight == walkLeft) return 0;

        return (walkRight) ? 1 : -1;
    }

    bool dash = false;
    bool dashHold = false;

    bool jump = false;
    bool jumpHold = false;
    int jumpNum = 0;

    bool crouch = false;

    int aimDir = 0;

	public void OnKeyInput(string name, bool state) {

        switch(name) {
            case "climb": this.climb = state; break;
            case "right": walkRight = state; break;
            case "left": walkLeft = state; break;
            case "drop": this.drop = state; break;
            case "dash": this.dash = state; break;
            case "jump": this.jump = state; break;
            case "crouch": this.crouch = state; break;
            //case "interact": interact(); break;
        }

    }

    public void interact() {
        Instantiate(this.ExclimationEmote).transform.Translate(0, 1, 0);
    }

    public override void FixedUpdate() {
        base.FixedUpdate();

        Animator anim = this.GetComponent<Animator>();
        Rigidbody2D rb = this.GetComponent<Rigidbody2D>();

        if(!this.dash)
            this.dashHold = false;

        if(!this.jump)
            this.jumpHold = false;

        if(base.CollFloor)
            this.jumpNum = 0;

        anim.SetBool(AnimType.B_CROUCH, crouch);

        bool canWalk = true;
        int walkDir = getWalkDir();

        if(this.dash && !this.dashHold) {

            int dashDir = 0;

            if(walkDir != 0)
                dashDir = walkDir;
            else
                dashDir = aimDir;

            canWalk = false;
            this.dashHold = true;

            poof();

            if(dashDir * rb.velocity.x < this.DashSpeed)
                rb.AddForce(new Vector2(((dashDir * this.DashSpeed) - rb.velocity.x) * rb.mass, 0), ForceMode2D.Impulse);

            anim.SetBool(AnimType.T_DASH, true);

        }

        if(canWalk && (this.walkLeft != this.walkRight)) {

            // same direction would be positive ][ or if we're not going as fast as we could be going
            if(walkDir * rb.velocity.x < 0 || Math.Abs(rb.velocity.x) < base.Speed) {
                rb.AddForce(new Vector2((walkDir * base.Speed - rb.velocity.x) * rb.mass, 0), ForceMode2D.Impulse);
            }

            if(!anim.GetBool(AnimType.B_WALK))
                anim.SetBool(AnimType.B_WALK, true);

        } else {
            anim.SetBool(AnimType.B_WALK, false);
        }

        if(this.jump && !this.jumpHold && this.jumpNum < this.MaxJumps) {

            this.jumpNum++;

            if(rb.velocity.y < base.JumpVel)
                rb.AddForce(new Vector2(0, (base.JumpVel - rb.velocity.y) * rb.mass), ForceMode2D.Impulse);

            anim.SetBool(AnimType.B_FALL, false);
            anim.SetBool(AnimType.T_JUMP, true);

        } else if(rb.velocity.y < 0 && !(base.CollFloor && this.walkLeft != this.walkRight)) {

            anim.SetBool(AnimType.B_FALL, true);

        } else {

            anim.SetBool(AnimType.B_FALL, false);
        }

    }

    public void poof() {
        // instantiate the poof prefab :P
        Instantiate(this.Poof, this.transform.position, this.transform.rotation);
    }
}
