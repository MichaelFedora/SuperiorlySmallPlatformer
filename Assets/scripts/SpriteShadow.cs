using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class SpriteShadow : MonoBehaviour {

    // This is an edited version of the one JesseAlexander posted on the Unity forums
    // see: http://forum.unity3d.com/threads/why-cant-sprites-gameobjects-cast-shadows.215461/#post-1454805

    public ShadowCastingMode castingMode;
    public bool recieveShadows;

    void Awake() {
        SpriteRenderer sprite = this.GetComponent<SpriteRenderer>();
        sprite.shadowCastingMode = castingMode;
        sprite.receiveShadows = recieveShadows;
    }
}
