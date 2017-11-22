using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public enum IASearch
{
    None,
    Normal,
    Force,
}

public enum IAMode
{ 
    None = 3,
    Target,
    Location,

}

public struct IAData 
{
    public Transform Target;
    public Vector3 Direction;
    public float Speed;
    public Vector3 DirNormalized;
    public ForceMode2D ForceMode;
    public RaycastHit2D Hit;
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
public class IA : MonoBehaviour
{
    //Control Properties:
    public IAMode Mode = IAMode.Target;
    public Transform Target;
    public float UpdateRate = 2.0f;
    public float Speed = 300.0f;
    public Path Path;
    public ForceMode2D FMode;
    public float NextWaypontDistance = 3.0f;
    public bool Search = false;
    public bool SearchPlayer = false;
    public bool AutomaticSearch = false;
    public IASearch SMode = IASearch.Normal;
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
    private Seeker SeekerComponent;
    private Rigidbody2D BodyComponent;
    private int CurrentPathIndex = 0;
    private Vector3 TargetLocation;
    

    protected IAData Data;
    protected RaycastHit2D Hit;

    protected virtual void Awake()
    {
        SeekerComponent = GetComponent<Seeker>();
        BodyComponent = GetComponent<Rigidbody2D>();
        Data = new IAData();
    }

    protected virtual void Start()
    {
        if (Target == null && SearchPlayer && Mode == IAMode.Target || (SearchPlayer && SMode == IASearch.Force))
            StartCoroutine(UpdatePlayer());
        if (Target == null && Search && Mode == IAMode.Target  || (Search && SMode == IASearch.Force))
            StartCoroutine(UpdateSearch());

        ApplyMode();

        SeekerComponent.StartPath(transform.position, TargetLocation, OnPathComplete);
        StartCoroutine(UpdatePath());
    }

    IEnumerator UpdateSearch() 
    {
        switch (Mode)
        {
            case IAMode.Target:
                GameObject sResult = GameObject.FindGameObjectWithTag("Player");
                if (sResult == null)
                {
                    yield return new WaitForSeconds(SearchRate);
                    StartCoroutine(UpdatePlayer());
                }
                else
                {
                    Search = false;
                    if (Vector3.Distance(sResult.transform.position, transform.position) <= SearchLength)
                    {
                        Target = sResult.transform;
                        StartCoroutine(UpdatePath());
                    }
                }
            break;

            case IAMode.Location:
                Search = false;
                if (Vector3.Distance(Location, transform.position) <= SearchLength)
                    TargetLocation = Location;
            break;
        }
        
    }

    IEnumerator UpdatePlayer()
    {
        switch (Mode)
        {
            case IAMode.Target:
                GameObject sResult = GameObject.FindGameObjectWithTag("Player");
                if (sResult == null)
                {
                    yield return new WaitForSeconds(SearchRate);
                    StartCoroutine(UpdatePlayer());
                }
                else
                {
                    SearchPlayer = false;
                    if (Vector3.Distance(sResult.transform.position, transform.position) <= SearchLength)
                    {
                        Target = sResult.transform;
                        StartCoroutine(UpdatePath());
                    }
                }
            break;
            case IAMode.Location:
                if (Vector3.Distance(Location, transform.position) <= SearchLength)
                    TargetLocation = Location;
            break;
        }
        
       
    }

    IEnumerator UpdatePath()
    {
        if (Target == null && Mode == IAMode.Target)
            yield break;
        SearchLocation = false;
        ApplyMode();
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

        if (Target == null && SearchPlayer || (SearchPlayer && SMode == IASearch.Force))
            StartCoroutine(UpdatePlayer());
        if (Target == null && Search || (Search && SMode == IASearch.Force))
            StartCoroutine(UpdateSearch());
        
        Data.Target = Target;
        Data.ForceMode = FMode;

        if (Target == null && !Search && AutomaticSearch)
            Search = true;

        if (Target == null && Mode == IAMode.Location && SearchLocation)
            StartCoroutine(UpdatePath());
    }

    protected virtual void FixedUpdate()
    {
        if (Path == null || Target == null && Mode == IAMode.Target)
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

        Hit = Physics2D.Raycast(transform.position, (TargetLocation - transform.position).normalized, Length);
        Data.Hit = Hit;
        if (Hit)
        {
            LineOfSight = (Hit.transform == Target);
        }

        //Hardcodeo:
        BodyComponent.AddForce(Data.DirNormalized, FMode);
        
        float Distance = Vector3.Distance(transform.position, Path.vectorPath[CurrentPathIndex]);
        if(Distance < NextWaypontDistance)
        {
            CurrentPathIndex++;
            return;
        }
    }

    private void ApplyMode()
    {
        switch (Mode)
        {
            default: break;
            case IAMode.None: TargetLocation = new Vector3(); break;
            case IAMode.Target: TargetLocation = Target.position; break;
            case IAMode.Location: TargetLocation = Location; break;
        }
    }

    void OnDisable()
    {
        SeekerComponent.pathCallback -= OnPathComplete;
    }

    //Hardcodeo:
    void OnCollisionEnter2D(Collision2D coll) {
        if (coll.gameObject.tag == SearchTag)
            Destroy(coll.gameObject);
    }
}
