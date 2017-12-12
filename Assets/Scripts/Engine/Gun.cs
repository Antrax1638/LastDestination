using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public enum GunFireMode
{
	Safe,
	Single,
	Auto,
	Burst,
	Load,
}

public enum GunReloadMode
{ 
    None,
    Tap,
    Full,
    One,
    Charge,
    Regenerate,
}

[System.Serializable]
public struct GunFireType
{
    public GunFireMode Mode;
    public string Name;

}

public class Gun : MonoBehaviour 
{
	//Publicos:
    [Header("Audio")]
    public AudioSource Shoot;

    [Header("Gun Stats")]
	public bool LookAtMouse;
	public GunFireMode FireMode;
	public int BurstSize;
	public int Magazine;
    public int CurrentMagSize;
	public int Ammo;
    public float LoadMultiplier = 1.0f;
    [Tooltip("Fire rate -Bullets per second-")]
    public float Rate;

    [Header("Reload")]
    public GunReloadMode ReloadMode = GunReloadMode.Full;
    public string ReloadName = "Reload";
    public float ReloadTime = 1.0f;
    public float ReloadSensibility = 0.05f;
    public bool Chamber;
    public Vector2 ReloadTapZone = new Vector2(75.0f, 85.0f);

    [Header("Projectile")]
    public GameObject Projectile;
    public float ProjectileSpeed = 10.0f;
    public bool ProjectilePoolExpandable = false;
    public int ProjectilePoolSize = 20;
    public float ProjectilePoolRate = 35.0f;

    [Header("Recoil")]
    public bool Recoil;
    public float RecoilForce;
    public int RecoilDirection = 1;


	//Privado:
	private SpriteRenderer SpriteComponent;
    private Camera CameraComponent;
	private Vector3 MouseWorld;
	private float HandRotation;
	private Player PlayerSc;
	private Transform BSpawn;
    private float LoadState, DeltaRate, ReloadTap, ReloadCharge;
    private int DeltaMagazine = 0, FinalMagSize = 0,CurrentAmmo = 0;
    private bool ReloadInvoke = false;
    private Player PlayerComponent;

    private List<GameObject> ProjectilePool = new List<GameObject>();

	void Awake()
    {
        SpriteComponent = GetComponent<SpriteRenderer>();
		BSpawn = GetComponentInChildren<Transform> ();
        CameraComponent = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        PlayerComponent = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

	}

	void Start()
	{
		PlayerSc = GetComponentInParent<Player> ();
        
        for (int i = 0; i < ProjectilePoolSize; i++) 
        {
            var Obj = Instantiate(Projectile, transform.position,transform.rotation);
            Obj.SetActive(false);
            ProjectilePool.Add(Obj);
        }

        if(ProjectilePoolExpandable)
            InvokeRepeating("ProjectilePoolTrash", 0.5f, ProjectilePoolRate);
	}

	void Update () 
	{
        DeltaRate += Time.deltaTime;
        
        //LookAtMouse:
		if (LookAtMouse) 
        {
            MouseWorld = CameraComponent.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, CameraComponent.nearClipPlane));
            HandRotation = Mathf.Atan2(MouseWorld.y - SpriteComponent.transform.position.y, MouseWorld.x - SpriteComponent.transform.position.x);
			HandRotation = HandRotation * (180.0f / Mathf.PI);
            //SpriteComponent.transform.localRotation = Quaternion.Euler(0, 0, HandRotation);
		}
        

		PrimaryFire ();
	    Reload();

		if (Input.GetKeyDown(KeyCode.Q))
			SwitchFireMode ();
		
	}

	public void PrimaryFire ()
	{
		if (CurrentMagSize > 0) 
		{
			switch (FireMode) 
			{
			case GunFireMode.Safe: break;
			case GunFireMode.Single: 
                if (Input.GetButtonDown("Fire")) 
                    CreateBullet (ProjectileSpeed); 
            break;

			case GunFireMode.Auto: 
                if (Input.GetButton("Fire")) 
                    CreateBullet (ProjectileSpeed);
            break;

			case GunFireMode.Burst:
                if (Input.GetButtonDown("Fire"))
                    for (int i = 0; i < BurstSize; i++) 
                    {
                        CreateBullet(ProjectileSpeed);
                    }
						
				break;
		
			case GunFireMode.Load:
                if (Input.GetButton("Fire"))
                    LoadState += (Time.deltaTime * LoadMultiplier);

                if (Input.GetButtonUp("Fire"))
                {
					CreateBullet (LoadState*ProjectileSpeed);
					LoadState = 0;
				}
				break;
			}
		}
	}
		
	public void Reload()
	{
        if (ReloadInvoke)
            return;
        
        DeltaMagazine = Magazine - CurrentMagSize;
        switch (ReloadMode) 
        {
            default: break;
            case GunReloadMode.Tap:
                if (Input.GetButtonDown(ReloadName))
                {
                    ReloadTap = Mathf.Lerp(ReloadTap, 100, Time.deltaTime);
                    if (ReloadTap >= 100)
                        ReloadTap = 0;
                    print(ReloadTap);
                    if (Input.GetButtonDown(ReloadName) && ReloadTap >= ReloadTapZone.x && ReloadTap <= ReloadTapZone.y)
                    {
                        CurrentAmmo = DeltaMagazine;
                        FinalMagSize = Magazine;
                        ReloadInvoke = true;
                    }
                    else
                    {
                        CurrentAmmo = DeltaMagazine;
                        FinalMagSize = Magazine;
                        ReloadInvoke = true;
                    }
                    Invoke("OnReload", ReloadTime);
                }
                
                break;

            case GunReloadMode.Full:
                if (Input.GetButtonDown(ReloadName) && Ammo > DeltaMagazine) 
                {
                    ReloadInvoke = true;
                    CurrentAmmo = DeltaMagazine;
                    FinalMagSize = Magazine;
                    Invoke("OnReload", ReloadTime);
                }
                break;

            case GunReloadMode.One:
                if (Input.GetButtonDown(ReloadName) && CurrentMagSize < Magazine && Ammo > (Ammo-1))
                {
                    FinalMagSize = CurrentMagSize;
                    ReloadInvoke = true;
                    CurrentAmmo = 1;
                    FinalMagSize++;
                    Invoke("OnReload", 0);
                }
                break;

            case GunReloadMode.Charge:
                if(Input.GetButtonDown(ReloadName))
                    ReloadCharge = CurrentMagSize;
                
                if (Input.GetButton(ReloadName) && Ammo > 0)
                {
                    
                    ReloadCharge += (Time.deltaTime + (Input.GetAxis(ReloadName)*ReloadSensibility));
                    ReloadCharge = Mathf.Clamp(ReloadCharge, 0, Magazine);
                    print(ReloadCharge);
                    
                }

                if (Input.GetButtonUp(ReloadName))
                {
                    ReloadInvoke = true;
                    CurrentAmmo = (int)ReloadCharge;
                    FinalMagSize = (int)ReloadCharge;
                    Invoke("OnReload", ReloadTime);
                    ReloadCharge = 0;
                }
                break;

            case GunReloadMode.Regenerate:
                
                ReloadCharge += (Time.deltaTime);
                if (ReloadCharge > ReloadTime)
                {
                    ReloadCharge = 0;
                    CurrentMagSize++;
                    CurrentMagSize = Mathf.Clamp(CurrentMagSize, 0, Magazine);
                }
                break;
        }


        FinalMagSize += (System.Convert.ToInt32(Chamber) * System.Convert.ToInt32(CurrentMagSize > 0));
        
	}

    protected void OnReload()
    {
        Ammo -= CurrentAmmo;
        CurrentMagSize = FinalMagSize;
        ReloadInvoke = false;
    }

	bool CreateBullet (float speed)
	{
        if (DeltaRate > (1.0f/Rate))
        {
            DeltaRate = 0;
            Projectile PScript;

            HandRotation += (RecoilForce * RecoilDirection) * Time.deltaTime;
            Vector2 Direction = Quaternion.Euler(0, 0, HandRotation) * Vector2.right;
            Shoot.Play();
            for (int i = 0; i < ProjectilePool.Count; i++)
            {
                if (ProjectilePool[i] != null && !ProjectilePool[i].activeInHierarchy)
                {
                    ProjectilePool[i].transform.position = transform.position;
                    ProjectilePool[i].transform.rotation = transform.rotation;
                    PScript = ProjectilePool[i].GetComponent<Projectile>();
                    PScript.Enable();
                    PScript.Direction = Direction;
                    PScript.Speed = speed;
                    PScript.Launch();
                    Physics2D.IgnoreCollision(PlayerComponent.GetComponent<CapsuleCollider2D>(), ProjectilePool[i].GetComponent<BoxCollider2D>());
                    CurrentMagSize--;
                    return true;
                }
            }

            if (ProjectilePoolExpandable)
            {
                var Obj = Instantiate(Projectile, transform.position, transform.rotation);
                var ObjScript = Obj.GetComponent<Projectile>();
                ObjScript.Direction = Direction;
                ObjScript.Speed = speed;
                ObjScript.Launch();
                CurrentMagSize--;
                Physics2D.IgnoreCollision(PlayerComponent.GetComponent<CapsuleCollider2D>(), Obj.GetComponent<BoxCollider2D>());
                ProjectilePool.Add(Obj);
                return true;
            }

            return false;

        }
        else
            return false;
	}

	void SwitchFireMode()
	{
		if (FireMode > GunFireMode.Load)
			FireMode = GunFireMode.Safe;
		else
			FireMode++;
	}

    void ProjectilePoolTrash()
    {
        for (int index = ProjectilePoolSize; index < ProjectilePool.Count; index++) 
        {
            if (ProjectilePool[index] != null && !ProjectilePool[index].activeInHierarchy) 
            {
                Destroy(ProjectilePool[index].gameObject);
                ProjectilePool.RemoveAt(index);
            }
        }
    }
}
