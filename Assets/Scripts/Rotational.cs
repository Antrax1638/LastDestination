using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotational : MonoBehaviour {

    public float Speed;
	
	public float deltaRot = 0.0f;

	void Update () {
        deltaRot += Speed;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, deltaRot));
        if (deltaRot > 360)
            deltaRot = 0;
	}
}
