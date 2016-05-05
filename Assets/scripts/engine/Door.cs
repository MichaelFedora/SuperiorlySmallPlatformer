using UnityEngine;
using System.Collections;

public class Door : ExitHandler, Interactable {

    public EmoteType getReaction() {
        return EmoteType.EXCLAMATION;
    }

    public void interact() {
        this.GetComponent<Animator>().SetBool("open", true);
        World.instance.switchRoom(base.room, base.spawnId);
    }
}
