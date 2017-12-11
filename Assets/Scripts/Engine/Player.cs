using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum Gender 
{
	Male = 0,
	Female = 1,
	None = 2

}
		
public enum Stance
{
	Standing = 3,
	Crouched,
	Down,
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class Player : MonoBehaviour
{
    //General Control System:
    public Gender PlayerGender = Gender.None;
    public bool Dash;
    public float DashForce = 0.0f;
    public int IdleIndex = 0;
    public float DeltaTime;
    public string FemaleIdleName = "Female_Idle",MaleIdleName = "Male_Idle";

    private bool Flip;
    private string TriggerTag;
    private float GravityScale;

    //Walk Control System: (Sistema de control para moviemiento)
    public bool Walk = true, Walking;
    public float WalkSpeed = 3.0f, WalkSpeedMultiplier = 1.0f;
    public float WalkDeadZone = 0.5f;
    public float WalkSmooth = 3.0f;
	public Stance StanceState = Stance.Standing;
	public float CrouchSpeed = 2.5f;
	public float DownSpeed = 1.25f;
    public Bounds[] StanceBounds = new Bounds[3];
    public string WalkName = "Walking";

    public bool Ladder;
    public float LadderUpSpeed = 1.0f;
    public float LadderDownSpeed = 2.0f;
    public string LadderTag = "None";
    public Vector2 LadderMultiplier = new Vector2(0,1);

    private float WalkVelocity = 0.0f;
    private int StanceTemp;
    private float LadderVelocity;

    //Jump Control System: (Control de salto)
    public bool Jump = true, Jumping, JumpOnAirReset = true;
    public float JumpHeight = 0.0f;
    public float JumpLenght = 1.0f;
    public int JumpOnAir = 0;
    public string JumpName = "Jumping";
    public Vector2 AirControl = new Vector2(0.1f,0.1f);

    private int JumpOnAirDelta = 0;
    private bool Grounded;

    //Running Control System: (Control de correr)
    public bool Run = true, Running;
    public float RunSpeed = 2.0f;
    public float RunSpeedSmooth = 2.0f;
    public float RunSpeedMultiply = 1.0f;
    public Vector2 RunSpeedDeadZone = new Vector2(3.0f,5.0f);
    public string RunName = "Running";
    
    private float RunVelocity = 0.0f;

    //Aiming Control System:
    public bool FaceToMouse;
    public bool LookAtMouse;
    public int LookAtMouseIdle;
    public List<int> LookAtMouseIndex;
    public Sprite Image;

    private Vector2 MouseWorld;
    private Vector2 HandLocation;
    private float HandRotation = 0.0f;
    private float OldHandRotation;

    //Components (Componentes):
    private Animator AnimatorComponent;
    private Rigidbody2D BodyComponent;
    private CapsuleCollider2D ColliderComponent;
    private Camera CameraComponent;
    private RuntimeAnimatorController RuntimeControllerComponent;

    void Awake() 
    {
        AnimatorComponent = GetComponent<Animator>();
        RuntimeControllerComponent = AnimatorComponent.runtimeAnimatorController;
        BodyComponent = GetComponent<Rigidbody2D>();
        ColliderComponent = GetComponent<CapsuleCollider2D>();
        CameraComponent = GameObject.FindObjectOfType<Camera>();
    }

    void Start() 
    {
        DeltaTime = 0.0f;
        GravityScale = BodyComponent.gravityScale;
        //OldHandRotation = GetComponentsInChildren<Transform>()[LookAtMouseIndex].transform.rotation.z;
    }

    void FixedUpdate()
    {
        WalkControl();
        RunControl();
        JumpControl();
        GeneralControl();
    }

    void Update()
    {
        DeltaTime += Time.deltaTime;
        if (FaceToMouse)
        {
            if ((CameraComponent.ScreenToWorldPoint(Input.mousePosition).x < transform.position.x)) 
            {
                Flip = true;
                GetPart("Body").localScale = new Vector3(-1, 1, 1);
                GetPart("Head").localScale = new Vector3(-1, 1, 1);
            }
            else 
            {
                Flip = false;
                GetPart("Body").localScale = new Vector3(1, 1, 1);
                GetPart("Head").localScale = new Vector3(1, 1, 1);
            }
        }

        if (!Walking && !Running && PlayerGender == Gender.Male)
            SetAnimationIdle(MaleIdleName, IdleIndex);
        else if (!Walking && !Running && PlayerGender == Gender.Female)
            SetAnimationIdle(FemaleIdleName, IdleIndex);

        
    }

    void LateUpdate()
    {
        if (LookAtMouse)
        {
            GetPart("FrontHand").transform.localPosition = new Vector3(-0.03f, 0.08f, 0);
            GetPart("FrontHand").GetComponent<SpriteRenderer>().sprite = Image;
            GetPart("BackHand").transform.localPosition = new Vector3(0.03f, 0.08f, 0);
            GetPart("BackHand").GetComponent<SpriteRenderer>().sprite = Image;
            MouseWorld = CameraComponent.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, CameraComponent.nearClipPlane));
            for (int i = 0; i < LookAtMouseIndex.Count; i++)
            {
                HandLocation = GetComponentsInChildren<Transform>()[LookAtMouseIndex[i]].transform.position;
                HandRotation = Mathf.Atan2(MouseWorld.y - HandLocation.y, MouseWorld.x - HandLocation.x);
                HandRotation = HandRotation * (180.0f / Mathf.PI);
                if (Flip)
                {
                    HandRotation -= 180;
                    HandRotation *= -1;
                }
                //GetComponentsInChildren<Transform>()[LookAtMouseIndex[i]].transform.localRotation = Quaternion.Euler(0, 0, HandRotation);
                GetComponentsInChildren<Transform>()[LookAtMouseIndex[i]].transform.localEulerAngles = new Vector3(0, 0, HandRotation);
                //GetComponentsInChildren<Transform>()[LookAtMouseIndex[i]].transform.Rotate(new Vector3(0, 0, HandRotation),Space.World);
            }
        }
        else
        {
            for (int i = 0; i < LookAtMouseIndex.Count; i++)
            {
                GetComponentsInChildren<Transform>()[LookAtMouseIndex[i]].transform.localRotation = Quaternion.identity;
            }
        }
        
        
        
    }

    private void WalkControl() 
    {
        //Walk Control System:
        WalkVelocity = Utils.FInterpTo(WalkVelocity, WalkSpeed * Input.GetAxisRaw("Horizontal"), WalkSmooth, Time.deltaTime);
        WalkVelocity *= System.Convert.ToInt32(Walk);
        WalkVelocity *= WalkSpeedMultiplier;
        if (!Grounded)
            WalkVelocity *= AirControl.x;
		switch (StanceState) 
		{
		case Stance.Standing:
			WalkVelocity = Mathf.Clamp (WalkVelocity, -WalkSpeed, WalkSpeed);
            StanceBounds[0].center = ColliderComponent.offset;
            ColliderComponent.offset = StanceBounds[0].center;
            StanceBounds[0].extents = ColliderComponent.size;
            ColliderComponent.size = StanceBounds[0].extents;
			break;
		case Stance.Crouched:
			WalkVelocity = Mathf.Clamp (WalkVelocity, -CrouchSpeed, CrouchSpeed);
            ColliderComponent.offset = StanceBounds[1].center;
            ColliderComponent.size = StanceBounds[1].extents;
			break;
		case Stance.Down:
			WalkVelocity = Mathf.Clamp (WalkVelocity, -DownSpeed, DownSpeed);
            ColliderComponent.offset = StanceBounds[2].center;
            ColliderComponent.size = StanceBounds[2].extents;
			break;
		}
        Walking = (Mathf.Abs(WalkVelocity) > WalkDeadZone);

        BodyComponent.gravityScale = GravityScale;
        if (Ladder) 
        {
            BodyComponent.gravityScale = 0;
            LadderVelocity = Utils.FInterpTo(LadderVelocity, Input.GetAxis("Vertical") * LadderUpSpeed * LadderDownSpeed, WalkSmooth, Time.deltaTime);
            LadderVelocity = Mathf.Clamp(LadderVelocity, -LadderDownSpeed, LadderUpSpeed);
            WalkVelocity *= LadderMultiplier.x; 
            LadderVelocity *= LadderMultiplier.y;
            BodyComponent.velocity = new Vector2( WalkVelocity, LadderVelocity );
        }
        
        if (Input.GetButton("Horizontal"))
            BodyComponent.velocity = new Vector2(WalkVelocity, BodyComponent.velocity.y);

        AnimatorComponent.SetBool(WalkName, Walking);
        if (Utils.Sign(WalkVelocity) < 0 && !FaceToMouse && Walking)
        { 
            Flip = true;
            GetPart("Body").localScale = new Vector3(-1, 1, 1);
            GetPart("Head").localScale = new Vector3(-1, 1, 1);
        }
        else if (Utils.Sign(WalkVelocity) > 0 && !FaceToMouse && Walking)
        {
            Flip = false;
            GetPart("Head").localScale = new Vector3(1, 1, 1);
            GetPart("Body").localScale = new Vector3(1, 1, 1);
        }
        
        if (Input.GetButtonDown("Stance")) 
        {
            StanceTemp = (int)Mathf.Clamp(StanceTemp + Input.GetAxisRaw("Stance"),3,5);
            StanceState = (Stance)StanceTemp;
        }
    }

    private void JumpControl()
    {
        //Jump Control System:
        if (Input.GetButtonDown("Jump") && Jump) 
        {
            if (Grounded)
            {
                Jumping = true;
                BodyComponent.AddForce( JumpHeight * Vector2.up ,ForceMode2D.Impulse);
                JumpOnAirDelta = JumpOnAir;
                Grounded = false;
            }
            else if (JumpOnAirDelta > 0)
            {
                BodyComponent.velocity *= System.Convert.ToInt32(!JumpOnAirReset);
                BodyComponent.AddForce(JumpHeight * Vector2.up, ForceMode2D.Impulse);
                JumpOnAirDelta--;
            }
        }
        AnimatorComponent.SetBool(JumpName, Jumping);
        
    }

    private void RunControl()
    {
        if (Input.GetButton("Run") && Run && StanceState == Stance.Standing)
        {
            RunVelocity = Utils.FInterpTo(RunVelocity, RunSpeed * Input.GetAxisRaw("Horizontal"), RunSpeedSmooth, Time.deltaTime);
            RunVelocity *= RunSpeedMultiply;
            Running = (RunVelocity > RunSpeedDeadZone.x && RunVelocity < RunSpeedDeadZone.y);
            BodyComponent.velocity = new Vector2(WalkVelocity + RunVelocity, BodyComponent.velocity.y);
        }
        AnimatorComponent.SetBool(RunName, Running);
    }

    private void GeneralControl()
    {
        if (Input.GetButtonDown("Dash") && Dash) {
            BodyComponent.AddForce(DashForce * Input.GetAxisRaw("Horizontal") * Vector2.right, ForceMode2D.Impulse);
        }

    }

    //Other Functions (otras funciones):
    public void SetAnimationIdle(string name, float time, int layer = 0)
    {
        if (AnimatorComponent != null)
        {
            AnimationClip selectedClip = new AnimationClip();
            foreach (AnimationClip c in RuntimeControllerComponent.animationClips)
            {
                if (c.name == name) { 
                    selectedClip = c; break; 
                }
            }
            AnimatorComponent.Play(Animator.StringToHash(name), layer, (time / selectedClip.length));

        }
    }

    public Transform GetPart(string name) 
    {
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.name == name)
                return t;
        }
        return null;
    }

    public bool GetFlip() { return Flip; }

    void OnCollisionEnter2D(Collision2D coll) 
    {
        Grounded = true;
        Jumping = false;
    }

    void OnCollisionStay2D(Collision2D coll) 
    {
        Grounded = true;
        Jumping = false;
    }
}
	