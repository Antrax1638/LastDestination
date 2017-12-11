using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour 
{
	public string Name = "Projectile";
	public float Speed = 0;
	public float MaxLifeTime = 1.5f;
    public float TimeToInvoke;
    public int Id;
    public bool Raycast;

    [Header("Stage")]
    public bool Stage;
    public float StageTime = 30.0f;

    [HideInInspector]
    public Vector2 Direction = new Vector2();

	private Rigidbody2D BodyComponent;
    private Collider2D ColliderComponent;
	private SpriteRenderer SpriteComponent;
	private float LifeTime,Energy;
    RaycastHit2D RayHit;

	void Awake()
	{
        BodyComponent = GetComponent<Rigidbody2D>();
		ColliderComponent = GetComponent<Collider2D> ();
		SpriteComponent = GetComponent<SpriteRenderer> ();
		gameObject.name = Name;
	}

    void FixedUpdate()
    {
        if(Raycast) RayHit = Physics2D.Raycast(transform.position, BodyComponent.velocity);
    }

	void Update()
	{
		LifeTime += Time.deltaTime;
        Energy = (BodyComponent.mass * Mathf.Pow(BodyComponent.velocity.magnitude, 2) / 2);
        
        if ( LifeTime > MaxLifeTime && MaxLifeTime > 0.0f)
            Disable();
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		if (col.gameObject.tag == "Wall" || col.gameObject.tag == "Prop") 
		{
            Disable();
		}

	}

	//Funciones:
	public void Launch()
	{
        BodyComponent.AddForce(Direction * Speed, ForceMode2D.Impulse);
        if (Stage && StageTime <= MaxLifeTime) Invoke("Stage0", StageTime);
        Invoke("OnLaunch", TimeToInvoke);
	}

    public virtual void OnLaunch() 
    {
      
        Debug.Log("Launched " + gameObject.name);
    }

	public void Disable()
    {
        gameObject.SetActive(false);
        BodyComponent.simulated = false;
        BodyComponent.velocity = Vector2.zero;
        BodyComponent.isKinematic = true;
        LifeTime = 0;
    }

    public void Enable()
    {
        gameObject.SetActive(true);
        BodyComponent.simulated = true;
        BodyComponent.velocity = Vector2.zero;
        BodyComponent.isKinematic = false;
        LifeTime = 0;
    }

    public virtual void Stage0()
    {
        Debug.Log("Stage 0");
    }

    public float GetEnergy()
    {
        return Energy;
    }
}
