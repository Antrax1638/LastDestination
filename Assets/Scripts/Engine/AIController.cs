using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIType
{
    None,
    Ranged,
    Melee,
    Other,
}

public class AIController : AI
{
    public float AttackRate = 200.0f;
    public float AttackLength = 1000.0f;
    public GameObject Prefab;
    public float AttackSpeed = 15.0f;
    public AIType Type = AIType.Ranged;
    
    protected Animator AnimatorComponent;
    private float AttackDelta = 0;
    private float TargetDistance;
    
    protected override void Awake()
    {
        base.Awake();
        AnimatorComponent = GetComponent<Animator>();
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
        AttackDelta += Time.fixedDeltaTime;
        TargetDistance = Vector3.Distance(TargetLocation, transform.position);

        if (LineOfSight && AttackDelta >= ((AttackRate / 60) / 60) && TargetDistance <= AttackLength)
        {
            OnAttack();
        }
    }

    protected virtual void OnAttack()
    {
        switch (Type)
        {
            default: break;

            case AIType.Ranged:
                var FiredProjectile = Instantiate(Prefab, transform.position, transform.rotation).GetComponent<Projectile>();
                Physics2D.IgnoreCollision(FiredProjectile.GetComponent<BoxCollider2D>(), GetCurrentCollider());
                FiredProjectile.Direction = TargetLocation - transform.position;
                FiredProjectile.Speed = AttackSpeed;
                FiredProjectile.Launch();
                AttackDelta = 0;
            break;

            case AIType.Melee:
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
}
