using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour
{
    public bool Scrolling, Smooth, Fixed;
    //public float ParalaxSpeed;
    public Vector2 ParalaxSpeed;
    public float ViewSize;


    private Transform CameraTransform;
    private Transform[] Layers;
    private int LeftIndex, RightIndex;
    private float Width,LastCameraX,LastCameraY;

	void Start () 
    {
        CameraTransform = Camera.main.transform;
        LastCameraX = CameraTransform.position.x;

        Layers = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            Layers[i] = transform.GetChild(i);
        LeftIndex = 0;
        RightIndex = Layers.Length - 1;

        Width = Layers[0].GetComponent<SpriteRenderer>().bounds.size.x;
	}

    void Update() 
    {
        if (Fixed) 
        {
            for (int i = 0; i < Layers.Length; i++)
            {
                Layers[i].position = new Vector3(Layers[i].position.x, CameraTransform.position.y, Layers[i].position.z);
            }
        }
            
        
        if (Smooth)
        {
            float DeltaY = CameraTransform.position.y - LastCameraY;
            float DeltaX = CameraTransform.position.x - LastCameraX;
            transform.position += new Vector3(DeltaX * ParalaxSpeed.x, DeltaY * ParalaxSpeed.y * System.Convert.ToInt32(!Fixed), 0);
        }
        
        LastCameraX = CameraTransform.position.x;
        LastCameraY = CameraTransform.position.y;

        if (Scrolling) 
        {
            if (CameraTransform.position.x < (Layers[LeftIndex].position.x + ViewSize))
                ScrollLeft();

            if (CameraTransform.position.x > (Layers[RightIndex].position.x - ViewSize))
                ScrollRight();
        }

        
    }

    void ScrollLeft()
    {
        int lastRight = RightIndex;
        Layers[RightIndex].position = new Vector3((Layers[LeftIndex].position.x - Width), Layers[LeftIndex].position.y, 0);
        LeftIndex = RightIndex;
        RightIndex--;
        if (RightIndex < 0)
            RightIndex = Layers.Length - 1;
    }

    void ScrollRight()
    {
        int lastLeft = LeftIndex;
        Layers[LeftIndex].position = new Vector3((Layers[RightIndex].position.x + Width), Layers[RightIndex].position.y, 0);
        RightIndex = LeftIndex;
        LeftIndex++;
        if (LeftIndex == Layers.Length)
            LeftIndex = 0;
    }
	
}
