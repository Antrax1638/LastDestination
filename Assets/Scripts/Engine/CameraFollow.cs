using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour 
{
    public Transform Target;
    public float Delay = 0.5f;
    public float SmoothSpeed = 0.125f;
    public float SmoothSpeedOffset = 0.1f;
    public Vector3 Offset = new Vector3(0,0,-5);
    public Vector2 Length = new Vector2(4.0f, 2.0f);
   
    private float DelayDelta = 0.0f;
    private int Touch = 0;
    private Camera CameraComponent;
    
    void Awake() {
        CameraComponent = GetComponent<Camera>();
    }

    void FixedUpdate()
    {
        if (Input.GetButton("CameraOffset") && DelayDelta > Time.deltaTime * 2) 
        {
            float x = CameraComponent.ScreenToViewportPoint(Input.mousePosition).x - 0.5f;
            float y = CameraComponent.ScreenToViewportPoint(Input.mousePosition).y - 0.5f;
            x *= Length.x; y *= Length.y;
            
            Vector3 MouseWorldCoords = new Vector3(x,y,Offset.z);
            Offset = Vector3.Lerp(Offset, MouseWorldCoords, SmoothSpeedOffset);
        }

        if (Input.GetButtonDown("CameraOffset"))
            Touch++;

        if (Touch == 1)
            DelayDelta += Time.deltaTime;
        else if (Touch >= 1 && DelayDelta > Delay)
        {
            Touch = 0; DelayDelta = 0.0f; 
        }
        else if (Touch >= 2)
        {
            Offset = new Vector3(0, 0, Offset.z);
            Touch = 0;
            DelayDelta = 0.0f;
        }

        Vector3 TargetOffset = Target.position + Offset;
        transform.position = Vector3.Lerp(transform.position, TargetOffset, SmoothSpeed);
    }

}
