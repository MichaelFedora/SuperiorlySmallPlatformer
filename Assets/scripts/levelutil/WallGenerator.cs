using UnityEngine;
using System.Collections;

public class WallGenerator : MonoBehaviour {

    public Vector2 size;
    public Transform tilePrefab;
    public bool collide;

    void Awake() {

        //float offset = (size.x % 2 == 0) ? -0.5f : 0.0f;

        if(collide) {
            BoxCollider2D box = this.gameObject.AddComponent<BoxCollider2D>();
            box.offset = new Vector2(-0.5f, -0.5f);
            box.size = size;
        }

        for(int i_y = 0; i_y < size.y; i_y++) {
            for(int i_x = 0; i_x < size.x; i_x++) {
                Transform t = Instantiate(tilePrefab);
                t.SetParent(this.transform, false);
                t.Translate(new Vector2(-size.x / 2 + i_x, -size.y/2 + i_y));
            }
        }
    }
}
