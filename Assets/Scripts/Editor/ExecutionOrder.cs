using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*public class ExecutionOrder : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}*/

class ExecutionOrder : EditorWindow 
{
    [MenuItem("Window/Execution Order (WIP)")]
    

    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(ExecutionOrder));
        
    }
    
    void OnGui() {
        //GUI.Label(new Rect(0, 0, 100, 50), "Script Execution Order");
    }
}
