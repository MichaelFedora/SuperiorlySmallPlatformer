using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public class Player : Entity {

    class AnimType {

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
    public float HardFallKnockoutTime;

    [Space(14.0f)]
    public Transform DashPoof;
    public Transform JumpPoof;

    // == Other Reference Variables

    Transform StandardHeadCollider;
    Transform CrouchHeadCollider;

    Transform EmoteerObj;

    public void Awake() {
        this.StandardHeadCollider = this.transform.FindChild("Standard");
        this.CrouchHeadCollider = this.transform.FindChild("Crouch");
        this.EmoteerObj = this.transform.FindChild("Emoteer");
    }

    // == Logic Variables

    bool climb = false;
    bool drop = false;

    bool walkRight = false;
    bool walkLeft = false;

    int getWalkDir() {
        if(walkRight == walkLeft) return 0;

        return (walkRight) ? 1 : -1;
    }

    bool dir = false; // false = right, true = left

    bool dash = false;
    bool dashHold = false;

    bool jump = false;
    bool jumpHold = false;
    int jumpNum = 0;

    bool crouch = false;

    int aimDir = 0;

    float knockoutTimer = 0.0f;

	void OnKeyInput(string name, bool state) {

        switch(name) {
            case "climb": this.climb = state; break;
            case "right": walkRight = state; break;
            case "left": walkLeft = state; break;
            case "drop": this.drop = state; break;
            case "dash": this.dash = state; break;
            case "jump": this.jump = state; break;
            case "crouch": this.crouch = state; break;
            case "interact": interact(); break;
        }

    }

    Interactable interactable;
    void OnTriggerEnter2D(Collider2D coll) {
        Interactable obj = coll.GetComponent<Interactable>();
        if(obj != null) {
            interactable = obj;
            this.EmoteerObj.GetComponent<Emoteer>().display(obj.getReaction());
        }
    }

    void OnTriggerExit2D(Collider2D coll) {
        Interactable obj = coll.GetComponent<Interactable>();
        if(obj != null)
            if(obj == interactable) {
                interactable = null;
                this.EmoteerObj.GetComponent<Emoteer>().stop();
            }
    }

    void interact() {
        if(interactable != null)
            interactable.interact();
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

        if(knockoutTimer > 0.0f) {
            knockoutTimer -= Time.fixedDeltaTime;
            return;
        }

        if(getWalkDir() != 0)
            this.dir = getWalkDir() < 0;

        SpriteRenderer sprite = this.GetComponent<SpriteRenderer>();
        sprite.flipX = this.dir;

        anim.SetBool(AnimType.B_CROUCH, crouch);

        if(this.crouch) {
            // if we're falling, and hit, groundpound (strength depending on speed) :P

            if(this.StandardHeadCollider.gameObject.activeSelf)
                this.StandardHeadCollider.gameObject.SetActive(false);

            if(!this.CrouchHeadCollider.gameObject.activeSelf)
                this.CrouchHeadCollider.gameObject.SetActive(true);
        } else {

            if(!this.StandardHeadCollider.gameObject.activeSelf)
                this.StandardHeadCollider.gameObject.SetActive(true);

            if(this.CrouchHeadCollider.gameObject.activeSelf)
                this.CrouchHeadCollider.gameObject.SetActive(false);
        }

        if(this.drop) {

            // push the player a little to get them going?

            if(LayerMask.LayerToName(this.gameObject.layer) == "Entity") {
                this.gameObject.layer = LayerMask.NameToLayer("EntityIgnorePlatform");
                for(int i = 0; i < this.transform.childCount; i++) {
                    Transform child = this.transform.GetChild(i);
                    child.gameObject.layer = LayerMask.NameToLayer("EntityIgnorePlatform");
                }
            }

        } else {

            if(LayerMask.LayerToName(this.gameObject.layer) == "EntityIgnorePlatform") {
                this.gameObject.layer = LayerMask.NameToLayer("Entity");
                for(int i = 0; i < this.transform.childCount; i++) {
                    Transform child = this.transform.GetChild(i);
                    child.gameObject.layer = LayerMask.NameToLayer("Entity");
                }
            }
        }

        if(this.CollFloor && rb.velocity.y < -9.8f * rb.gravityScale * 0.8f) {

            anim.SetBool(AnimType.T_FELLHARD, true);

            knockoutTimer = HardFallKnockoutTime;

            return;
        }

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

            Instantiate(DashPoof).Translate(this.transform.position);

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

            this.jumpHold = true;

            this.jumpNum++;

            if(jumpNum > 1)
                Instantiate(JumpPoof).Translate(this.transform.position);

            if(rb.velocity.y < base.JumpVel)
                rb.AddForce(new Vector2(0, (base.JumpVel - rb.velocity.y) * rb.mass), ForceMode2D.Impulse);

            anim.SetBool(AnimType.T_JUMP, true);
            anim.SetBool(AnimType.B_FALL, true);

        } else {

            if(rb.velocity.magnitude > 9.8f * rb.gravityScale * 0.2f && !(base.CollFloor && this.walkLeft != this.walkRight)) {

                anim.SetBool(AnimType.B_FALL, true);

            } else {

                anim.SetBool(AnimType.B_FALL, false);
            }
        }

    }
}
