using UnityEngine;
using System.Collections;

public class ExitTrigger : ExitHandler {

    public void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("Entity") || other.gameObject.layer == LayerMask.NameToLayer("EntityIgnorePlatform")) {
            World.instance.switchRoom(base.room, base.spawnId);
        }
    }
}
