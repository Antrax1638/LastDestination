using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{
    public float Health = 1000,Stamina = 100;
    public int Armor = 0;
    [HideInInspector]
    public bool isAlive = true;
	
    [Header("UI Compoents")]
    public Image HealthComponent;

    private float DamageTaken,MaxHealth;
   
    void Start() 
    {
        MaxHealth = Health;
    }

	void Update () 
    {
        if (Health <= 0)
        {
            Disable();
        }
	}

    public void TakeDamage(float Damage)
    {
        float DamageMultiplier = 100.0f / (100.0f + Armor);
        print(DamageMultiplier);
        Health -= (Damage * DamageMultiplier);
        HealthComponent.fillAmount = Health / MaxHealth;
        DamageTaken = Mathf.Clamp(DamageTaken + Damage * DamageMultiplier, 0, MaxHealth);
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.transform.tag == "Projectile")
        {
            Projectile ProjectileObject = coll.gameObject.GetComponent<Projectile>();
            TakeDamage(ProjectileObject.GetEnergy());
            ProjectileObject.Disable();
        }
    }

    void Enable()
    {
        transform.gameObject.SetActive(true);
        Health = DamageTaken;
        MaxHealth = Health;
        DamageTaken = 0;
        Armor = 0;
        isAlive = true;
    }

    void Disable() 
    {
        transform.gameObject.SetActive(false);
        isAlive = false;
    }
}
