using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AIController))]
public class AIControllerEditor : Editor 
{
    AIController AIC;
    int Tab;

    void OnEnable()
    {
        AIC = (AIController)target;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        Tab = GUILayout.Toolbar(Tab, new string[] { "Basic" , "Advanced" });
        switch (Tab)
        {
            default: break;
            case 0: BasicControls(); break;
            case 1: AdvancedControls(); break;
            
        }
    }

    void BasicControls()
    {
        AIC.Mode = (AIMode)EditorGUILayout.EnumPopup("Mode", AIC.Mode);
        if (AIC.Mode == AIMode.Target)
            AIC.Target = EditorGUILayout.ObjectField("Target", AIC.Target, typeof(Transform),true) as Transform;
        else
            AIC.Location = EditorGUILayout.Vector3Field("Target Location", AIC.Location);
        AIC.UpdateRate = EditorGUILayout.FloatField("Update Rate", AIC.UpdateRate);
        AIC.NextWaypontDistance = EditorGUILayout.FloatField("Next Waypoint Distance", AIC.NextWaypontDistance);
        GUILayout.Space(5);

        GUILayout.Label("Memory System");
        AIC.Memory = EditorGUILayout.Toggle("Memory", AIC.Memory);
        AIC.MemoryTime = EditorGUILayout.FloatField("Memory Time", AIC.MemoryTime);
        GUILayout.Space(5);

        GUILayout.Label("Search System");
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (AIC.Mode == AIMode.Target)
        {
            GUILayout.Label("Search");
            AIC.Search = EditorGUILayout.Toggle(AIC.Search);
            GUILayout.Label("Search Player");
            AIC.SearchPlayer = EditorGUILayout.Toggle(AIC.SearchPlayer);
            GUILayout.Label("Auto Search");
            AIC.AutomaticSearch = EditorGUILayout.Toggle(AIC.AutomaticSearch);
        }
        else
            AIC.SearchLocation = EditorGUILayout.Toggle("Search Location", AIC.SearchLocation);
        GUILayout.EndHorizontal();
        AIC.SearchMode = (AISearch)EditorGUILayout.EnumPopup("Search Mode", AIC.SearchMode);
        AIC.SearchRate = EditorGUILayout.FloatField("Search Rate", AIC.SearchRate);
        AIC.SearchLength = EditorGUILayout.FloatField("Search Lenght", AIC.SearchLength);
        AIC.SearchTag = EditorGUILayout.TextField("Search Tag", AIC.SearchTag);
        GUILayout.EndVertical();
        GUILayout.Space(5);

        

    }

    void AdvancedControls()
    {
        AIC.ControlMode = (AIControl)EditorGUILayout.EnumPopup("Control Mode", AIC.ControlMode);
        AIC.Speed = EditorGUILayout.FloatField("Speed", AIC.Speed);

        GUILayout.Label("Attack System");
        AIC.Type = (AIType)EditorGUILayout.EnumPopup("Type", AIC.Type);
        switch (AIC.Type) {
            case AIType.Ranged: 
                AIC.Prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", AIC.Prefab, typeof(GameObject), true);
                AIC.AttackLength = EditorGUILayout.FloatField("Attack Length", AIC.AttackLength);
                break;
        }
        
        AIC.AttackRate = EditorGUILayout.FloatField("Attack Rate", AIC.AttackRate);
        AIC.Length = EditorGUILayout.FloatField("Sight Length", AIC.Length);

    }

}
