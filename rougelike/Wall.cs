using UnityEngine;
using System.Collections;

//unchanged

public class Wall : MonoBehaviour {

    public Sprite dmgSprite;
    public int hp = 4;

    public AudioClip chopSound1, chopSound2;

    private SpriteRenderer spriteRenderer;
    
    // Use this for initialization
	void Awake ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
	}

    public void DamageWall( int loss)
    {
        spriteRenderer.sprite = dmgSprite;
        hp -= loss;

        SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);

        if (hp <= 0)
        {
            gameObject.SetActive(false);
        }
    }
	
	
}
