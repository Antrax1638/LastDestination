using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIAttackType
{
    None,
    Ranged,
    Melee,
    Other,
}

public enum AIAttackMode
{ 
    Artillery,
    Shoot,
}


/*
public struct AIState
{
    public enum States
    {
        Less,
        LessOrEqual,
        Equal,
        Greater,
        GreaterOrEqual,
    }

    public int Length;
    private List<int> Constant;
    private List<States> StateComparision;

    public void Init()
    {
        Constant = new List<int>();
        StateComparision = new List<States>();

    }

    public void AddState(int Constant, States St)
    {
        if (!this.Constant.Contains(Constant) && !this.StateComparision.Contains(St)) 
        {
            this.Constant.Add(Constant);
            this.StateComparision.Add(St);
        }
        Length = this.Constant.Count;
    }

    public void RemoveState(int index)
    {
        if (index < 0 || index >= Constant.Count || index >= StateComparision.Count)
            return;

        this.Constant.RemoveAt(index);
        this.StateComparision.RemoveAt(index);
        Length = this.Constant.Count;
    }

    public bool GetState(int index,int value) 
    {
        switch (StateComparision[index])
        {
            default: return false;
            case States.Less: return (value < Constant[index]);
            case States.LessOrEqual: return (value <= Constant[index]);
            case States.Equal: return (value == Constant[index]);
            case States.Greater: return (value > Constant[index]);
            case States.GreaterOrEqual: return (value >= Constant[index]);
        }
    }

}*/

public class AIController : AI
{
    //Attack:
    public bool Attack;
    public float AttackRate = 200.0f;
    public float AttackLength = 1000.0f;
    public GameObject Prefab;
    public float AttackSpeed = 15.0f;
    public AIAttackType AType = AIAttackType.Ranged;
    public AIAttackMode AMode = AIAttackMode.Shoot;
    public float Accuracy = 1;
    
    protected Animator AnimatorComponent;
    protected float HighAngle, LowAngle;
    private float AttackDelta = 0;
    private float TargetDistance;
    
    //State:
    public int Aggression;
    public List<int> AggressionTable;

    protected override void Awake()
    {
        base.Awake();
        AnimatorComponent = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();
        //Hardcodeo a full:
        AnimatorComponent.SetBool("Flying", true);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        switch (ControlMode)
        {
            default: BodyComponent.AddForce(Data.DirNormalized, (ForceMode2D)ControlMode); break;
            case AIControl.Velocity: BodyComponent.velocity = Vector2.Lerp(BodyComponent.velocity, Data.DirNormalized, Time.fixedDeltaTime); break;
        }

    }

    protected override void FixedUpdateCall()
    {
        base.FixedUpdateCall();

        AttackDelta += Time.fixedDeltaTime; 
        TargetDistance = Vector3.Distance(TargetLocation, transform.position);
        
        if (LineOfSight && AttackDelta >= ((AttackRate / 60) / 60) && Attack)
        {
            OnAttack();
        }
    }

    protected virtual void OnAttack()
    {
        Vector2 Direction = new Vector2();
        Vector2 AltDirection = new Vector2(); 
        
        LowAngle = Utils.GetLaunchAngle(TargetLocation, transform.position, AttackSpeed, Physics2D.gravity.y).y;
        HighAngle = Utils.GetLaunchAngle(TargetLocation, transform.position, AttackSpeed, Physics2D.gravity.y).x;

        switch (AMode)
        {
            case AIAttackMode.Artillery: Direction = new Vector2(Mathf.Cos(HighAngle), Mathf.Sin(HighAngle)); break;
            case AIAttackMode.Shoot: Direction = TargetLocation - transform.position; break;
        }

        AltDirection.x = Direction.x + Random.Range(-1.0f, 1.0f);
        AltDirection.y = Direction.y + Random.Range(-1.0f, 1.0f);

        switch (AType)
        {
            default: break;

            case AIAttackType.Ranged:
                if (TargetDistance <= AttackLength) 
                { 
                    var FiredProjectile = Instantiate(Prefab, transform.position, transform.rotation).GetComponent<Projectile>();
                    Physics2D.IgnoreCollision(FiredProjectile.GetComponent<BoxCollider2D>(), GetComponent<Collider2D>());
                    Direction = Vector2.Lerp(AltDirection, Direction, Accuracy);
                    FiredProjectile.Direction = Direction;
                    FiredProjectile.Speed = AttackSpeed;
                    FiredProjectile.Launch();
                    AttackDelta = 0;
                }
            break;

            case AIAttackType.Melee:
            if (Target != null)
            {
                print("melee");
            }
            break;
        }
    }

    private Collider2D GetCurrentCollider()
    {
        if (GetComponent<BoxCollider2D>() != null)
            return GetComponent<BoxCollider2D>();

        if (GetComponent<CapsuleCollider2D>() != null)
            return GetComponent<CapsuleCollider2D>();

        if (GetComponent<CircleCollider2D>() != null)
            return GetComponent<CircleCollider2D>();

        if (GetComponent<CompositeCollider2D>() != null)
            return GetComponent<CompositeCollider2D>();

        if (GetComponent<EdgeCollider2D>() != null)
            return GetComponent<EdgeCollider2D>();

        if (GetComponent<PolygonCollider2D>() != null)
            return GetComponent<PolygonCollider2D>();

        return null;
    }

    void ComputeState()
    {
        AggressionTable[Target.transform.gameObject.layer] = Mathf.Clamp(AggressionTable[Target.transform.gameObject.layer], -100, 100);

        if (AggressionTable[Target.transform.gameObject.layer] >= -100)
        {  }
        if (AggressionTable[Target.transform.gameObject.layer] > -50)
        {  }
        if (AggressionTable[Target.transform.gameObject.layer] >= 0)
        {  }
        if (AggressionTable[Target.transform.gameObject.layer] > 50)
        {  }
        if(AggressionTable[Target.transform.gameObject.layer] >= 100)
        {  }
    }

}
