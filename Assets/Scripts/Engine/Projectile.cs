using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour 
{
	public string Name = "Projectile";
	public Vector2 Direction = new Vector2();
	public float Speed = 0;
	public float MaxLifeTime = 0;
    public float TimeToInvoke = 1.5f;
	
	private Rigidbody2D ProjectileBody;
	private BoxCollider2D Collider;
	private SpriteRenderer ProjectileSprite;
	private float LifeTime;

	void Awake()
	{
		ProjectileBody = GetComponent<Rigidbody2D> ();
		Collider = GetComponent<BoxCollider2D> ();
		ProjectileSprite = GetComponent<SpriteRenderer> ();
		gameObject.name = Name;
	}
	
	void Update()
	{
		LifeTime += Time.deltaTime;
		if (gameObject != null && LifeTime > MaxLifeTime && MaxLifeTime > 0.0f)
			Destroy (this.gameObject);
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		if (col.gameObject.tag == "Wall" || col.gameObject.tag == "Prop") 
		{
            Destroy(gameObject);
		}

	}

	//Funciones:
	public void Launch()
	{
		ProjectileBody.AddForce (Direction * Speed,ForceMode2D.Impulse);
        Invoke("OnLaunch", TimeToInvoke);
	}

    public virtual void OnLaunch() 
    {
        Debug.Log("Launched " + gameObject.name);
    }

	
}
