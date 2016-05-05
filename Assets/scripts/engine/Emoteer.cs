using UnityEngine;
using System.Collections;

public class Emoteer : MonoBehaviour {

    // WIP

	public void display(EmoteType type) {

        this.GetComponent<SpriteRenderer>().enabled = true;
        this.GetComponent<Animator>().SetBool(type.ToString().ToLower(), true);
    }

    public void stop() {
        this.GetComponent<SpriteRenderer>().enabled = false;
    }
}
