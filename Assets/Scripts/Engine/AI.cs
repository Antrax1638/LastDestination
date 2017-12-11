using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public enum AISearch
{
    None,
    Normal,
    Force,
}

public enum AIMode
{ 
    None = 3,
    Target,
    Location,

}

public enum AIControl
{
    Force = ForceMode2D.Force,
    Impulse = ForceMode2D.Impulse,
    Velocity,

}

public struct AIData 
{
    public Transform Target;
    public Vector3 Direction;
    public float Speed;
    public Vector3 DirNormalized;
    public AIControl ControlMode;
    public RaycastHit2D Hit;
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
public class AI : MonoBehaviour
{
    //Control Properties:
    public AIMode Mode = AIMode.Target;
    public Transform Target;
    public float UpdateRate = 2.0f;
    public float Speed = 300.0f;
    public Path Path;
    public AIControl ControlMode;
    public bool Jump;
    public float JumpHeight;
    public float JumpProbability;
    public float NextWaypontDistance = 3.0f;
    public bool Search = false;
    public bool SearchPlayer = false;
    public bool AutomaticSearch = false;
    public AISearch SearchMode = AISearch.Normal;
    public string SearchTag = "None";
    public float SearchRate = 1.0f;
    public float Length = 2.5f;
    public bool Memory = false;
    public float MemoryTime = 30.0f;
    public Vector3 Location;
    public bool SearchLocation;
    public float SearchLength = 15000.0f;

    [HideInInspector]
    public bool PathEnded = false;

    [HideInInspector]
    public bool LineOfSight = false;
    
    private float MemoryDelta = 0;
    private float DeltaTime = 0;
    protected Seeker SeekerComponent;
    protected Rigidbody2D BodyComponent;
    private int CurrentPathIndex = 0;
    protected Vector3 TargetLocation;
    

    protected AIData Data;
    protected RaycastHit2D Hit,JumpHit;
    protected float LookAtRotation = 0,JumpHeightValue,JumpDeltaTime;

    protected virtual void Awake()
    {
        SeekerComponent = GetComponent<Seeker>();
        BodyComponent = GetComponent<Rigidbody2D>();
        Data = new AIData();
    }

    protected virtual void Start()
    {
        if (Target == null && SearchPlayer && Mode == AIMode.Target || (SearchPlayer && SearchMode == AISearch.Force))
            StartCoroutine(UpdatePlayer());
        if (Target == null && Search && Mode == AIMode.Target || (Search && SearchMode == AISearch.Force))
            StartCoroutine(UpdateSearch());

        SeekerComponent.StartPath(transform.position, TargetLocation, OnPathComplete);
        StartCoroutine(UpdatePath());
    }

    IEnumerator UpdateSearch() 
    {
        switch (Mode)
        {
            case AIMode.Target:
                GameObject sResult = GameObject.FindGameObjectWithTag(SearchTag);
                if (sResult == null)
                {
                    yield return new WaitForSeconds(SearchRate);
                    StartCoroutine(UpdateSearch());
                }
                else
                {
                    //Search = false;
                    if (Vector3.Distance(sResult.transform.position, transform.position) <= SearchLength)
                    {
                        Search = false;
                        Target = sResult.transform;
                        TargetLocation = Target.position;
                        StartCoroutine(UpdatePath());
                    }
                }
            break;

            case AIMode.Location:
                Search = false;
                if (Vector3.Distance(Location, transform.position) <= SearchLength)
                    TargetLocation = Location;
                StartCoroutine(UpdatePath());
            break;
        }
        
    }

    IEnumerator UpdatePlayer()
    {
        switch (Mode)
        {
            case AIMode.Target:
                GameObject sResult = GameObject.FindGameObjectWithTag("Player");
                if (sResult == null)
                {
                    yield return new WaitForSeconds(SearchRate);
                    StartCoroutine(UpdatePlayer());
                }
                else
                {
                    if (Vector3.Distance(sResult.transform.position, transform.position) <= SearchLength)
                    {
                        SearchPlayer = false;
                        Target = sResult.transform;
                        TargetLocation = Target.position;
                        StartCoroutine(UpdatePath());
                    }
                    
                }
            break;

            case AIMode.Location:
                SearchPlayer = false;
                if (Vector3.Distance(Location, transform.position) <= SearchLength)
                    TargetLocation = Location;
                StartCoroutine(UpdatePath());
            break;
        }
        
       
    }

    IEnumerator UpdatePath()
    {
        if (Target == null && Mode == AIMode.Target)
            yield break;
        SearchLocation = false;

        switch (Mode)
        {
            case AIMode.Location: TargetLocation = Location; break;
            case AIMode.Target: TargetLocation = Target.position; break;
        }

        SeekerComponent.StartPath(transform.position, TargetLocation, OnPathComplete);
        yield return new WaitForSeconds(1.0f / UpdateRate);
        StartCoroutine(UpdatePath());
    }

    public void OnPathComplete(Path p) 
    {
        if (!p.error)
        {
            Path = p;
            CurrentPathIndex = 0;
        }
    }

    protected virtual void Update()
    {
        DeltaTime += Time.deltaTime;
        if (Memory && Target != null)
            MemoryDelta += Time.deltaTime;

        if (Memory && MemoryDelta > MemoryTime && !LineOfSight)
        {
            Target = null;
            MemoryDelta = 0.0f;
        }

        if (Target == null && SearchPlayer || (SearchPlayer && SearchMode == AISearch.Force))
            StartCoroutine(UpdatePlayer());
        if (Target == null && Search || (Search && SearchMode == AISearch.Force))
            StartCoroutine(UpdateSearch());
        
        Data.Target = Target;
        Data.ControlMode = ControlMode;

        if (Target == null && !Search && AutomaticSearch)
            Search = true;

        if (Target == null && Mode == AIMode.Location && SearchLocation)
            StartCoroutine(UpdatePath());
    }

    protected virtual void FixedUpdateCall() 
    {
        JumpDeltaTime += Time.fixedDeltaTime;
        float JumpForce = Mathf.Sqrt(Physics2D.gravity.magnitude * JumpHeight * 2);

        //JumpHit = Physics2D.Raycast(transform.position, Dir, 1);
        float Probability = Random.Range(0.0f, 100.0f);
        
        if (Jump && JumpDeltaTime > 1 && Probability < JumpProbability)
        {
            BodyComponent.AddForce(Data.DirNormalized * JumpForce, ForceMode2D.Impulse);
            JumpDeltaTime = 0;
        }
    }

    protected virtual void FixedUpdate()
    {
        FixedUpdateCall();
        if (Path == null || Target == null && Mode == AIMode.Target)
            return;
        if (CurrentPathIndex >= Path.vectorPath.Count)
        {
            if (PathEnded)
                return;

            PathEnded = true;
            return;
        }
        PathEnded = false;

        Data.Speed = Speed;
        Data.Direction = Path.vectorPath[CurrentPathIndex] - transform.position;
        Data.DirNormalized = Data.Direction.normalized;
        Data.DirNormalized *= Speed * Time.fixedDeltaTime;

        Vector2 difference = TargetLocation - transform.position;
        LookAtRotation = Mathf.Atan2(difference.x, difference.y) * Mathf.Rad2Deg;

        Hit = Physics2D.Raycast(transform.position, (TargetLocation - transform.position).normalized, Length);
        Data.Hit = Hit;
        if (Hit)
        {
            if (LineOfSight = (Hit.transform == Target))
            {
                MemoryDelta -= Mathf.Clamp(MemoryDelta, 0, MemoryTime);
            }
        }
   
        float Distance = Vector3.Distance(transform.position, Path.vectorPath[CurrentPathIndex]);
        if(Distance < NextWaypontDistance)
        {
            CurrentPathIndex++;
            return;
        }
    }

    void OnDisable()
    {
        SeekerComponent.pathCallback -= OnPathComplete;
    }

}
