using UnityEngine;
using System.Collections;

public class PlatformGenerator : MonoBehaviour {

    public int width;
    public Transform tilePrefab;
    public bool oneWay;

    void Awake() {

        float offset = (width % 2 == 0) ? -0.5f : 0.0f;

        if(oneWay) {
            EdgeCollider2D edge = this.gameObject.AddComponent<EdgeCollider2D>();
            edge.usedByEffector = true;
            edge.points = new Vector2[] {
                new Vector2(-width / 2.0f + offset, 0.5f),
                new Vector2(width / 2.0f + offset, 0.5f) };

            PlatformEffector2D peff = this.gameObject.AddComponent<PlatformEffector2D>();
            peff.useOneWayGrouping = true;
            peff.colliderMask = 1 << LayerMask.NameToLayer("Entity");
        } else {
            BoxCollider2D box = this.gameObject.AddComponent<BoxCollider2D>();
            box.offset = new Vector2(offset, 0);
            box.size = new Vector2(width, 1);
        }

        for(int i = 0; i < width; i++) {
            Transform t = Instantiate(tilePrefab);
            t.SetParent(this.transform, false);
            t.Translate(new Vector2(-width / 2.0f + i, 0));
        }
    }
}
