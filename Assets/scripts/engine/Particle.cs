using UnityEngine;
using System.Collections.Generic;

public class Particle : MonoBehaviour {

    public float LifeTime = 0.0f;

    public void FixedUpdate() {
        if(this.LifeTime != 0.0f) {
            this.LifeTime -= Time.deltaTime;

            if(this.LifeTime < 0)
                selfDestruct();
        }
    }

    public void selfDestruct() {
        Destroy(this.gameObject);
    }
}
