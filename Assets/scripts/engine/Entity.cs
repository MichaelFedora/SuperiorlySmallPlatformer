using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class Entity : MonoBehaviour {

    [Header("Entity")]
    public float Speed = 1.0f;
    public float JumpVel = 5.0f;

    [Space(14.0f)]
    public Vector2 DragCoeff = new Vector2(0, 0);
    public Vector2 LevelFriction = new Vector2(20.0f, 5.0f);

    [Space(14.0f)]
    public float CollThreshold = 1.0f;

    public Dictionary<string, UnityEvent<object[]>> Events = new Dictionary<string, UnityEvent<object[]>>();

    public bool CollFloor;// { get; protected set; }
    public bool CollWallLeft;// { get; protected set; }
    public bool CollWallRight;// { get; protected set; }
    public bool CollCieling;// { get; protected set; }

    public bool CollEntity;// { get; protected set; }

    public Entity() {

        CollFloor = CollWallLeft = CollWallRight = CollCieling = CollEntity = false;
    }

    public virtual void FixedUpdate() {
        /// Collision: Evertything
        //Vector2 tl = new Vector2(-fDistToEdges[1],fDistToEdges[0]);
        //Vector2 br = new Vector2(fDistToEdges[3],-fDistToEdges[2]);

        // the points lie on a box like so
        //
        //   2     1
        //   *--|--*|
        //   |  |   * 0
        //   --------
        // 3 *  |   |
        //   |*-|---* 5
        //    4
        //
        // the distance between 0 & 1 on the y axis is the CollThreshold
        // so the cieling "box" would have top left be 2 and bottom right be 0.

        int mask = 1 << LayerMask.NameToLayer("Level"); // TODO: Implement layers


        Vector2 collSize = this.GetComponent<BoxCollider2D>().size;

        Vector2 distToEdges = new Vector2(collSize.x / 2.0f, collSize.y / 2.0f);

        // NOTE: So this is slightly old logic, recycled from earlier projects
        // but I noticed that it worked better by INSTEAD of having the

        Vector2[] points = new Vector2[6] {
            new Vector2( distToEdges.x,                         distToEdges.y + CollThreshold),
            new Vector2( distToEdges.x + CollThreshold,    distToEdges.y),
            new Vector2(-distToEdges.x,                         distToEdges.y),
            new Vector2(-distToEdges.x,                        -(distToEdges.y + CollThreshold)),
            new Vector2(-(distToEdges.x + CollThreshold), -distToEdges.y),
            new Vector2( distToEdges.x,                        -distToEdges.y)
        };

        Vector2 tl = new Vector2(0, 0);
        Vector2 br = new Vector2(0, 0);

        /// Collision: Floor

        tl = points[3] + (Vector2)this.transform.position;
        br = points[5] + (Vector2)this.transform.position;

        this.CollFloor = Physics2D.OverlapArea(tl, br, mask);

        ///Collision: Wall - Left

        tl = points[2] + (Vector2)this.transform.position;
        br = points[4] + (Vector2)this.transform.position;

        this.CollWallLeft = Physics2D.OverlapArea(tl, br, mask);

        /// Collision: Wall - Right

        tl = points[1] + (Vector2)this.transform.position;
        br = points[5] + (Vector2)this.transform.position;

        this.CollWallRight = Physics2D.OverlapArea(tl, br, mask);

        /// Collision: Cieling

        tl = points[2] + (Vector2)this.transform.position;
        br = points[0] + (Vector2)this.transform.position;

        this.CollCieling = Physics2D.OverlapArea(tl, br, mask);

        /// Collision: Level Entity (check whole area)
        // TODO: Neccesity of "LevelEntity" check
        /*mask = 1 << LayerMask.NameToLayer("LevelEntity");
		
		tl = new Vector2(-distToEdges.x, distToEdges.y) + (Vector2) this.transform.position;
		br = new Vector2(distToEdges.x, -distToEdges.y) + (Vector2) this.transform.position;
		
		this.LvlEnt = Physics2D.OverlapArea(tl, br, mask);*/

        /// Physics: Simulate Drag

        Vector2 dc = new Vector2(DragCoeff.x * 0.01f, DragCoeff.y * 0.01f);
        Rigidbody2D rb = this.GetComponent<Rigidbody2D>();


        if(CollFloor)
            dc.x = LevelFriction.x * 0.01f;

        if(CollWallLeft || CollWallRight)
            dc.y = LevelFriction.y * 0.01f;

        rb.AddForce(
            -new Vector2(
                rb.velocity.x * dc.x,
                rb.velocity.y * dc.y
            ) * rb.mass,
            ForceMode2D.Impulse
        );

        // ====== ===== =====
    }
}
